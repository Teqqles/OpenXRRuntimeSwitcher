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

    private IEnumerable<ApiLayer> ReadHive(RegistryHive hive, LayerScope scope) =>
        _registry.ReadDwordValues(hive, ImplicitKey).Select(v => new ApiLayer(
            Scope: scope,
            ManifestPath: v.Name,
            Name: OpenXRManifestReader.TryReadApiLayerName(v.Name) ?? Path.GetFileNameWithoutExtension(v.Name),
            Enabled: v.Data == 0,
            PathExists: OpenXRManifestReader.ManifestExists(v.Name)));

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

        // Requested order first (existing paths only), then any leftovers.
        var final = orderedManifestPaths
            .Where(dataByPath.ContainsKey)
            .Concat(current.Select(v => v.Name))
            .Distinct()
            .ToList();

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
}
