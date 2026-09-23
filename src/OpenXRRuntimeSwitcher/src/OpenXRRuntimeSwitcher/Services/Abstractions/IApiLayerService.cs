using OpenXRRuntimeSwitcher.Models;

namespace OpenXRRuntimeSwitcher.Services.Abstractions;

public interface IApiLayerService
{
    IReadOnlyList<ApiLayer> GetLayers();
    void SetEnabled(ApiLayer layer, bool enabled);
    void Delete(ApiLayer layer);
    void Reorder(LayerScope scope, IReadOnlyList<string> orderedManifestPaths);
}
