[CmdletBinding()]
param(
    [string] $RimWorldDir = '',
    [string] $HarmonyPath = '',
    [switch] $IncludeTests,
    [switch] $Package
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$root = Split-Path $PSScriptRoot -Parent
$project = if ($IncludeTests) { 'Tests/SmokeTests.csproj' } else { 'Source/PutAPinInIt.csproj' }
$buildArgs = @('build', (Join-Path $root $project), '--configuration', 'Release')
if ($RimWorldDir) { $buildArgs += "-p:RimWorldDir=$RimWorldDir" }
if ($HarmonyPath) { $buildArgs += "-p:HarmonyPath=$HarmonyPath" }
& dotnet @buildArgs
if ($LASTEXITCODE -ne 0) { throw "Build failed (exit code $LASTEXITCODE)." }

if ($Package) {
    [xml] $metadata = Get-Content -LiteralPath (Join-Path $root 'About/About.xml') -Raw
    $dist = Join-Path $root 'dist'
    New-Item -ItemType Directory -Path $dist -Force | Out-Null
    $stage = Join-Path $dist ([guid]::NewGuid().ToString())
    try {
        & (Join-Path $PSScriptRoot 'Deploy.ps1') -ModsDir $stage -SkipBuild
        $archive = Join-Path $dist "PutAPinInIt-$($metadata.ModMetaData.modVersion).zip"
        Compress-Archive -LiteralPath (Join-Path $stage 'PutAPinInIt') -DestinationPath $archive -Force
        Write-Host "Packaged $archive"
    } finally {
        if (Test-Path -LiteralPath $stage -PathType Container) {
            $resolvedStage = (Resolve-Path -LiteralPath $stage).Path
            if ($resolvedStage -eq $stage) { Remove-Item -LiteralPath $resolvedStage -Recurse -Force }
        }
    }
}
