using OpenXRRuntimeSwitcher.Services.Abstractions;

namespace OpenXRRuntimeSwitcher.Services;

/// <summary>
/// Default mapping of runtime key -> (friendly name, icon).
/// This is isolated so it can be mocked or replaced for tests.
/// </summary>
public sealed class RuntimeInfoProvider : IRuntimeInfoProvider
{
    private readonly Dictionary<string, RuntimeInfo> _map;

    public RuntimeInfoProvider(IRuntimeIconFactory iconFactory)
    {

        _map = new Dictionary<string, RuntimeInfo>(StringComparer.OrdinalIgnoreCase)
        {
            { "steamxr", new RuntimeInfo("SteamVR OpenXR", iconFactory.GetIcon("steamxr")) },
            { "oculus_openxr", new RuntimeInfo("Meta OpenXR", iconFactory.GetIcon("oculus_openxr")) },
            { "pimax", new RuntimeInfo("PimaxXR", iconFactory.GetIcon("pimax")) },
            { "mixedreality", new RuntimeInfo("Windows Mixed Reality", iconFactory.GetIcon("mixedreality")) },
            { "varjo", new RuntimeInfo("Varjo OpenXR", iconFactory.GetIcon("varjo")) },
            { "virtualdesktop", new RuntimeInfo("Virtual Desktop OpenXR", iconFactory.GetIcon("virtualdesktop")) }
        };
    }

    public bool TryGetInfo(string key, out RuntimeInfo info)
    {
        TrayLogger.Log($"Trying to get runtime info for key: '{key}'");
        if (_map.TryGetValue(key ?? string.Empty, out var foundInfo))
        {
            info = foundInfo;
            TrayLogger.Log($"Found runtime info: Name='{info.FriendlyName ?? string.Empty}'");
            return true;
        }
        TrayLogger.Log($"No runtime info found for key: '{key}'");
        info = null!;
        return false;
    }

    public IReadOnlyDictionary<string, RuntimeInfo> GetAll() => _map;
}