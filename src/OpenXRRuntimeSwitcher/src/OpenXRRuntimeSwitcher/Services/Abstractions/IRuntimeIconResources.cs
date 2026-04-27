namespace OpenXRRuntimeSwitcher.Services.Abstractions;

public interface IRuntimeIconResources
{
    Image SteamVRIcon { get; }
    Image SteamVRIconDarkMode { get; }

    Image MetaIcon { get; }
    Image MetaIconDarkMode { get; }

    Image PimaxIcon { get; }
    Image PimaxIconDarkMode { get; }

    Image WMRIcon { get; }
    Image WMRIconDarkMode { get; }

    Image VarjoIcon { get; }
    Image VarjoIconDarkMode { get; }

    Image VirtualDesktopIcon { get; }
    Image VirtualDesktopIconDarkMode { get; }

    Image UnknownIcon { get; }
    Image UnknownIconDarkMode { get; }
}
