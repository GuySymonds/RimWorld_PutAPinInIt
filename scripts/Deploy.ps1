[CmdletBinding()]
param(
    [string] $RimWorldDir = '',
    [string] $ModsDir = '',
    [switch] $SkipBuild
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$project = Join-Path $root 'Source/PutAPinInIt.csproj'
$assembly = Join-Path $root 'Assemblies/PutAPinInIt.dll'

if ([string]::IsNullOrWhiteSpace($RimWorldDir)) {
    $steamDefault = Join-Path ([Environment]::GetFolderPath('ProgramFilesX86')) 'Steam/steamapps/common/RimWorld'
    if (Test-Path -LiteralPath $steamDefault -PathType Container) {
        $RimWorldDir = $steamDefault
    } else {
        throw 'RimWorld not found at the default Steam location. Supply -RimWorldDir "D:\SteamLibrary\steamapps\common\RimWorld".'
    }
}
$RimWorldDir = (Resolve-Path -LiteralPath $RimWorldDir -ErrorAction Stop).Path

$managed = Join-Path $RimWorldDir 'RimWorldWin64_Data/Managed/Assembly-CSharp.dll'
if (-not (Test-Path -LiteralPath $managed -PathType Leaf)) {
    throw "Not a valid Windows RimWorld installation: missing $managed"
}

if ([string]::IsNullOrWhiteSpace($ModsDir)) {
    $ModsDir = Join-Path $RimWorldDir 'Mods'
}
if (-not (Test-Path -LiteralPath $ModsDir -PathType Container)) {
    throw "Mods directory not found: $ModsDir (pass -ModsDir if necessary)."
}

$destination = Join-Path ((Resolve-Path -LiteralPath $ModsDir).Path) 'PutAPinInIt'
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
    if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
        throw 'dotnet SDK not found. Install a supported .NET SDK, or use -SkipBuild with an already built DLL.'
    }
    & dotnet build $project --configuration Release "-p:RimWorldDir=$RimWorldDir"
    if ($LASTEXITCODE -ne 0) { throw "Build failed (exit code $LASTEXITCODE). Deployment aborted." }
}

if (-not (Test-Path -LiteralPath $assembly -PathType Leaf)) {
    throw "Missing compiled DLL: $assembly. Build first, or remove -SkipBuild."
}

foreach ($folder in @('About', 'Textures', 'Assemblies')) {
    $source = Join-Path $root $folder
    if (-not (Test-Path -LiteralPath $source -PathType Container)) {
        throw "Missing runtime folder: $source"
    }
}
New-Item -ItemType Directory -Path $destination -Force | Out-Null
foreach ($folder in @('About', 'Textures', 'Assemblies')) {
    $source = Join-Path $root $folder
    $target = Join-Path $destination $folder
    New-Item -ItemType Directory -Path $target -Force | Out-Null
    Copy-Item -Path (Join-Path $source '*') -Destination $target -Recurse -Force
}
Write-Host "Deployed Put A Pin In It to: $destination"
Write-Host 'Restart RimWorld if it is running, then enable the mod in the Mods menu.'
