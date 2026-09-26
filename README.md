# 📘 OpenXR Runtime Switcher
A lightweight Windows utility for switching OpenXR runtimes and managing OpenXR API layers.

OpenXR Runtime Switcher lets you change the active OpenXR runtime on Windows without digging through registry keys or vendor‑specific settings. It detects installed runtimes, shows friendly names and icons, lives in the system tray, and gives you full control over which implicit API layers load and in what order.

---

## ✨ Features

### Runtime switching
- **Detects installed OpenXR runtimes**: SteamVR, Meta/Oculus, PimaxXR, Varjo, Windows Mixed Reality, and custom runtimes.
- **Accurate friendly names**: read from each runtime's JSON manifest, not guessed from file paths.
- **Vendor icons**: clean logos for each runtime, with dark‑mode variants.
- **Safe switching**: the Apply button is disabled while a VR session is active.
- **Custom runtimes**: add any OpenXR runtime with a name, manifest path, and optional icon. Custom runtimes persist across restarts and are registered automatically on startup.

### API layer management (NEW)
- **View implicit API layers** from both user (`HKCU`) and system (`HKLM`) scope in one list, showing name, manifest path, scope, and enabled state.
- **Reorder load order** with Move Up / Move Down. Reordering is transactional: if a registry write fails, the original order is restored.
- **Enable / disable layers** without uninstalling them. Disabling is non‑destructive, so you can turn a layer back on at any time.
- **Delete** stale or unwanted layer entries.
- **Broken layer detection**: layers whose manifest file no longer exists are highlighted in red.
- **Scope‑aware editing**: system‑scope layers can only be changed when running elevated.

### Tray & desktop integration
- **Tray icon** shows the current runtime at a glance.
- **Global hotkeys** switch runtimes instantly, configured in `config.ini`.
- **Toast notifications** on runtime change (can be disabled).
- **Start with Windows** via Task Scheduler.
- **Dark mode**: the UI follows the system colour mode and swaps to dark‑mode icon variants automatically.

## 🖼 UI Overview

![selecting new runtime with preview](https://github.com/Teqqles/OpenXRRuntimeSwitcher/raw/main/docs/images/selecting_runtime.png)

The icon and friendly name update automatically when you select a runtime.

![selecting new runtime with preview](https://github.com/Teqqles/OpenXRRuntimeSwitcher/raw/main/docs/images/image_switching.png)

The Apply button is disabled when VR is active.

The tray icon shows the current runtime.

![tray icon](https://github.com/Teqqles/OpenXRRuntimeSwitcher/raw/main/docs/images/tray_icon.png)

## 🚀 How to Use

### Switching runtimes

1. Launch the app
2. Select a runtime from the dropdown
3. Click **Apply**
4. Restart any VR apps (if needed)

### Adding custom runtimes

1. Select "Add Custom Runtime..." from the dropdown
2. Fill in the dialog:
   - **Name:** Display name for your runtime
   - **JSON Manifest:** Path to the OpenXR runtime manifest file
   - **Image (Optional):** Path to an icon image
3. Click OK

Your custom runtime is saved and immediately available in the dropdown.

**Testing without VR hardware?** See the `examples/` folder for sample files you can use to test the custom runtime feature.

### Managing API layers

1. Click **API Layers...** in the main window
2. Select a layer in the list, then:
   - **Move Up / Move Down**: change its position in the load order
   - **Enable / Disable**: toggle whether the OpenXR loader uses it
   - **Delete**: remove the layer's registry entry
   - **Refresh**: reload the list from the registry
3. Click **Close** when done

Layers shown in red point to a manifest that no longer exists. These are usually left behind by an uninstalled tool and are safe to delete.

### Hotkeys and settings

Edit `config.ini` next to the executable:

```ini
[General]
; Set to 1 to disable toast notifications when switching runtimes
disableToast=0

[Hotkeys]
steamxr=Ctrl+Shift+Alt+F7
oculus_openxr=Ctrl+Shift+Alt+F8
pimax=Ctrl+Shift+Alt+F9
```

## 🔍 How It Works

### Runtime detection
Reads the active runtime from:

```
HKLM\SOFTWARE\Khronos\OpenXR\1\ActiveRuntime
```

Then parses the JSON manifest:

```json
{
  "runtime": {
    "name": "SteamVR OpenXR",
    "library_path": "bin/win64/vrclient_x64.dll"
  }
}
```

This gives the true friendly name.

### API layers
Implicit layers are registered as DWORD values under:

```
HKCU\SOFTWARE\Khronos\OpenXR\1\ApiLayers\Implicit
HKLM\SOFTWARE\Khronos\OpenXR\1\ApiLayers\Implicit
```

Each value name is a layer manifest path; a value of `0` means enabled and non‑zero means disabled. Value order determines load order.

## 🧩 Supported Runtimes

| Runtime | | Note |
| --- | --- | --- |
| SteamVR OpenXR |	✔ | |
| Meta / Oculus OpenXR |	✔ | |
| PimaxXR	| ✔ | |
| Varjo OpenXR | ? | Untested |
| Windows Mixed Reality |	? | Untested |
| Custom runtimes |	✔ | |

32‑bit runtimes are not currently handled.

## 🛠 Development Notes

Written in C# / .NET 9

Uses WinForms for the UI

Icons stored in Resources/ and embedded via .resx

Runtime detection uses JSON parsing, not filename guessing

### Building Releases

The project includes build automation for creating both installer and ZIP releases:

**Requirements:**
- .NET 9 SDK
- NSIS (install via `choco install nsis` or from https://nsis.sourceforge.io/)

**Quick Build:**
```powershell
.\build-installer.ps1
```

This creates both release formats in the `publish/` directory:
- **Installer**: `OpenXRRuntimeSwitcher-Setup-<version>.exe`
  - Program Files installation
  - Desktop and Start Menu shortcuts
  - Optional "Run at startup" via Task Scheduler (supports UAC elevation)
  - Windows Add/Remove Programs integration
- **ZIP**: `OpenXRRuntimeSwitcher-<version>.zip`
  - Portable version for manual extraction

Version is automatically extracted from the `.csproj` file.

For more details, see [`src/OpenXRRuntimeSwitcher/installer/README-INSTALLER.md`](src/OpenXRRuntimeSwitcher/installer/README-INSTALLER.md)

## 📄 License

MIT License: free to use, modify, and distribute.

## 🤝 Contributing

Pull requests are welcome!
If you want to add support for additional runtimes or improve detection logic, feel free to open an issue or PR.
