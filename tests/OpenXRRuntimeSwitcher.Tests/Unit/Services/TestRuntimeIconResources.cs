using System.Drawing;
using OpenXRRuntimeSwitcher.Properties;
using OpenXRRuntimeSwitcher.Services.Abstractions;

namespace OpenXRRuntimeSwitcher.Tests.Unit.Services;

public sealed class TestRuntimeIconResources : IRuntimeIconResources
{
    public Image SteamVRIcon => CloneWithTag(Resources.SteamVRIcon, nameof(Resources.SteamVRIcon));
    public Image SteamVRIconDarkMode => CloneWithTag(Resources.SteamVRIconDarkMode, nameof(Resources.SteamVRIconDarkMode));

    public Image MetaIcon => CloneWithTag(Resources.MetaIcon, nameof(Resources.MetaIcon));
    public Image MetaIconDarkMode => CloneWithTag(Resources.MetaIconDarkMode, nameof(Resources.MetaIconDarkMode));

    public Image PimaxIcon => CloneWithTag(Resources.PimaxIcon, nameof(Resources.PimaxIcon));
    public Image PimaxIconDarkMode => CloneWithTag(Resources.PimaxIconDarkMode, nameof(Resources.PimaxIconDarkMode));

    public Image WMRIcon => CloneWithTag(Resources.WMRIcon, nameof(Resources.WMRIcon));
    public Image WMRIconDarkMode => CloneWithTag(Resources.WMRIconDarkMode, nameof(Resources.WMRIconDarkMode));

    public Image VarjoIcon => CloneWithTag(Resources.VarjoIcon, nameof(Resources.VarjoIcon));
    public Image VarjoIconDarkMode => CloneWithTag(Resources.VarjoIconDarkMode, nameof(Resources.VarjoIconDarkMode));

    public Image VirtualDesktopIcon => CloneWithTag(Resources.VirtualDesktopIcon, nameof(Resources.VirtualDesktopIcon));
    public Image VirtualDesktopIconDarkMode => CloneWithTag(Resources.VirtualDesktopIconDarkMode, nameof(Resources.VirtualDesktopIconDarkMode));

    public Image UnknownIcon => CloneWithTag(Resources.UnknownIcon, nameof(Resources.UnknownIcon));
    public Image UnknownIconDarkMode => CloneWithTag(Resources.UnknownIconDarkMode, nameof(Resources.UnknownIconDarkMode));

    private static Image CloneWithTag(Image img, string tag)
    {
        var clone = (Image)img.Clone();
        clone.Tag = tag;
        return clone;
    }
}
