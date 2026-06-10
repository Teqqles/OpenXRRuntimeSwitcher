# Changelog

## [Unreleased]

### Added

- Option to disable toast notifications (#16)
  - Checkbox in UI to toggle toast notifications on/off
  - Persists to config.ini [General] section
  - Setting applies immediately without restart
- NSIS installer with automated build script (#3)
  - Windows installer with Add/Remove Programs integration
  - Desktop and Start Menu shortcuts
  - Optional "Run at startup" using Task Scheduler (supports UAC elevation)
  - PowerShell build script (`build-installer.ps1`) automates publish + installer creation
  - Installer documentation
- Manifest validation before switching runtimes (#13)
  - Validates manifest format (file_format_version, runtime object, library_path)
  - Verifies runtime DLL exists on disk
  - Resolves relative and absolute library paths correctly
  - Shows error messages when validation fails
- Dark Mode.  The application now respects the user settings in Windows.
- Exported brand icons, so Users can use them in their own workflows (e.g. Stream Deck actions)

### Changed

- File structure to better conform to our own CONTRIBUTING guide.
- Runtime switching now validates manifests before applying changes to prevent switching to broken runtimes

### Fixed

- Application starts with Windows.  Previous implementation does not work with UAC.

## [0.1.1] - 2026-04-25

### Added

- Installer, this is a work in progress and currently untested.
- Ability to minimize the window and this now does that to system tray.

### Changed

- Test project, having it all in one place was a pain with WinForms, so separated out, probably broken the test project as a result.

### Removed

- The buggy VR active detection and just put a notice in for now.  If I think of a better way to detect this, I'll re-add.

## [0.1.0] - 2026-04-24

### Initial implementation

Initial implementation: System tray app with hot key usage (configured via config,ini) and a bunch of runtimes prepopulated in.
