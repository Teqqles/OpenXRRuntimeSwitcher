using System.Linq;
using System.Text.Json;
using OpenXRRuntimeSwitcher.Models;
using OpenXRRuntimeSwitcher.Services.Abstractions;

namespace OpenXRRuntimeSwitcher.Services;

public sealed class ApiLayerService : IApiLayerService
{
    public const string ImplicitKey = @"SOFTWARE\Khronos\OpenXR\1\ApiLayers\Implicit";

    private readonly IRegistryService _registry;

    public ApiLayerService(IRegistryService registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    public IReadOnlyList<ApiLayer> GetLayers()
    {
        var layers = new List<ApiLayer>();
        layers.AddRange(ReadHive(RegistryHive.CurrentUser, LayerScope.User));
        layers.AddRange(ReadHive(RegistryHive.LocalMachine, LayerScope.System));
        return layers;
    }

    private IEnumerable<ApiLayer> ReadHive(RegistryHive hive, LayerScope scope)
    {
        var values = _registry.ReadDwordValues(hive, ImplicitKey);
        for (var i = 0; i < values.Count; i++)
        {
            var (path, data) = values[i];
            var exists = SafeFileExists(path);
            yield return new ApiLayer(
                Scope: scope,
                ManifestPath: path,
                Name: ResolveName(path, exists),
                Enabled: data == 0,
                PathExists: exists,
                Order: i);
        }
    }

    private static bool SafeFileExists(string path)
    {
        try { return !string.IsNullOrWhiteSpace(path) && File.Exists(path); }
        catch { return false; }
    }

    private static string ResolveName(string manifestPath, bool exists)
    {
        var fallback = Path.GetFileNameWithoutExtension(manifestPath);
        if (!exists) return fallback;
        try
        {
            using var stream = File.OpenRead(manifestPath);
            using var doc = JsonDocument.Parse(stream);
            if (doc.RootElement.TryGetProperty("api_layer", out var layer)
                && layer.ValueKind == JsonValueKind.Object
                && layer.TryGetProperty("name", out var name)
                && name.ValueKind == JsonValueKind.String)
            {
                var n = name.GetString();
                return string.IsNullOrWhiteSpace(n) ? fallback : n;
            }
        }
        catch { /* malformed manifest: fall back to filename */ }
        return fallback;
    }

    private static RegistryHive HiveFor(LayerScope scope) =>
        scope == LayerScope.User ? RegistryHive.CurrentUser : RegistryHive.LocalMachine;

    public void SetEnabled(ApiLayer layer, bool enabled)
    {
        ArgumentNullException.ThrowIfNull(layer);
        _registry.WriteDword(HiveFor(layer.Scope), ImplicitKey, layer.ManifestPath, enabled ? 0 : 1);
    }

    public void Delete(ApiLayer layer)
    {
        ArgumentNullException.ThrowIfNull(layer);
        _registry.DeleteValue(HiveFor(layer.Scope), ImplicitKey, layer.ManifestPath);
    }

    public void Reorder(LayerScope scope, IReadOnlyList<string> orderedManifestPaths)
    {
        ArgumentNullException.ThrowIfNull(orderedManifestPaths);
        var hive = HiveFor(scope);

        // Snapshot current data so we can preserve each layer's enabled/disabled state.
        var current = _registry.ReadDwordValues(hive, ImplicitKey);
        var dataByPath = current.ToDictionary(v => v.Name, v => v.Data);

        // Final sequence: requested order first (only those that exist), then any leftovers.
        var final = new List<string>();
        foreach (var path in orderedManifestPaths)
            if (dataByPath.ContainsKey(path) && !final.Contains(path))
                final.Add(path);
        foreach (var v in current)
            if (!final.Contains(v.Name))
                final.Add(v.Name);

        // Delete all, then re-add in order (loader honors registry enumeration order).
        foreach (var v in current)
            _registry.DeleteValue(hive, ImplicitKey, v.Name);
        foreach (var path in final)
            _registry.WriteDword(hive, ImplicitKey, path, dataByPath[path]);
    }
}
