using OpenXRRuntimeSwitcher.Models;

namespace OpenXRRuntimeSwitcher.Services.Abstractions;

public interface ICustomRuntimeService
{
    IReadOnlyList<CustomRuntimeDefinition> LoadCustomRuntimes(string filePath);
    void SaveCustomRuntimes(string filePath, IReadOnlyList<CustomRuntimeDefinition> runtimes);
    void RegisterCustomRuntimesInRegistry(IReadOnlyList<CustomRuntimeDefinition> runtimes);
    bool AddCustomRuntime(string filePath, CustomRuntimeDefinition runtime);
    bool IsManifestAlreadyRegistered(string manifestPath);
}
