# Changelog

## [Unreleased] - 2026-04-27

### Added

- Dark Mode.  The application now respects the user settings in Windows.
- Exported brand icons, so Users can use them in their own workflows (e.g. Stream Deck actions)

### Changed

- File structure to better conform to our own CONTRIBUTING guide.

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
