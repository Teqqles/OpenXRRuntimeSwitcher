using Xunit;
using System.IO;
using OpenXRRuntimeSwitcher.Services;

namespace OpenXRRuntimeSwitcher.Tests.Unit.Services;

public sealed class HotkeyConfigServiceTests
{
    [Fact]
    public void Load_ValidConfig_ReturnsMappings()
    {
        var path = Path.GetTempFileName();
        File.WriteAllText(path, "[Hotkeys]\nCycle=Ctrl+Alt+X\nSteamVR=Ctrl+Alt+1");

        var svc = new ConfigService();
        var cfg = svc.Load(path);

        Assert.True(cfg.Mappings.ContainsKey("Cycle"));
        Assert.Equal("Ctrl+Alt+X", cfg.Mappings["Cycle"]);

        File.Delete(path);
    }

    [Fact]
    public void Load_MissingFile_ReturnsEmptyMappings()
    {
        var svc = new ConfigService();
        var cfg = svc.Load("nonexistent.ini");

        Assert.Empty(cfg.Mappings);
    }
}
