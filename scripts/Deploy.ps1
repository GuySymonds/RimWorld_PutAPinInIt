[CmdletBinding()]
param(
    [string] $RimWorldDir = '',
    [string] $ModsDir = '',
    [switch] $SkipBuild,
    [string] $HarmonyPath = ''
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$files = @('About/About.xml', 'Textures/PutAPinInIt/FlagPin.png', 'Assemblies/PutAPinInIt.dll', 'LICENSE')

if (!$ModsDir -and !$RimWorldDir) {
    $RimWorldDir = $env:RimWorldDir
}
if (!$ModsDir -and !$RimWorldDir) {
    $steamDefault = 'S:\SteamLibrary\steamapps\common\RimWorld'
    if (!(Test-Path -LiteralPath $steamDefault -PathType Container)) {
        $steamDefault = Join-Path ([Environment]::GetFolderPath('ProgramFilesX86')) 'Steam/steamapps/common/RimWorld'
    }
    $RimWorldDir = $steamDefault
}
if (!$ModsDir) {
    if (!(Test-Path -LiteralPath (Join-Path $RimWorldDir 'RimWorldWin64.exe') -PathType Leaf)) {
        throw "RimWorld not found at $RimWorldDir. Supply -RimWorldDir."
    }
    $ModsDir = Join-Path $RimWorldDir 'Mods'
    if (!(Test-Path -LiteralPath $ModsDir -PathType Container)) {
        throw "Mods directory not found: $ModsDir (pass -ModsDir if necessary)."
    }
}

$destination = Join-Path $ModsDir 'PutAPinInIt'
$existingAbout = Join-Path $destination 'About/About.xml'
if ((Test-Path -LiteralPath $destination) -and -not (Test-Path -LiteralPath $existingAbout)) {
    throw "Refusing to write over an unrecognised mod directory: $destination"
}
if (Test-Path -LiteralPath $existingAbout) {
    [xml]$existing = Get-Content -LiteralPath $existingAbout -Raw
    if ($existing.ModMetaData.packageId -ne 'GuySymonds.PutAPinInIt') {
        throw "Refusing to overwrite a different mod at $destination"
    }
}

if (-not $SkipBuild) {
    if ($RimWorldDir) {
        $RimWorldDir = (Resolve-Path -LiteralPath $RimWorldDir).Path
    }
    & (Join-Path $PSScriptRoot 'Build-Mod.ps1') -RimWorldDir $RimWorldDir -HarmonyPath $HarmonyPath
}

foreach ($file in $files) {
    if (!(Test-Path -LiteralPath (Join-Path $root $file) -PathType Leaf)) {
        throw "Missing mod file: $file"
    }
}
foreach ($file in $files) {
    $target = Join-Path $destination $file
    New-Item -ItemType Directory -Path (Split-Path $target -Parent) -Force | Out-Null
    Copy-Item -LiteralPath (Join-Path $root $file) -Destination $target -Force
}
Write-Host "Deployed Put A Pin In It to: $destination"
