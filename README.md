# 📘 OpenXR Runtime Switcher
A lightweight Windows utility for switching between installed OpenXR runtimes.

OpenXR Runtime Switcher is a small and fast tool that lets you change the active OpenXR runtime on Windows without digging through registry keys or vendor specific settings. It automatically detects installed runtimes and shows friendly names and icons.

## ✨ Features
Detects installed OpenXR runtimes  
SteamVR, Meta/Oculus, PimaxXR, Varjo, Windows Mixed Reality, and custom runtimes.

- Friendly names + icons  
No file paths — the UI shows clean vendor names and logos.

- Runtime detection via JSON manifest  
Reads the OpenXR loader’s active runtime JSON for accurate identification.

- Tray icon  
Quick access from the system tray.

- Run at startup  
Automatically launch the tool when Windows starts.

- Clean, modern UI layout  
Icon + friendly name at the top, runtime dropdown, and Apply/Startup controls at the bottom.

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
