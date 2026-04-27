using OpenXRRuntimeSwitcher.Services;
using OpenXRRuntimeSwitcher.Services.Abstractions;
using System.Drawing;

namespace OpenXRRuntimeSwitcher.Tests.Unit.Services;

public sealed class RuntimeInfoProviderTests
{
    [Fact]
    public void TryGetInfo_AllKnownKeys_ReturnsExpectedFriendlyNames()
    {
        var provider = new RuntimeInfoProvider(new FakeIconFactory());

        Assert.True(provider.TryGetInfo("steamxr", out var steam));
        Assert.Equal("SteamVR OpenXR", steam.FriendlyName);

        Assert.True(provider.TryGetInfo("oculus_openxr", out var meta));
        Assert.Equal("Meta OpenXR", meta.FriendlyName);

        Assert.True(provider.TryGetInfo("pimax", out var pimax));
        Assert.Equal("PimaxXR", pimax.FriendlyName);

        Assert.True(provider.TryGetInfo("mixedreality", out var wmr));
        Assert.Equal("Windows Mixed Reality", wmr.FriendlyName);

        Assert.True(provider.TryGetInfo("varjo", out var varjo));
        Assert.Equal("Varjo OpenXR", varjo.FriendlyName);

        Assert.True(provider.TryGetInfo("virtualdesktop", out var vd));
        Assert.Equal("Virtual Desktop OpenXR", vd.FriendlyName);
    }

    [Fact]
    public void TryGetInfo_UnknownKey_ReturnsFalse()
    {
        var provider = new RuntimeInfoProvider(new FakeIconFactory());
        Assert.False(provider.TryGetInfo("unknown_key", out _));
    }

    private sealed class FakeIconFactory : IRuntimeIconFactory
    {
        public Image GetIcon(string key) => new Bitmap(1, 1);
    }
}
