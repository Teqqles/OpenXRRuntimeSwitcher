using System.Linq;
using Xunit;
using OpenXRRuntimeSwitcher.Models;
using OpenXRRuntimeSwitcher.Services;
using OpenXRRuntimeSwitcher.Services.Abstractions;
using OpenXRRuntimeSwitcher.Tests.Fakes;

namespace OpenXRRuntimeSwitcher.Tests.Unit.Services;

public sealed class ApiLayerServiceTests
{
    private const string Key = @"SOFTWARE\Khronos\OpenXR\1\ApiLayers\Implicit";

    [Fact]
    public void GetLayers_EmptyRegistry_ReturnsEmpty()
    {
        var svc = new ApiLayerService(new FakeRegistryService());
        Assert.Empty(svc.GetLayers());
    }

    [Fact]
    public void GetLayers_ReadsBothHives_WithScopeAndOrder()
    {
        var fake = new FakeRegistryService();
        fake.WriteDword(RegistryHive.CurrentUser, Key, @"C:\user1.json", 0);
        fake.WriteDword(RegistryHive.LocalMachine, Key, @"C:\sys1.json", 1);

        var layers = new ApiLayerService(fake).GetLayers();

        var user = layers.Single(l => l.Scope == LayerScope.User);
        var sys = layers.Single(l => l.Scope == LayerScope.System);
        Assert.Equal(@"C:\user1.json", user.ManifestPath);
        Assert.True(user.Enabled);
        Assert.Equal(0, user.Order);
        Assert.False(sys.Enabled);      // data 1 = disabled
    }

    [Fact]
    public void GetLayers_MissingPath_SetsPathExistsFalse()
    {
        var fake = new FakeRegistryService();
        fake.WriteDword(RegistryHive.CurrentUser, Key, @"C:\does\not\exist.json", 0);

        var layer = new ApiLayerService(fake).GetLayers().Single();

        Assert.False(layer.PathExists);
    }

    [Fact]
    public void GetLayers_NameFromManifestJson_ElseFilename()
    {
        var dir = Path.Combine(Path.GetTempPath(), $"OpenXRLayerTest_{Guid.NewGuid()}");
        try
        {
            Directory.CreateDirectory(dir);
            var named = Path.Combine(dir, "named.json");
            File.WriteAllText(named,
                @"{ ""file_format_version"": ""1.0.0"", ""api_layer"": { ""name"": ""XR_APILAYER_TEST"" } }");

            var fake = new FakeRegistryService();
            fake.WriteDword(RegistryHive.CurrentUser, Key, named, 0);
            fake.WriteDword(RegistryHive.CurrentUser, Key, @"C:\missing\other.json", 0);

            var layers = new ApiLayerService(fake).GetLayers();

            Assert.Equal("XR_APILAYER_TEST", layers.Single(l => l.ManifestPath == named).Name);
            Assert.Equal("other", layers.Single(l => l.ManifestPath == @"C:\missing\other.json").Name);
        }
        finally
        {
            if (Directory.Exists(dir)) Directory.Delete(dir, recursive: true);
        }
    }
}
