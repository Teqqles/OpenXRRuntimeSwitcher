using OpenXRRuntimeSwitcher.Models;
using IniParser;
using IniParser.Model;

namespace OpenXRRuntimeSwitcher.Services;

public interface IConfigService
{
    Config Load(string path);
    void UpdateDisableToast(bool disableToast);
}

public sealed class ConfigService : IConfigService
{
    private readonly FileIniDataParser _parser = new();
    private string? _lastLoadedPath;

    public Config Load(string path)
    {
        _lastLoadedPath = path;
        TrayLogger.Log($"Loading config from {path}...");
        if (!File.Exists(path))
        {
            TrayLogger.Log($"No config found at {path}");
            return new Config
            {
                Mappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                DisableToast = false
            };
        }

        var data = _parser.ReadFile(path);
        var mappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var disableToast = false;

        if (data.Sections.ContainsSection("General"))
        {
            var general = data["General"];
            if (general.ContainsKey("disableToast"))
            {
                var value = general["disableToast"];
                disableToast = value == "1" || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
            }
        }

        if (data.Sections.ContainsSection("Hotkeys"))
        {
            foreach (var key in data["Hotkeys"])
            {
                mappings[key.KeyName] = key.Value;
            }
        }

        TrayLogger.Log($"Loaded {mappings.Count} hotkey mappings from config.");
        TrayLogger.Log($"DisableToast: {disableToast}");

        return new Config
        {
            Mappings = mappings,
            DisableToast = disableToast
        };
    }

    public void UpdateDisableToast(bool disableToast)
    {
        if (_lastLoadedPath == null)
            throw new InvalidOperationException("Cannot update config before Load has been called");

        IniData data;

        if (File.Exists(_lastLoadedPath))
        {
            data = _parser.ReadFile(_lastLoadedPath);
        }
        else
        {
            data = new IniData();
        }

        if (!data.Sections.ContainsSection("General"))
        {
            data.Sections.AddSection("General");
        }

        data["General"]["disableToast"] = disableToast ? "1" : "0";

        _parser.WriteFile(_lastLoadedPath, data);
        TrayLogger.Log($"Updated disableToast={disableToast} in config");
    }
}
