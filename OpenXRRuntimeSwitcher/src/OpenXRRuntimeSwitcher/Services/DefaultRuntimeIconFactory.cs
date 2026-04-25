using OpenXRRuntimeSwitcher.Properties;
using System.Drawing;

namespace OpenXRRuntimeSwitcher.Services;

public sealed class DefaultRuntimeIconFactory : IRuntimeIconFactory
{
    public Image GetIcon(string key)
    {
        return key?.ToLowerInvariant() switch
        {
            "steamxr" => Resources.SteamVRIcon,
            "oculus_openxr" => Resources.MetaIcon,
            "pimax" => Resources.PimaxIcon,
            "mixedreality" => Resources.WMRIcon,
            "varjo" => Resources.VarjoIcon,
            "virtualdesktop" => Resources.VirtualDesktopIcon,
            _ => new Bitmap(1, 1)
        };
    }
}