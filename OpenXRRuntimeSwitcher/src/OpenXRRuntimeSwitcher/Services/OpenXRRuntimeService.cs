using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.Json;
using OpenXRRuntimeSwitcher.Models;
using OpenXRRuntimeSwitcher.Services.Abstractions;

namespace OpenXRRuntimeSwitcher.Services;

public interface IOpenXRRuntimeService
{
    IReadOnlyList<OpenXRRuntime> GetAvailableRuntimes();
    string? GetActiveRuntimeManifest();
    void SetActiveRuntime(string manifestPath);
}

public sealed class OpenXRRuntimeService : IOpenXRRuntimeService
{
    private readonly IRegistryService _registry;
    private const string BaseKey = @"SOFTWARE\Khronos\OpenXR\1";
    private const string AvailableKey = BaseKey + @"\AvailableRuntimes";

    public OpenXRRuntimeService(IRegistryService registry)
    {
        _registry = registry;
    }

    public IReadOnlyList<OpenXRRuntime> GetAvailableRuntimes()
    {
        var values = _registry.ReadKeyValues(AvailableKey);

        return values.Select(kvp =>
        {
            // kvp.Key is the manifest path; kvp.Value is typically a numeric flag (e.g. "0")
            var manifestPathRaw = kvp.Key ?? string.Empty;
            var manifestPath = NormalizeManifestPath(manifestPathRaw);

            // Try to read a friendly name from the manifest JSON ("runtime"."name")
            var manifestName = TryReadRuntimeNameFromManifest(manifestPath);

            // Fall back to the registry value when present, otherwise file name
            var displayName = !string.IsNullOrWhiteSpace(manifestName)
                ? manifestName
                : (!string.IsNullOrWhiteSpace(kvp.Value) ? kvp.Value : Path.GetFileNameWithoutExtension(manifestPath));

            return new OpenXRRuntime(displayName ?? string.Empty, manifestPath);
        }).ToList();
    }

    public string? GetActiveRuntimeManifest() =>
        _registry.ReadValue(BaseKey, "ActiveRuntime");

    public void SetActiveRuntime(string manifestPath)
    {
        if (string.IsNullOrWhiteSpace(manifestPath))
            throw new ArgumentException("Manifest path cannot be null or empty.", nameof(manifestPath));

        _registry.WriteValue(BaseKey, "ActiveRuntime", manifestPath);
    }

    private static string NormalizeManifestPath(string raw)
    {
        // Remove surrounding quotes, expand env vars and trim whitespace
        if (string.IsNullOrWhiteSpace(raw)) return raw ?? string.Empty;
        var trimmed = raw.Trim().Trim('"');
        try
        {
            return Environment.ExpandEnvironmentVariables(trimmed);
        }
        catch
        {
            return trimmed;
        }
    }

    private static string? TryReadRuntimeNameFromManifest(string manifestPath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(manifestPath))
                return null;

            // Only attempt if file exists
            if (!File.Exists(manifestPath))
                return null;

            using var stream = File.OpenRead(manifestPath);
            using var doc = JsonDocument.Parse(stream);
            if (doc.RootElement.TryGetProperty("runtime", out var runtimeElement))
            {
                if (runtimeElement.ValueKind == JsonValueKind.Object
                    && runtimeElement.TryGetProperty("name", out var nameElement)
                    && nameElement.ValueKind == JsonValueKind.String)
                {
                    var name = nameElement.GetString();
                    return string.IsNullOrWhiteSpace(name) ? null : name;
                }
            }

            return null;
        }
        catch
        {
            // Manifest might be malformed or unreadable; ignore and fall back gracefully.
            return null;
        }
    }
}