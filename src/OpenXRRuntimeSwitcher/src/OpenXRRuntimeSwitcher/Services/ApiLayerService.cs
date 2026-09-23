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

    public void SetEnabled(ApiLayer layer, bool enabled) => throw new NotImplementedException();
    public void Delete(ApiLayer layer) => throw new NotImplementedException();
    public void Reorder(LayerScope scope, IReadOnlyList<string> orderedManifestPaths) => throw new NotImplementedException();
}
