using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using OpenXRRuntimeSwitcher.Models;

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
                Mappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            };
        }

        var mappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
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

            // Existing Hotkeys section parsing
            if (!string.Equals(section, "Hotkeys", StringComparison.OrdinalIgnoreCase))
                continue;

            var kv = line.Split('=', 2);
            if (kv.Length != 2) continue;

            var hotkey = kv[0].Trim();
            var mapping = kv[1].Trim();
            if (!string.IsNullOrEmpty(hotkey) && !string.IsNullOrEmpty(mapping))
                mappings[hotkey] = mapping;
        }

        TrayLogger.Log($"Loaded {mappings.Count} hotkey mappings from config.");

        return new Config
        {
            Mappings = mappings
        };
    }
}
