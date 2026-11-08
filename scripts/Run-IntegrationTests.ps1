[CmdletBinding()]
param(
    [string] $RimWorldDir = $env:RimWorldDir,
    [string] $HarmonyPath = '',
    [ValidateRange(1, 3600)]
    [int] $TimeoutSeconds = 240
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if (!$RimWorldDir) {
    $RimWorldDir = 'S:\SteamLibrary\steamapps\common\RimWorld'
    if (!(Test-Path -LiteralPath $RimWorldDir -PathType Container)) {
        $RimWorldDir = Join-Path ([Environment]::GetFolderPath('ProgramFilesX86')) 'Steam/steamapps/common/RimWorld'
    }
}
$RimWorldDir = (Resolve-Path -LiteralPath $RimWorldDir).Path
$exe = Join-Path $RimWorldDir 'RimWorldWin64.exe'
if (!(Test-Path -LiteralPath $exe -PathType Leaf)) { throw "RimWorld executable missing: $exe" }
if (Get-Process RimWorldWin64 -ErrorAction SilentlyContinue) { throw 'Close RimWorld before running integration tests.' }

$root = Split-Path $PSScriptRoot -Parent
[xml] $modMetadata = Get-Content -LiteralPath (Join-Path $root 'About/About.xml') -Raw
[xml] $testMetadata = Get-Content -LiteralPath (Join-Path $root 'Tests/SmokeMod/About/About.xml') -Raw
$modId = $modMetadata.ModMetaData.packageId.ToLowerInvariant()
$testId = $testMetadata.ModMetaData.packageId.ToLowerInvariant()
& (Join-Path $PSScriptRoot 'Build-Mod.ps1') -RimWorldDir $RimWorldDir -HarmonyPath $HarmonyPath -IncludeTests
& (Join-Path $PSScriptRoot 'Deploy.ps1') -RimWorldDir $RimWorldDir -SkipBuild

$runId = [guid]::NewGuid().ToString('N')
$testData = Join-Path $root ".test-data\$runId"
$config = Join-Path $testData 'Config'
New-Item -ItemType Directory -Path $config -Force | Out-Null
$version = (Get-Content -LiteralPath (Join-Path $RimWorldDir 'Version.txt') -Raw).Trim()
@"
<ModsConfigData>
  <version>$version</version>
  <activeMods>
    <li>brrainz.harmony</li>
    <li>ludeon.rimworld</li>
    <li>$modId</li>
    <li>$testId</li>
  </activeMods>
  <knownExpansions>
    <li>ludeon.rimworld.royalty</li>
    <li>ludeon.rimworld.ideology</li>
    <li>ludeon.rimworld.biotech</li>
    <li>ludeon.rimworld.anomaly</li>
    <li>ludeon.rimworld.odyssey</li>
  </knownExpansions>
</ModsConfigData>
"@ | Set-Content -LiteralPath (Join-Path $config 'ModsConfig.xml') -Encoding UTF8
@'
<PrefsData>
  <devMode>true</devMode>
  <runInBackground>true</runInBackground>
  <pauseOnLoad>true</pauseOnLoad>
  <volumeGame>0</volumeGame>
  <volumeMusic>0</volumeMusic>
</PrefsData>
'@ | Set-Content -LiteralPath (Join-Path $config 'Prefs.xml') -Encoding UTF8

$report = Join-Path $testData 'Results.txt'
$log = Join-Path $testData 'Player.log'
$testTarget = Join-Path $RimWorldDir "Mods\PutAPinInItTests-$runId"
try {
    foreach ($file in 'About/About.xml', 'Assemblies/PutAPinInIt.Tests.dll') {
        $target = Join-Path $testTarget $file
        New-Item -ItemType Directory -Path (Split-Path $target -Parent) -Force | Out-Null
        Copy-Item -LiteralPath (Join-Path $root "Tests/SmokeMod/$file") -Destination $target
    }

    # Vanilla texture atlases require graphics initialization even in batch mode.
    $arguments = @('-batchmode', '-quicktest', '-putapinit-tests', "-savedatafolder=`"$testData`"", '-logFile', "`"$log`"")
    $process = Start-Process -FilePath $exe -WorkingDirectory $RimWorldDir -ArgumentList $arguments -WindowStyle Hidden -PassThru
    try {
        if (!$process.WaitForExit($TimeoutSeconds * 1000)) {
            Stop-Process -InputObject $process
            $process.WaitForExit()
            throw "Tests timed out. Inspect $log"
        }
        if (!(Test-Path -LiteralPath $report -PathType Leaf)) {
            throw "RimWorld exited without a test report. Inspect $log"
        }
        $results = Get-Content -LiteralPath $report
        $results | Write-Output
        if ($process.ExitCode -ne 0 -or $results -match '^FAIL' -or $results[-1] -ne 'PASS Real save and reload preserve the remaining pin') {
            throw "Integration tests failed. Inspect $report and $log"
        }
        Write-Host "Test report: $report"
    } finally {
        $process.Dispose()
    }
} finally {
    if (Test-Path -LiteralPath $testTarget -PathType Container) {
        $resolvedTarget = (Resolve-Path -LiteralPath $testTarget).Path
        if ($resolvedTarget -eq $testTarget) { Remove-Item -LiteralPath $resolvedTarget -Recurse -Force }
    }
}
