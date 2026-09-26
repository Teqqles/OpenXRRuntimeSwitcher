using System.Linq;
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
            yield return new ApiLayer(
                Scope: scope,
                ManifestPath: path,
                Name: OpenXRManifestReader.TryReadApiLayerName(path) ?? Path.GetFileNameWithoutExtension(path),
                Enabled: data == 0,
                PathExists: OpenXRManifestReader.ManifestExists(path),
                Order: i);
        }
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
        var current = _registry.ReadDwordValues(hive, ImplicitKey).ToList();
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
        // Non-atomic: if a write fails after the deletes, restore the original snapshot
        // so a partial failure never drops the user's layer registrations.
        try
        {
            foreach (var v in current)
                _registry.DeleteValue(hive, ImplicitKey, v.Name);
            foreach (var path in final)
                _registry.WriteDword(hive, ImplicitKey, path, dataByPath[path]);
        }
        catch
        {
            // Best-effort rollback to the pre-Reorder state before rethrowing.
            foreach (var leftover in _registry.ReadDwordValues(hive, ImplicitKey).ToList())
                _registry.DeleteValue(hive, ImplicitKey, leftover.Name);
            foreach (var v in current)
                _registry.WriteDword(hive, ImplicitKey, v.Name, v.Data);
            throw;
        }
    }

    public static bool CanEdit(ApiLayer layer, bool isElevated)
    {
        ArgumentNullException.ThrowIfNull(layer);
        return layer.Scope == LayerScope.User || isElevated;
    }
}
