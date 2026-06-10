# OpenXR Runtime Switcher - Installer Build Script
# Builds the application and creates an NSIS installer

param(
    [switch]$SkipBuild,
    [switch]$SkipInstaller,
    [switch]$SkipZip,
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "OpenXR Runtime Switcher Installer Build" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$projectRoot = $PSScriptRoot
$projectFile = Join-Path $projectRoot "src\OpenXRRuntimeSwitcher\OpenXRRuntimeSwitcher.csproj"
$publishDir = Join-Path $projectRoot "publish"
$installerScript = Join-Path $projectRoot "src\OpenXRRuntimeSwitcher\installer\OpenXRRuntimeSwitcher.nsi"

# Extract version from csproj
Write-Host "Reading version from project file..." -ForegroundColor Gray
[xml]$csproj = Get-Content $projectFile
$version = $csproj.Project.PropertyGroup.Version
if (-not $version) {
    Write-Host "Warning: No version found in csproj, using 0.0.0" -ForegroundColor Yellow
    $version = "0.0.0"
}
Write-Host "  Version: $version" -ForegroundColor Cyan
Write-Host ""

$installerOutput = Join-Path $publishDir "OpenXRRuntimeSwitcher-Setup-$version.exe"
$zipOutput = Join-Path $publishDir "OpenXRRuntimeSwitcher-$version.zip"

# Step 1: Build and publish the application
if (-not $SkipBuild) {
    Write-Host "[1/3] Building and publishing application..." -ForegroundColor Yellow

    if (Test-Path $publishDir) {
        Write-Host "  Cleaning existing publish directory..." -ForegroundColor Gray
        Remove-Item $publishDir -Recurse -Force
    }

    Write-Host "  Running dotnet publish..." -ForegroundColor Gray
    & dotnet publish $projectFile `
        -c $Configuration `
        -r win-x64 `
        -o $publishDir `
        --self-contained true `
        /p:PublishSingleFile=true `
        /p:IncludeAllContentForSelfExtract=true

    if ($LASTEXITCODE -ne 0) {
        Write-Host "Build failed!" -ForegroundColor Red
        exit 1
    }

    Write-Host "  Build completed successfully!" -ForegroundColor Green
    Write-Host ""
} else {
    Write-Host "[1/3] Skipping build (using existing publish directory)" -ForegroundColor Gray
    Write-Host ""
}

# Step 2: Check for NSIS
if (-not $SkipInstaller) {
    Write-Host "[2/3] Checking for NSIS..." -ForegroundColor Yellow

    $makensis = Get-Command makensis -ErrorAction SilentlyContinue

    if (-not $makensis) {
        Write-Host ""
        Write-Host "ERROR: NSIS (makensis.exe) not found in PATH!" -ForegroundColor Red
        Write-Host ""
        Write-Host "Please install NSIS:" -ForegroundColor Yellow
        Write-Host "  Option 1: choco install nsis" -ForegroundColor Cyan
        Write-Host "  Option 2: Download from https://nsis.sourceforge.io/" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "After installing, make sure 'makensis.exe' is in your PATH." -ForegroundColor Yellow
        Write-Host ""
        Write-Host "To skip installer creation, run with -SkipInstaller flag." -ForegroundColor Gray
        exit 1
    }

    Write-Host "  Found: $($makensis.Source)" -ForegroundColor Green
    Write-Host ""

    # Step 3: Build the installer
    Write-Host "[3/4] Building NSIS installer..." -ForegroundColor Yellow
    Write-Host "  Input script: $installerScript" -ForegroundColor Gray
    Write-Host "  Output file: $installerOutput" -ForegroundColor Gray
    Write-Host ""

    & makensis /V2 /DVERSION=$version $installerScript

    if ($LASTEXITCODE -ne 0) {
        Write-Host "Installer build failed!" -ForegroundColor Red
        exit 1
    }

    # Rename the installer to include version
    $defaultOutput = Join-Path $publishDir "OpenXRRuntimeSwitcher-Setup.exe"
    if (Test-Path $defaultOutput) {
        Move-Item $defaultOutput $installerOutput -Force
    }

    Write-Host ""
    Write-Host "Installer created successfully!" -ForegroundColor Green
    Write-Host "  Location: $installerOutput" -ForegroundColor Cyan
    Write-Host "  Size: $([math]::Round((Get-Item $installerOutput).Length / 1MB, 2)) MB" -ForegroundColor Cyan
} else {
    Write-Host "[2/4] Skipping NSIS check" -ForegroundColor Gray
    Write-Host "[3/4] Skipping installer creation" -ForegroundColor Gray
}

# Step 4: Create ZIP distribution
if (-not $SkipZip) {
    Write-Host ""
    Write-Host "[4/4] Creating ZIP distribution..." -ForegroundColor Yellow

    if (Test-Path $zipOutput) {
        Remove-Item $zipOutput -Force
    }

    # Create temp directory for ZIP contents
    $tempZipDir = Join-Path $publishDir "temp_zip"
    if (Test-Path $tempZipDir) {
        Remove-Item $tempZipDir -Recurse -Force
    }
    New-Item -ItemType Directory -Path $tempZipDir | Out-Null

    # Copy everything except installer setup and temp files
    Get-ChildItem $publishDir -Exclude "*Setup*.exe", "temp_*", "*.zip" | Copy-Item -Destination $tempZipDir -Recurse -Force

    # Create the ZIP
    Compress-Archive -Path "$tempZipDir\*" -DestinationPath $zipOutput -Force

    # Clean up temp directory
    Remove-Item $tempZipDir -Recurse -Force

    Write-Host "  ZIP created successfully!" -ForegroundColor Green
    Write-Host "  Location: $zipOutput" -ForegroundColor Cyan
    Write-Host "  Size: $([math]::Round((Get-Item $zipOutput).Length / 1MB, 2)) MB" -ForegroundColor Cyan
} else {
    Write-Host ""
    Write-Host "[4/4] Skipping ZIP creation" -ForegroundColor Gray
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Build completed!" -ForegroundColor Green
if (-not $SkipInstaller) {
    Write-Host "  Installer: $installerOutput" -ForegroundColor Cyan
}
if (-not $SkipZip) {
    Write-Host "  ZIP: $zipOutput" -ForegroundColor Cyan
}
Write-Host "========================================" -ForegroundColor Cyan
