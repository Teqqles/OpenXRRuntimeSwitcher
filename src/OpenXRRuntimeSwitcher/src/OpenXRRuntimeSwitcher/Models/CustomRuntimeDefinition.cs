namespace OpenXRRuntimeSwitcher.Models;

public sealed record CustomRuntimeDefinition(string Name, string ManifestPath, string? ImagePath = null);
