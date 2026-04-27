namespace OpenXRRuntimeSwitcher.Services.Abstractions;

public interface IRegistryService
{
    string? ReadValue(string keyPath, string valueName);
    IReadOnlyDictionary<string, string> ReadKeyValues(string keyPath);
    void WriteValue(string keyPath, string valueName, string value);
}
