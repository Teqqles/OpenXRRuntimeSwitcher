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

    [Fact]
    public void SetEnabled_False_WritesDisabledDword_WithoutDeleting()
    {
        var fake = new FakeRegistryService();
        fake.WriteDword(RegistryHive.CurrentUser, Key, @"C:\a.json", 0);
        var svc = new ApiLayerService(fake);
        var layer = svc.GetLayers().Single();

        svc.SetEnabled(layer, false);

        var stored = fake.ReadDwordValues(RegistryHive.CurrentUser, Key).Single();
        Assert.Equal(@"C:\a.json", stored.Name);   // still present -> resumeable
        Assert.Equal(1, stored.Data);              // disabled
    }

    [Fact]
    public void SetEnabled_True_WritesEnabledDword_InCorrectHive()
    {
        var fake = new FakeRegistryService();
        fake.WriteDword(RegistryHive.LocalMachine, Key, @"C:\sys.json", 1);
        var svc = new ApiLayerService(fake);
        var layer = svc.GetLayers().Single(l => l.Scope == LayerScope.System);

        svc.SetEnabled(layer, true);

        Assert.Equal(0, fake.ReadDwordValues(RegistryHive.LocalMachine, Key).Single().Data);
        Assert.Empty(fake.ReadDwordValues(RegistryHive.CurrentUser, Key)); // hive not touched
    }

    [Fact]
    public void Delete_RemovesValueFromItsHive()
    {
        var fake = new FakeRegistryService();
        fake.WriteDword(RegistryHive.CurrentUser, Key, @"C:\a.json", 0);
        fake.WriteDword(RegistryHive.CurrentUser, Key, @"C:\b.json", 0);
        var svc = new ApiLayerService(fake);
        var a = svc.GetLayers().Single(l => l.ManifestPath == @"C:\a.json");

        svc.Delete(a);

        var remaining = fake.ReadDwordValues(RegistryHive.CurrentUser, Key);
        Assert.Single(remaining);
        Assert.Equal(@"C:\b.json", remaining[0].Name);
    }

    [Fact]
    public void Reorder_RewritesValuesInGivenOrder_PreservingEnabledState()
    {
        var fake = new FakeRegistryService();
        fake.WriteDword(RegistryHive.CurrentUser, Key, @"C:\a.json", 0); // enabled
        fake.WriteDword(RegistryHive.CurrentUser, Key, @"C:\b.json", 1); // disabled
        fake.WriteDword(RegistryHive.CurrentUser, Key, @"C:\c.json", 0);
        var svc = new ApiLayerService(fake);

        svc.Reorder(LayerScope.User, new[] { @"C:\c.json", @"C:\a.json", @"C:\b.json" });

        var values = fake.ReadDwordValues(RegistryHive.CurrentUser, Key);
        Assert.Equal(new[] { @"C:\c.json", @"C:\a.json", @"C:\b.json" }, values.Select(v => v.Name).ToArray());
        Assert.Equal(0, values[0].Data); // c still enabled
        Assert.Equal(0, values[1].Data); // a still enabled
        Assert.Equal(1, values[2].Data); // b still disabled
    }

    [Theory]
    [InlineData(LayerScope.User, false, true)]   // user layers always editable
    [InlineData(LayerScope.User, true, true)]
    [InlineData(LayerScope.System, false, false)] // system needs elevation
    [InlineData(LayerScope.System, true, true)]
    public void IsEditable_GatesSystemLayersOnElevation(LayerScope scope, bool elevated, bool expected)
    {
        var layer = new ApiLayer(scope, @"C:\a.json", "a", true, true, 0);
        Assert.Equal(expected, layer.IsEditable(elevated));
    }

    [Fact]
    public void Reorder_WriteDwordFails_RollsBackAndRethrows()
    {
        var fake = new FakeRegistryService();
        fake.WriteDword(RegistryHive.CurrentUser, Key, @"C:\a.json", 0);
        fake.WriteDword(RegistryHive.CurrentUser, Key, @"C:\b.json", 1);
        fake.WriteDword(RegistryHive.CurrentUser, Key, @"C:\c.json", 0);
        var svc = new ApiLayerService(fake);

        // Snapshot the original state before Reorder.
        var originalValues = fake.ReadDwordValues(RegistryHive.CurrentUser, Key).ToList();

        // Make WriteDword fail once when writing the second value in the new order.
        // Allow subsequent writes (including rollback) to succeed.
        var throwOnce = false;
        fake.ThrowOnWriteDword = name =>
        {
            if (name == @"C:\b.json" && !throwOnce)
            {
                throwOnce = true;
                return true;
            }
            return false;
        };

        // Reorder should throw and roll back to the original state.
        Assert.Throws<InvalidOperationException>(() =>
            svc.Reorder(LayerScope.User, new[] { @"C:\c.json", @"C:\b.json", @"C:\a.json" }));

        // Verify the hive was restored exactly to the pre-Reorder state.
        var afterFailure = fake.ReadDwordValues(RegistryHive.CurrentUser, Key).ToList();
        Assert.Equal(originalValues.Count, afterFailure.Count);
        for (var i = 0; i < originalValues.Count; i++)
        {
            Assert.Equal(originalValues[i].Name, afterFailure[i].Name);
            Assert.Equal(originalValues[i].Data, afterFailure[i].Data);
        }
    }
}
