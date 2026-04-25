📘 OpenXR Runtime Switcher
A lightweight Windows utility for switching between installed OpenXR runtimes.

OpenXR Runtime Switcher is a small and fast tool that lets you change the active OpenXR runtime on Windows without digging through registry keys or vendor specific settings. It automatically detects installed runtimes and shows friendly names and icons.

✨ Features
Detects installed OpenXR runtimes  
SteamVR, Meta/Oculus, PimaxXR, Varjo, Windows Mixed Reality, and custom runtimes.

Friendly names + icons  
No file paths — the UI shows clean vendor names and logos.

Runtime detection via JSON manifest  
Reads the OpenXR loader’s active runtime JSON for accurate identification.

Tray icon  
Quick access from the system tray.

Run at startup  
Automatically launch the tool when Windows starts.

Clean, modern UI layout  
Icon + friendly name at the top, runtime dropdown, and Apply/Startup controls at the bottom.

🖼 UI Overview

The icon and friendly name update automatically when you select a runtime.

The Apply button is disabled when VR is active.

The tray icon shows the current runtime.

🔍 How Runtime Detection Works

Active Runtime (Registry + JSON)
Reads:

HKLM\SOFTWARE\Khronos\OpenXR\1\ActiveRuntime
Then parses the JSON manifest:

json
{
  "runtime": {
    "name": "SteamVR OpenXR",
    "library_path": "bin/win64/vrclient_x64.dll"
  }
}
This gives the true friendly name.

🚀 How to Use
Launch the app

Select a runtime from the dropdown

Click Apply

Restart any VR apps (if needed)

If VR is running, the Apply button will be disabled until the session ends.

🧩 Supported Runtimes
Runtime
SteamVR OpenXR	✔
Meta / Oculus OpenXR	✔
PimaxXR	✔
Varjo OpenXR	✔
Windows Mixed Reality	✔
Custom runtimes	✔


🛠 Development Notes
Written in C# / .NET

Uses WinForms for the UI

Icons stored in Resources/ and embedded via .resx

Runtime detection uses JSON parsing, not filename guessing

📄 License
MIT License — free to use, modify, and distribute.

🤝 Contributing
Pull requests are welcome!
If you want to add support for additional runtimes or improve detection logic, feel free to open an issue or PR.