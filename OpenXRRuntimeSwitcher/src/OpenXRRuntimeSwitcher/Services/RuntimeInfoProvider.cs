using System;
using System.Collections.Generic;
using System.Drawing;

namespace OpenXRRuntimeSwitcher.Services;

/// <summary>
/// Default mapping of runtime key -> (friendly name, icon).
/// This is isolated so it can be mocked or replaced for tests.
/// </summary>
public sealed class RuntimeInfoProvider : IRuntimeInfoProvider
{
    private readonly Dictionary<string, RuntimeInfo> _map;

    public RuntimeInfoProvider(IRuntimeIconFactory? iconFactory = null)
    {
        var factory = iconFactory ?? new DefaultRuntimeIconFactory();

        _map = new Dictionary<string, RuntimeInfo>(StringComparer.OrdinalIgnoreCase)
        {
            { "steamxr", new RuntimeInfo("SteamVR OpenXR", factory.GetIcon("steamxr")) },
            { "oculus_openxr", new RuntimeInfo("Meta OpenXR", factory.GetIcon("oculus_openxr")) },
            { "pimax", new RuntimeInfo("PimaxXR", factory.GetIcon("pimax")) },
            { "mixedreality", new RuntimeInfo("Windows Mixed Reality", factory.GetIcon("mixedreality")) },
            { "varjo", new RuntimeInfo("Varjo OpenXR", factory.GetIcon("varjo")) },
            { "virtualdesktop", new RuntimeInfo("VirtualDesktopXR", factory.GetIcon("virtualdesktop")) }
        };
    }

    public bool TryGetInfo(string key, out RuntimeInfo info)
    {
        if (_map.TryGetValue(key ?? string.Empty, out var foundInfo))
        {
            info = foundInfo;
            return true;
        }
        info = null!;
        return false;
    }

    public IReadOnlyDictionary<string, RuntimeInfo> GetAll() => _map;
}