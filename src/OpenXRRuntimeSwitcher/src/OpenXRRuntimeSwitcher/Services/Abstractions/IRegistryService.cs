namespace OpenXRRuntimeSwitcher.Services.Abstractions;

public enum RegistryHive
{
    LocalMachine,
    CurrentUser
}

public interface IRegistryService
{
    string? ReadValue(string keyPath, string valueName);
    IReadOnlyDictionary<string, string> ReadKeyValues(string keyPath);
    void WriteValue(string keyPath, string valueName, string value);

    IReadOnlyList<(string Name, int Data)> ReadDwordValues(RegistryHive hive, string keyPath);
    void WriteDword(RegistryHive hive, string keyPath, string valueName, int data);
    void DeleteValue(RegistryHive hive, string keyPath, string valueName);
}
