using OpenXRRuntimeSwitcher.Services;
using OpenXRRuntimeSwitcher.Services.Abstractions;
using OpenXRRuntimeSwitcher.Services.UI;
using OpenXRRuntimeSwitcher.Tests.Unit.Services;
using System.Drawing;
using Xunit;

namespace OpenXRRuntimeSwitcher.Tests.Unit;

public sealed class DefaultRuntimeIconFactoryTests
{
    [Theory]
    [InlineData("steamxr", "SteamVRIcon", "SteamVRIconDarkMode")]
    [InlineData("oculus_openxr", "MetaIcon", "MetaIconDarkMode")]
    [InlineData("pimax", "PimaxIcon", "PimaxIconDarkMode")]
    [InlineData("mixedreality", "WMRIcon", "WMRIconDarkMode")]
    [InlineData("varjo", "VarjoIcon", "VarjoIconDarkMode")]
    [InlineData("virtualdesktop", "VirtualDesktopIcon", "VirtualDesktopIconDarkMode")]
    [InlineData("unknown random runtime", "UnknownIcon", "UnknownIconDarkMode")]
    public void GetIcon_ReturnsCorrectResource_BasedOnDarkMode(
        string key,
        string expectedLightTag,
        string expectedDarkTag)
    {
        // Arrange
        var resources = new TestRuntimeIconResources();

        var lightFactory = new DefaultRuntimeIconFactory(
            new FakeDarkModeProvider(false),
            resources);

        var darkFactory = new DefaultRuntimeIconFactory(
            new FakeDarkModeProvider(true),
            resources);

        // Act
        var lightIcon = lightFactory.GetIcon(key);
        var darkIcon = darkFactory.GetIcon(key);

        // Assert
        Assert.Equal(expectedLightTag, lightIcon.Tag);
        Assert.Equal(expectedDarkTag, darkIcon.Tag);
    }

    private sealed class FakeDarkModeProvider : IDarkModeProvider
    {
        private readonly bool _dark;

        public FakeDarkModeProvider(bool dark)
        {
            _dark = dark;
        }

        public bool IsDarkMode() => _dark;
    }
}
