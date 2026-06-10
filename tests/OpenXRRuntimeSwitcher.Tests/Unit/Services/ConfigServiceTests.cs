using Xunit;
using OpenXRRuntimeSwitcher.Services;

namespace OpenXRRuntimeSwitcher.Tests.Unit.Services;

public sealed class ConfigServiceTests : IDisposable
{
    private readonly string _testDirectory;

    public ConfigServiceTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"ConfigTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
            Directory.Delete(_testDirectory, recursive: true);
    }

    [Fact]
    public void Load_NonExistentFile_ReturnsDefaultConfig()
    {
        var service = new ConfigService();
        var configPath = Path.Combine(_testDirectory, "nonexistent.ini");

        var config = service.Load(configPath);

        Assert.NotNull(config);
        Assert.Empty(config.Mappings);
        Assert.False(config.DisableToast);
    }

    [Fact]
    public void Load_DisableToastSet1_ReturnsTrue()
    {
        var configPath = Path.Combine(_testDirectory, "config.ini");
        File.WriteAllText(configPath, @"[General]
disableToast=1

[Hotkeys]
steamxr=Ctrl+Shift+Alt+F7");

        var service = new ConfigService();
        var config = service.Load(configPath);

        Assert.True(config.DisableToast);
        Assert.Single(config.Mappings);
    }

    [Fact]
    public void Load_DisableToastSet0_ReturnsFalse()
    {
        var configPath = Path.Combine(_testDirectory, "config.ini");
        File.WriteAllText(configPath, @"[General]
disableToast=0

[Hotkeys]
steamxr=Ctrl+Shift+Alt+F7");

        var service = new ConfigService();
        var config = service.Load(configPath);

        Assert.False(config.DisableToast);
    }

    [Fact]
    public void Load_DisableToastSetTrue_ReturnsTrue()
    {
        var configPath = Path.Combine(_testDirectory, "config.ini");
        File.WriteAllText(configPath, @"[General]
disableToast=true

[Hotkeys]
steamxr=Ctrl+Shift+Alt+F7");

        var service = new ConfigService();
        var config = service.Load(configPath);

        Assert.True(config.DisableToast);
    }

    [Fact]
    public void Load_DisableToastMissing_ReturnsFalse()
    {
        var configPath = Path.Combine(_testDirectory, "config.ini");
        File.WriteAllText(configPath, @"[General]

[Hotkeys]
steamxr=Ctrl+Shift+Alt+F7");

        var service = new ConfigService();
        var config = service.Load(configPath);

        Assert.False(config.DisableToast);
    }

    [Fact]
    public void Load_NoGeneralSection_ReturnsFalse()
    {
        var configPath = Path.Combine(_testDirectory, "config.ini");
        File.WriteAllText(configPath, @"[Hotkeys]
steamxr=Ctrl+Shift+Alt+F7");

        var service = new ConfigService();
        var config = service.Load(configPath);

        Assert.False(config.DisableToast);
        Assert.Single(config.Mappings);
    }
}
