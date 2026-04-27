# 📘 OpenXR Runtime Switcher
A lightweight Windows utility for switching between installed OpenXR runtimes.

OpenXR Runtime Switcher is a small, fast tool that lets you change the active OpenXR runtime on Windows without digging through registry keys or vendor‑specific settings. It automatically detects installed runtimes, shows friendly names and icons, and now adapts to **Windows dark mode**.

---

## ✨ Features

### ✔ Detects installed OpenXR runtimes  
SteamVR, Meta/Oculus, PimaxXR, Varjo, Windows Mixed Reality, and custom runtimes.

### ✔ Friendly names + icons  
Clean vendor names and logos — no file paths.

### ✔ Dark‑mode‑aware icons (NEW)  
The app now detects Windows dark mode (via `.NET 9`’s `Application.IsDarkModeEnabled`) and automatically switches to dark‑mode icon variants for all supported runtimes.

### ✔ System colour mode (NEW)  
The UI now follows the system’s colour mode using:

```csharp
Application.SetColorMode(SystemColorMode.System);
```

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
| **Custom runtime support** | ✔ Add custom runtimes (read from registry only) | ✔ Add custom runtimes | ❌ No |
| **Dark mode support** | ✔ Full dark‑mode UI + icon variants | ❌ None | ✔ Fluent UI (implicitly dark‑mode friendly) |
| **Tray icon integration** | ✔ Shows current runtime | ❌ None | ❌ None |
| **Admin rights handling** | Requires admin | Requires admin | Requires admin |
| **32‑bit runtime handling** | ❌ Does not handle 32‑bit | Not mentioned | ❌ Does not handle 32‑bit |
| **Installer / packaging** | ZIP | Standalone executable | Standalone executable |
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

## 📄 License

MIT License — free to use, modify, and distribute.

## 🤝 Contributing

Pull requests are welcome!
If you want to add support for additional runtimes or improve detection logic, feel free to open an issue or PR.
