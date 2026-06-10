namespace OpenXRRuntimeSwitcher.Models;

public sealed class Config
{
    public required Dictionary<string, string> Mappings { get; init; }
    public bool DisableToast { get; init; }
}
