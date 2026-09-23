using System.Collections.Generic;
using OpenXRRuntimeSwitcher.Services.Abstractions;

namespace OpenXRRuntimeSwitcher.Tests.Fakes;

public sealed class FakeRegistryService : IRegistryService
{
    private readonly Dictionary<string, Dictionary<string, string>> _store = new();
    // Insertion-ordered per (hive|key): List preserves insertion order across delete+re-add.
    private readonly Dictionary<string, List<(string Name, int Data)>> _dwordStore = new();

    // Optional failure injection for testing error-recovery paths.
    public Func<string, bool>? ThrowOnWriteDword { get; set; }

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
        if (_dwordStore.TryGetValue(DwordKey(hive, keyPath), out var values))
            return new List<(string, int)>(values);
        return new List<(string, int)>();
    }

    public void WriteDword(RegistryHive hive, string keyPath, string valueName, int data)
    {
        if (ThrowOnWriteDword?.Invoke(valueName) == true)
            throw new InvalidOperationException($"Injected failure: WriteDword({valueName})");

        var k = DwordKey(hive, keyPath);
        if (!_dwordStore.ContainsKey(k))
            _dwordStore[k] = new List<(string, int)>();

        var list = _dwordStore[k];
        var index = list.FindIndex(e => e.Name == valueName);
        if (index >= 0)
            list[index] = (valueName, data);  // Update in place, preserve position
        else
            list.Add((valueName, data));      // Append to end if new
    }

    public void DeleteValue(RegistryHive hive, string keyPath, string valueName)
    {
        if (_dwordStore.TryGetValue(DwordKey(hive, keyPath), out var list))
        {
            var index = list.FindIndex(e => e.Name == valueName);
            if (index >= 0)
                list.RemoveAt(index);  // Remove by index, preserves order of remaining entries
        }
    }
}
