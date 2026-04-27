using OpenXRRuntimeSwitcher.Properties;
using OpenXRRuntimeSwitcher.Services.Abstractions;

namespace OpenXRRuntimeSwitcher.Services;

public sealed class RuntimeIconResources : IRuntimeIconResources
{
    public Image SteamVRIcon => Resources.SteamVRIcon;
    public Image SteamVRIconDarkMode => Resources.SteamVRIconDarkMode;

    public Image MetaIcon => Resources.MetaIcon;
    public Image MetaIconDarkMode => Resources.MetaIconDarkMode;

    public Image PimaxIcon => Resources.PimaxIcon;
    public Image PimaxIconDarkMode => Resources.PimaxIconDarkMode;

    public Image WMRIcon => Resources.WMRIcon;
    public Image WMRIconDarkMode => Resources.WMRIconDarkMode;

    public Image VarjoIcon => Resources.VarjoIcon;
    public Image VarjoIconDarkMode => Resources.VarjoIconDarkMode;

    public Image VirtualDesktopIcon => Resources.VirtualDesktopIcon;
    public Image VirtualDesktopIconDarkMode => Resources.VirtualDesktopIconDarkMode;

    public Image UnknownIcon => Resources.UnknownIcon;
    public Image UnknownIconDarkMode => Resources.UnknownIconDarkMode;
}
