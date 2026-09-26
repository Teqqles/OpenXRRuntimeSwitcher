namespace OpenXRRuntimeSwitcher.Models;

public enum LayerScope
{
    User,   // HKCU
    System  // HKLM
}

public sealed record ApiLayer(
    LayerScope Scope,
    string ManifestPath,
    string Name,
    bool Enabled,
    bool PathExists)
{
    public bool IsEditable(bool isElevated) => Scope == LayerScope.User || isElevated;
}
