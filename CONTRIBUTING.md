# Contributing to OpenXRRuntimeSwitcher

Thank you for contributing. This document describes repository layout, coding standards, and contribution workflow used by this project.

## Repository layout

We follow the recommended .NET repository layout:

- `src/` - Application and library projects. Each project lives in its own folder, for example `src/OpenXRRuntimeSwitcher/`.
- `tests/` - Test projects. Each test project mirrors the corresponding project under `src/`.
- `docs/` - Optional documentation and design notes.
- `samples/` - Small sample apps or code snippets.
- `build/` - Build scripts and CI helpers.

This layout keeps code and tests separated and simplifies tooling.

## Project structure rules

- Projects targeting Windows UI apps must set `TargetFramework` to `net8.0-windows` and `UseWindowsForms` as needed. Prefer centralizing common properties in `Directory.Build.props` at repo root.
- Keep projects self-contained under `src/<ProjectName>/`.
- Keep resources under `Properties/` inside the project folder (e.g. `src/OpenXRRuntimeSwitcher/Properties/Resources.resx`).

## Coding standards

- Use file-scoped namespaces (e.g. `namespace OpenXRRuntimeSwitcher;`).
- Use 4 spaces for indentation.
- Keep nullable reference types enabled (`<Nullable>enable</Nullable>`).
- Prefer explicit `using` statements inside files rather than global usings unless the using is widely shared across the repo.
- Keep code format consistent with `.editorconfig` (see `.editorconfig` at repo root).

## Pull request and branching

- Fork the repo or create a feature branch from `main` named `feat/<short-description>` or `fix/<short-description>`.
- Include unit tests for logic changes; add integration tests if necessary.
- Keep PRs small and focused. Describe why the change is needed and how it was implemented.
- CI must pass before merge. Squash merge is preferred for a clean history.

## Local development

- Use `dotnet build` from the repo root.
- To move existing project into the new layout, use `git mv` to preserve history: `git mv OpenXRRuntimeSwitcher.csproj src/OpenXRRuntimeSwitcher/` and move the related source files accordingly.

## Files added by maintainers

We maintain the repository-level files:

- `Directory.Build.props` - central MSBuild props such as target framework
- `.editorconfig` - formatting rules
- `CONTRIBUTING.md` - this file

If you need changes to these policies, open an issue or PR describing the rationale.