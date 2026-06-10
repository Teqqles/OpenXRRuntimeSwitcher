# OpenXR Runtime Switcher Installer

This folder contains the **Nullsoft Scriptable Install System (NSIS)** configuration for packaging the application into a distributable `.exe` installer.

---

## How It Works

The installer:

1. Copies all files from the `publish/` directory into  
   `C:\Program Files\OpenXRRuntimeSwitcher`
2. Creates Start Menu and Desktop shortcuts
3. Adds an optional “Run at startup” checkbox that creates a Windows Task Scheduler task
4. Registers uninstall information in Windows (appears in Add/Remove Programs)
5. Generates an uninstaller (`uninstall.exe`)

The “Run at startup” feature uses **Windows Task Scheduler** (not registry Run key) to support UAC elevation properly.

---

## Build Requirements

- **NSIS** (Nullsoft Scriptable Install System)
  - Install via Chocolatey: `choco install nsis`
  - Or download from: https://nsis.sourceforge.io/
- **.NET 9 SDK** (for building/publishing the app)

---

## Quick Build

Use the automated build script from the repository root:

```powershell
.\build-installer.ps1
```

The script will:
1. Extract version from the `.csproj` file
2. Build and publish the application to the `publish/` directory
3. Create the NSIS installer with version number: `OpenXRRuntimeSwitcher-Setup-<version>.exe`
4. Create a ZIP distribution: `OpenXRRuntimeSwitcher-<version>.zip`

Both files will be in the `publish/` directory.

### Script Options

```powershell
# Skip building (use existing publish folder)
.\build-installer.ps1 -SkipBuild

# Build only (skip installer creation)
.\build-installer.ps1 -SkipInstaller

# Skip ZIP creation
.\build-installer.ps1 -SkipZip

# Use Debug configuration
.\build-installer.ps1 -Configuration Debug
```

---

## Manual Build Steps

If you prefer to build manually:

```powershell
# 1. Publish the application
dotnet publish src\OpenXRRuntimeSwitcher\OpenXRRuntimeSwitcher.csproj `
  -c Release `
  -r win-x64 `
  -o publish `
  --self-contained true `
  /p:PublishSingleFile=true `
  /p:IncludeAllContentForSelfExtract=true

# 2. Build the installer (with version)
makensis /DVERSION=0.3.0 src\OpenXRRuntimeSwitcher\installer\OpenXRRuntimeSwitcher.nsi

# 3. Create ZIP distribution
Compress-Archive -Path "publish\*" -DestinationPath "publish\OpenXRRuntimeSwitcher-0.3.0.zip" -Exclude "*.exe"
```

Output files:
- Installer: `publish\OpenXRRuntimeSwitcher-Setup-0.3.0.exe`
- ZIP: `publish\OpenXRRuntimeSwitcher-0.3.0.zip`

---

## Installer Features

### Installation
- Installs to `C:\Program Files\OpenXRRuntimeSwitcher` by default
- Creates desktop shortcut
- Creates Start Menu folder with shortcut
- Optionally creates Windows Task Scheduler task for auto-start (with elevation)

### Uninstallation
- Removes all installed files
- Removes shortcuts
- Removes Task Scheduler task (if created)
- Removes registry entries
- Appears in Windows “Add or Remove Programs”

---

## Technical Details

- **Task Name**: `OpenXRRuntimeSwitcher_AutoStart`
- **Run Level**: Highest (supports UAC elevation)
- **Trigger**: On user logon
- **Registry Key**: `HKLM\Software\Microsoft\Windows\CurrentVersion\Uninstall\OpenXR Runtime Switcher`

---

## Notes

- The installer requires **administrator privileges** to install to Program Files
- The startup task is created using `schtasks` command (built into Windows)
- If the startup task creation fails, users can enable it later from the application UI
