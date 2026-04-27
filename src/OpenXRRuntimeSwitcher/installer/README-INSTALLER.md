# 🧩 OpenXR Runtime Switcher Installer

This folder contains the **Nullsoft Scriptable Install System (NSIS)** configuration for packaging the application into a distributable `.exe` installer.

---

## 🛠 How It Works

The installer:

1. Copies all files from the `publish/` directory into  
   `C:\Program Files\OpenXRRuntimeSwitcher`
2. Creates Start Menu and Desktop shortcuts
3. Adds an optional “Run at startup” checkbox
4. Registers uninstall information in Windows
5. Generates an uninstaller (`uninstall.exe`)

---

## 🧱 Build Requirements

- **NSIS** (install via Chocolatey or official site)
- **.NET8 SDK** (for publishing the app)

---

## 🚀 Build Steps

```bash
dotnet publish src/OpenXRRuntimeSwitcher/OpenXRRuntimeSwitcher.csproj -c Release -r win-x64 -p:PublishSingleFile=true -o publish
makensis installer/OpenXRRuntimeSwitcher.nsi
