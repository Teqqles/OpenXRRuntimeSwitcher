using OpenXRRuntimeSwitcher.Services.Abstractions;
using OpenXRRuntimeSwitcher.Services.UI;

public sealed class DefaultRuntimeIconFactory : IRuntimeIconFactory
{
    private readonly IDarkModeProvider _darkMode;
    private readonly IRuntimeIconResources _resources;

    public DefaultRuntimeIconFactory(IDarkModeProvider darkMode, IRuntimeIconResources resources)
    {
        _darkMode = darkMode;
        _resources = resources;
    }

    public Image GetIcon(string key)
    {
        bool dark = _darkMode.IsDarkMode();

        return key?.ToLowerInvariant() switch
        {
            "steamxr" => dark ? _resources.SteamVRIconDarkMode : _resources.SteamVRIcon,
            "oculus_openxr" => dark ? _resources.MetaIconDarkMode : _resources.MetaIcon,
            "pimax" => dark ? _resources.PimaxIconDarkMode : _resources.PimaxIcon,
            "mixedreality" => dark ? _resources.WMRIconDarkMode : _resources.WMRIcon,
            "varjo" => dark ? _resources.VarjoIconDarkMode : _resources.VarjoIcon,
            "virtualdesktop" => dark ? _resources.VirtualDesktopIconDarkMode : _resources.VirtualDesktopIcon,
            _ => dark ? _resources.UnknownIconDarkMode : _resources.UnknownIcon
        };
    }
}
