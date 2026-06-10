# Custom Runtime Examples

This folder contains example files for adding custom OpenXR runtimes to the OpenXR Runtime Switcher.

## Files

- **custom_runtimes.json** - Example custom runtime definitions file
- **sample_openxr_manifest.json** - Example OpenXR runtime manifest structure

## How to Use

### Testing the Custom Runtime Feature (Using Included Example)

The easiest way to test the custom runtime feature without installing a real VR runtime:

1. **Copy the example files** from this `examples/` folder to your installation directory:
   - Copy `custom_runtimes.json` → `C:\Program Files\OpenXRRuntimeSwitcher\`
   - Copy `sample_openxr_manifest.json` → `C:\Program Files\OpenXRRuntimeSwitcher\examples\`

2. **Launch the application**
   - The "Example Test Runtime" will appear in the dropdown
   - You can select it and click Apply
   - The runtime will be set in the registry (though it won't actually work for VR since it's a dummy)

This allows you to test the custom runtime UI and functionality without needing actual VR hardware.

### Creating Your Own Test Manifest

If you want to create a custom test manifest in a different location:

**Example:** `C:\TestRuntime\openxr_runtime.json`
```json
{
  "file_format_version": "1.0.0",
  "runtime": {
    "name": "My Test Runtime",
    "library_path": ".\\bin\\x64\\test.dll",
    "api_layers": []
  }
}
```

Then update `custom_runtimes.json`:
```json
[
  {
    "Name": "My Test Runtime",
    "ManifestPath": "C:\\TestRuntime\\openxr_runtime.json",
    "ImagePath": null
  }
]
```

### Adding Custom Runtimes via UI

Instead of manually editing the JSON file, you can use the built-in UI:

1. Launch OpenXR Runtime Switcher
2. Click the dropdown showing available runtimes
3. Select **"Add Custom Runtime..."** at the bottom
4. Fill in the form:
   - **Name:** Display name for your runtime
   - **JSON Manifest:** Browse to the OpenXR manifest file (e.g., the included `sample_openxr_manifest.json`)
   - **Image (Optional):** Path to an icon file (PNG, JPG, BMP)
5. Click OK

The runtime will be saved to `custom_runtimes.json` automatically in the same directory as the executable.

## Custom Runtime Format

### custom_runtimes.json Structure

```json
[
  {
    "Name": "Display name shown in dropdown",
    "ManifestPath": "Full path to OpenXR manifest JSON file",
    "ImagePath": "Optional path to icon image"
  }
]
```

**Fields:**
- **Name** (required): Friendly name displayed in the runtime dropdown
- **ManifestPath** (required): Absolute path to the OpenXR runtime manifest JSON file
- **ImagePath** (optional): Absolute path to an icon image. Set to `null` or omit if not needed.

### OpenXR Manifest Structure

OpenXR runtimes provide a JSON manifest file with this structure:

```json
{
  "file_format_version": "1.0.0",
  "runtime": {
    "name": "Runtime Name",
    "library_path": "path\\to\\runtime.dll",
    "api_layers": []
  }
}
```

The OpenXR Runtime Switcher reads the `runtime.name` field to display the runtime's official name.

## Finding Existing Runtime Manifests

Installed OpenXR runtimes register their manifests in the Windows Registry:

**Registry Key:** `HKEY_LOCAL_MACHINE\SOFTWARE\Khronos\OpenXR\1\AvailableRuntimes`

You can browse this registry key to find manifest paths for installed runtimes like SteamVR, Meta, Pimax, etc.

## Notes

- Manifest files must exist at the specified path for the runtime to be registered
- Custom runtimes are automatically registered on application startup
- If a manifest file is deleted or moved, the runtime will be skipped with a log warning
- Icon images are optional but enhance the UI experience
- All paths should use Windows path format with escaped backslashes (`\\`) in JSON

## Example Use Cases

### Adding an Unsupported Runtime

If you have a VR runtime not recognized by OpenXR Runtime Switcher:

1. Locate its OpenXR manifest file (check registry or installation folder)
2. Use "Add Custom Runtime..." UI to register it
3. Optionally provide an icon from the runtime's installation folder

### Testing Without VR Hardware

For development or testing:

1. Create a dummy manifest file anywhere
2. Add it as a custom runtime
3. Switch to it (won't work for actual VR, but tests the switcher UI)

### Portable Runtime Configuration

Share your custom runtime configuration:

1. Copy `custom_runtimes.json` from your OpenXR Runtime Switcher directory
2. Share it with teammates or across machines
3. Recipients just need to place it next to the executable
