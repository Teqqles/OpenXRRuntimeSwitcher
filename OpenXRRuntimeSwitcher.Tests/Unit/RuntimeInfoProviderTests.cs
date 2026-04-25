using System.Drawing;
using Xunit;
using OpenXRRuntimeSwitcher.Services;

namespace OpenXRRuntimeSwitcher.Tests.Unit;

public sealed class RuntimeInfoProviderTests
{
    [Fact]
    public void TryGetInfo_KnownKeys_ReturnsFriendlyName()
    {
        var provider = new RuntimeInfoProvider(new FakeIconFactory());
        Assert.True(provider.TryGetInfo("steamxr", out var info));
        Assert.Equal("SteamVR OpenXR", info.FriendlyName);

        // unknown key returns false
        Assert.False(provider.TryGetInfo("unknown_key", out _));
    }

    private sealed class FakeIconFactory : IRuntimeIconFactory
    {
        public Image GetIcon(string key) => new Bitmap(1, 1);
    }
}