namespace OpenXRRuntimeSwitcher.Services.Abstractions;

public interface IRuntimeInfoProvider
{
    bool TryGetInfo(string key, out RuntimeInfo info);
    IReadOnlyDictionary<string, RuntimeInfo> GetAll();
}

public sealed record RuntimeInfo(string FriendlyName, Image Icon);