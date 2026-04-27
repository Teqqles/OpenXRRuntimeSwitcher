using System.Collections.Generic;
using OpenXRRuntimeSwitcher.Services.Abstractions;

namespace OpenXRRuntimeSwitcher.Tests.Fakes;

public sealed class FakeRegistryService : IRegistryService
{
    private readonly Dictionary<string, Dictionary<string, string>> _store = new();

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
}
