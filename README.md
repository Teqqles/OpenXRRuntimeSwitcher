# 📘 OpenXR Runtime Switcher
A lightweight Windows utility for switching between installed OpenXR runtimes.

OpenXR Runtime Switcher is a small, fast tool that lets you change the active OpenXR runtime on Windows without digging through registry keys or vendor‑specific settings. It automatically detects installed runtimes, shows friendly names and icons, and now adapts to **Windows dark mode**.

---

## ✨ Features

### ✔ Detects installed OpenXR runtimes  
SteamVR, Meta/Oculus, PimaxXR, Varjo, Windows Mixed Reality, and custom runtimes.

### ✔ Add custom runtimes via UI (NEW)  
Built-in dialog to add any OpenXR runtime with name, manifest path, and optional icon. Custom runtimes persist across restarts and are automatically registered on startup.

### ✔ Friendly names + icons  
Clean vendor names and logos — no file paths.

### ✔ Dark‑mode‑aware icons  
The app detects Windows dark mode (via `.NET 9`’s `Application.IsDarkModeEnabled`) and automatically switches to dark‑mode icon variants for all supported runtimes.

### ✔ System colour mode  
The UI follows the system’s colour mode using:

```csharp
Application.SetColorMode(SystemColorMode.System);
```

### ✔ Manage OpenXR API layers (NEW)
View registered implicit OpenXR API layers (user and system scope), reorder load order,
enable/disable layers as a resumeable play/pause, and delete unwanted entries. Layers whose
manifest path no longer exists are highlighted in red. System-scope edits require running
the app as administrator.

## 🖼 UI Overview

![selecting new runtime with preview](https://github.com/Teqqles/OpenXRRuntimeSwitcher/raw/main/docs/images/selecting_runtime.png)

The icon and friendly name update automatically when you select a runtime.

![selecting new runtime with preview](https://github.com/Teqqles/OpenXRRuntimeSwitcher/raw/main/docs/images/image_switching.png)

The Apply button is disabled when VR is active.

The tray icon shows the current runtime.

![tray icon](https://github.com/Teqqles/OpenXRRuntimeSwitcher/raw/main/docs/images/tray_icon.png)

## 🔍 How Runtime Detection Works

Active Runtime (Registry + JSON)
Reads:

HKLM\SOFTWARE\Khronos\OpenXR\1\ActiveRuntime
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

## 🚀 How to Use

Launch the app

Select a runtime from the dropdown

Click Apply

Restart any VR apps (if needed)

### Adding Custom Runtimes

1. Select "Add Custom Runtime..." from the dropdown
2. Fill in the dialog:
   - **Name:** Display name for your runtime
   - **JSON Manifest:** Path to the OpenXR runtime manifest file
   - **Image (Optional):** Path to an icon image
3. Click OK

Your custom runtime is saved and immediately available in the dropdown.

**Testing without VR hardware?** See the `examples/` folder for sample files you can use to test the custom runtime feature.

## 🧩 Supported Runtimes

| Runtime | | Note |
| --- | --- | --- |
| SteamVR OpenXR |	✔ | |
| Meta / Oculus OpenXR |	✔ | |
| PimaxXR	| ✔ | |
| Varjo OpenXR | ? | Untested |
| Windows Mixed Reality |	? | Untested |
| Custom runtimes |	✔ | |

## Alternatives

| Feature / Capability | **Teqqles / OpenXRRuntimeSwitcher** | **WaGi‑Coding / OpenXR‑Runtime‑Switcher** | **Ybalrid / OpenXR‑Runtime‑Manager** |
| --- | --- | --- | --- |
| **Primary purpose** | Modern Windows utility to switch OpenXR runtimes with friendly UI and hotkeys | Simple tool to switch system default OpenXR runtime | Utility to view & switch current OpenXR runtime |
| **UI framework** | WinForms (.NET 9 features, dark‑mode aware) | WinForms (older .NET style) | Fluent UI (recent upgrade) |
| **Runtime detection method** | Registry + JSON manifest parsing (accurate friendly names) | Registry presets; does *not* validate JSON | OpenXR enumeration + known manifest paths |
| **Supported runtimes** | SteamVR, Meta/Oculus, PimaxXR, Varjo (untested), WMR (untested), custom | SteamVR, Oculus/Meta, ViveVR, WMR, Varjo, custom | SteamVR, Oculus, MixedRealityRuntime, Varjo |
| **Custom runtime support** | ✔ Add via UI with persistence | ✔ Manual registry editing | ❌ No |
| **Dark mode support** | ✔ Full dark‑mode UI + icon variants | ❌ None | ✔ Fluent UI (implicitly dark‑mode friendly) |
| **Tray icon integration** | ✔ Shows current runtime | ❌ None | ❌ None |
| **Admin rights handling** | Requires admin | Requires admin | Requires admin |
| **32‑bit runtime handling** | ❌ Does not handle 32‑bit | Not mentioned | ❌ Does not handle 32‑bit |
| **Installer / packaging** | NSIS installer + ZIP | Standalone executable | Standalone executable |
| **Last updated** | **Active (2026)** | 2022 | **Active (2026)** |
| **Stars / activity** | 1 star (new project) | 102 stars | 18 stars |
| **License** | MIT | Custom license (similar to MIT) | MIT |

### 🔎 Summary
Teqqles/OpenXRRuntimeSwitcher (this repo)
Dark mode, icons, accurate detection, tray integration and hotkeys.

[WaGi‑Coding/OpenXR-Runtime-Switcher](https://github.com/WaGi-Coding/OpenXR-Runtime-Switcher)
The classic tool. Simple, functional, supports many runtimes, but lacks safety checks and modern UI. Requires admin elevation and doesn’t validate JSON manifests.

[Ybalrid/OpenXR-Runtime-Manager](https://github.com/Ybalrid/OpenXR-Runtime-Manager/tree/master)
Lightweight and clean, recently updated with Fluent UI. Good detection logic but fewer features overall, no tray icon, and hotkeys.

If you know of another tool not mentioned above, submit an issue!

## 🛠 Development Notes

Written in C# / .NET

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

MIT License — free to use, modify, and distribute.

## 🤝 Contributing

Pull requests are welcome!
If you want to add support for additional runtimes or improve detection logic, feel free to open an issue or PR.
