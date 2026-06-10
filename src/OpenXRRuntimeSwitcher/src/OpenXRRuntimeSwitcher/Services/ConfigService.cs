using OpenXRRuntimeSwitcher.Models;
using System.Text;

namespace OpenXRRuntimeSwitcher.Services;

public interface IConfigService
{
    Config Load(string path);
}

public sealed class ConfigService : IConfigService
{
    public Config Load(string path)
    {
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

        var mappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var disableToast = false;
        string? section = null;

        foreach (var rawLine in File.ReadAllLines(path, Encoding.UTF8))
        {
            var line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith(";"))
                continue;

            if (line.StartsWith("[") && line.EndsWith("]"))
            {
                section = line[1..^1].Trim();
                continue;
            }

            var kv = line.Split('=', 2);
            if (kv.Length != 2) continue;

            var key = kv[0].Trim();
            var value = kv[1].Trim();

            if (string.Equals(section, "General", StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(key, "DisableToast", StringComparison.OrdinalIgnoreCase))
                {
                    disableToast = value == "1" || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
                }
            }
            else if (string.Equals(section, "Hotkeys", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                    mappings[key] = value;
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
}
