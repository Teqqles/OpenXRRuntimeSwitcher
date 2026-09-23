using System.Collections.Generic;
using OpenXRRuntimeSwitcher.Services.Abstractions;

namespace OpenXRRuntimeSwitcher.Tests.Fakes;

public sealed class FakeRegistryService : IRegistryService
{
    private readonly Dictionary<string, Dictionary<string, string>> _store = new();
    // Insertion-ordered per (hive|key): Dictionary<string,int> preserves insertion order in .NET.
    private readonly Dictionary<string, Dictionary<string, int>> _dwordStore = new();

    private static string DwordKey(RegistryHive hive, string keyPath) => $"{hive}|{keyPath}";

    public string? ReadValue(string keyPath, string valueName)
    {
        if (_store.TryGetValue(keyPath, out var values) &&
            values.TryGetValue(valueName, out var val))
            return val;
        return null;
    }

    public IReadOnlyDictionary<string, string> ReadKeyValues(string keyPath)
    {
        if (_store.TryGetValue(keyPath, out var values))
            return new Dictionary<string, string>(values);
        return new Dictionary<string, string>();
    }

    public void WriteValue(string keyPath, string valueName, string value)
    {
        if (!_store.ContainsKey(keyPath))
            _store[keyPath] = new Dictionary<string, string>();
        _store[keyPath][valueName] = value;
    }

    public IReadOnlyList<(string Name, int Data)> ReadDwordValues(RegistryHive hive, string keyPath)
    {
        var list = new List<(string, int)>();
        if (_dwordStore.TryGetValue(DwordKey(hive, keyPath), out var values))
            foreach (var kvp in values)
                list.Add((kvp.Key, kvp.Value));
        return list;
    }

    public void WriteDword(RegistryHive hive, string keyPath, string valueName, int data)
    {
        var k = DwordKey(hive, keyPath);
        if (!_dwordStore.ContainsKey(k))
            _dwordStore[k] = new Dictionary<string, int>();
        _dwordStore[k][valueName] = data;
    }

    public void DeleteValue(RegistryHive hive, string keyPath, string valueName)
    {
        if (_dwordStore.TryGetValue(DwordKey(hive, keyPath), out var values))
            values.Remove(valueName);
    }
}
