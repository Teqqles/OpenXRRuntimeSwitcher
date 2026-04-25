using System.Collections.Generic;
using Microsoft.Win32;

namespace OpenXRRuntimeSwitcher.Services.Abstractions;

public sealed class WindowsRegistryService : IRegistryService
{
    public string? ReadValue(string keyPath, string valueName)
    {
        using var key = Registry.LocalMachine.OpenSubKey(keyPath);
        return key?.GetValue(valueName)?.ToString();
    }

    public IReadOnlyDictionary<string, string> ReadKeyValues(string keyPath)
    {
        using var key = Registry.LocalMachine.OpenSubKey(keyPath);
        var dict = new Dictionary<string, string>();

        if (key is null) return dict;

        foreach (var name in key.GetValueNames())
        {
            var value = key.GetValue(name)?.ToString();
            if (value is not null)
                dict[name] = value;
        }

        return dict;
    }

    public void WriteValue(string keyPath, string valueName, string value)
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(keyPath, writable: true);
            key?.SetValue(valueName, value, RegistryValueKind.String);
        }
        catch (UnauthorizedAccessException)
        {
            throw new InvalidOperationException(
                "Administrator privileges are required to change the OpenXR runtime."
            );
        }
    }

}
