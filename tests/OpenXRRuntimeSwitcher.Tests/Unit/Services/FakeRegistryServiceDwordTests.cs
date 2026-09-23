using System.Linq;
using Xunit;
using OpenXRRuntimeSwitcher.Services.Abstractions;
using OpenXRRuntimeSwitcher.Tests.Fakes;

namespace OpenXRRuntimeSwitcher.Tests.Unit.Services;

public sealed class FakeRegistryServiceDwordTests
{
    private const string Key = @"SOFTWARE\Khronos\OpenXR\1\ApiLayers\Implicit";

    [Fact]
    public void WriteDword_ThenReadDwordValues_RoundTripsInInsertionOrder()
    {
        var fake = new FakeRegistryService();
        fake.WriteDword(RegistryHive.CurrentUser, Key, @"C:\a.json", 0);
        fake.WriteDword(RegistryHive.CurrentUser, Key, @"C:\b.json", 1);

        var values = fake.ReadDwordValues(RegistryHive.CurrentUser, Key);

        Assert.Equal(2, values.Count);
        Assert.Equal((@"C:\a.json", 0), values[0]);
        Assert.Equal((@"C:\b.json", 1), values[1]);
    }

    [Fact]
    public void ReadDwordValues_MissingKey_ReturnsEmpty()
    {
        var fake = new FakeRegistryService();
        Assert.Empty(fake.ReadDwordValues(RegistryHive.LocalMachine, Key));
    }

    [Fact]
    public void DeleteValue_RemovesOnlyThatValue()
    {
        var fake = new FakeRegistryService();
        fake.WriteDword(RegistryHive.CurrentUser, Key, @"C:\a.json", 0);
        fake.WriteDword(RegistryHive.CurrentUser, Key, @"C:\b.json", 0);

        fake.DeleteValue(RegistryHive.CurrentUser, Key, @"C:\a.json");

        var values = fake.ReadDwordValues(RegistryHive.CurrentUser, Key);
        Assert.Single(values);
        Assert.Equal(@"C:\b.json", values[0].Name);
    }

    [Fact]
    public void Hives_AreIsolated()
    {
        var fake = new FakeRegistryService();
        fake.WriteDword(RegistryHive.CurrentUser, Key, @"C:\a.json", 0);

        Assert.Single(fake.ReadDwordValues(RegistryHive.CurrentUser, Key));
        Assert.Empty(fake.ReadDwordValues(RegistryHive.LocalMachine, Key));
    }
}
