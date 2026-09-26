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

    public void WriteValue(string keyPath, string valueName, string value) =>
        RequireAdmin("the OpenXR runtime", () =>
        {
            using var key = Registry.LocalMachine.OpenSubKey(keyPath, writable: true);
            key?.SetValue(valueName, value, RegistryValueKind.String);
        });

    private static RegistryKey RootFor(RegistryHive hive) =>
        hive == RegistryHive.CurrentUser ? Registry.CurrentUser : Registry.LocalMachine;

    public IReadOnlyList<(string Name, int Data)> ReadDwordValues(RegistryHive hive, string keyPath)
    {
        using var key = RootFor(hive).OpenSubKey(keyPath);
        var list = new List<(string, int)>();

        if (key is null) return list;

        foreach (var name in key.GetValueNames())
        {
            if (string.IsNullOrEmpty(name)) continue;
            var raw = key.GetValue(name);
            var data = raw is int i ? i : (int.TryParse(raw?.ToString(), out var p) ? p : 0);
            list.Add((name, data));
        }

        return list;
    }

    public void WriteDword(RegistryHive hive, string keyPath, string valueName, int data) =>
        RequireAdmin("OpenXR API layers", () =>
        {
            using var key = RootFor(hive).CreateSubKey(keyPath, writable: true);
            key.SetValue(valueName, data, RegistryValueKind.DWord);
        });

    public void DeleteValue(RegistryHive hive, string keyPath, string valueName) =>
        RequireAdmin("OpenXR API layers", () =>
        {
            using var key = RootFor(hive).OpenSubKey(keyPath, writable: true);
            key?.DeleteValue(valueName, throwOnMissingValue: false);
        });

    private static void RequireAdmin(string target, Action write)
    {
        try
        {
            write();
        }
        catch (UnauthorizedAccessException)
        {
            throw new InvalidOperationException($"Administrator privileges are required to change {target}.");
        }
    }
}
