using OpenXRRuntimeSwitcher.Models;
using OpenXRRuntimeSwitcher.Services.Abstractions;
using System.Text.Json;

namespace OpenXRRuntimeSwitcher.Services;

public sealed class CustomRuntimeService : ICustomRuntimeService
{
    private readonly IRegistryService _registryService;
    private const string AvailableRuntimesKey = @"SOFTWARE\Khronos\OpenXR\1\AvailableRuntimes";

    public CustomRuntimeService(IRegistryService registryService)
    {
        _registryService = registryService ?? throw new ArgumentNullException(nameof(registryService));
    }

    public IReadOnlyList<CustomRuntimeDefinition> LoadCustomRuntimes(string filePath)
    {
        if (!File.Exists(filePath))
            return Array.Empty<CustomRuntimeDefinition>();

        try
        {
            var json = File.ReadAllText(filePath);
            var runtimes = JsonSerializer.Deserialize<List<CustomRuntimeDefinition>>(json);
            return runtimes ?? new List<CustomRuntimeDefinition>();
        }
        catch (Exception ex)
        {
            TrayLogger.LogException(nameof(LoadCustomRuntimes), ex);
            return Array.Empty<CustomRuntimeDefinition>();
        }
    }

    public void SaveCustomRuntimes(string filePath, IReadOnlyList<CustomRuntimeDefinition> runtimes)
    {
        var json = JsonSerializer.Serialize(runtimes, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        File.WriteAllText(filePath, json);
    }

    public void RegisterCustomRuntimesInRegistry(IReadOnlyList<CustomRuntimeDefinition> runtimes)
    {
        var existingRegistry = _registryService.ReadKeyValues(AvailableRuntimesKey);

        foreach (var runtime in runtimes)
        {
            if (string.IsNullOrWhiteSpace(runtime.ManifestPath))
                continue;

            if (!File.Exists(runtime.ManifestPath))
            {
                TrayLogger.Log($"Skipping custom runtime '{runtime.Name}': manifest not found at {runtime.ManifestPath}");
                continue;
            }

            if (existingRegistry.ContainsKey(runtime.ManifestPath))
            {
                TrayLogger.Log($"Custom runtime '{runtime.Name}' already in registry at {runtime.ManifestPath}, skipping.");
                continue;
            }

            try
            {
                _registryService.WriteValue(AvailableRuntimesKey, runtime.ManifestPath, "0");
                TrayLogger.Log($"Registered custom runtime '{runtime.Name}' with manifest: {runtime.ManifestPath}");
            }
            catch (Exception ex)
            {
                TrayLogger.LogException($"Failed to register custom runtime '{runtime.Name}'", ex);
            }
        }
    }

    public bool AddCustomRuntime(string filePath, CustomRuntimeDefinition runtime)
    {
        var existing = LoadCustomRuntimes(filePath).ToList();

        if (existing.Any(r => string.Equals(r.ManifestPath, runtime.ManifestPath, StringComparison.OrdinalIgnoreCase)))
        {
            TrayLogger.Log($"Custom runtime with manifest '{runtime.ManifestPath}' already exists in custom_runtimes.json, skipping add.");
            return false;
        }

        if (IsManifestAlreadyRegistered(runtime.ManifestPath))
        {
            TrayLogger.Log($"Custom runtime with manifest '{runtime.ManifestPath}' already exists in registry, skipping add.");
            return false;
        }

        existing.Add(runtime);
        SaveCustomRuntimes(filePath, existing);
        return true;
    }

    public bool IsManifestAlreadyRegistered(string manifestPath)
    {
        var existingRegistry = _registryService.ReadKeyValues(AvailableRuntimesKey);
        return existingRegistry.ContainsKey(manifestPath);
    }
}
