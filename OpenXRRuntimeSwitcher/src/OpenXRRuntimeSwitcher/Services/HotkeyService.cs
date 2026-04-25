using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using OpenXRRuntimeSwitcher.Models;

namespace OpenXRRuntimeSwitcher.Services;

public interface IHotkeyService
{
    void RegisterHotkeys(IntPtr handle, Config config, IReadOnlyList<OpenXRRuntime> runtimes);
    bool TryGetActionFromMessage(ref Message m, out string action);
}

public sealed class HotkeyService : IHotkeyService
{
    private readonly Dictionary<int, string> _idToAction = new();
    private int _nextId = 1;

    private const uint MOD_ALT = 0x0001;
    private const uint MOD_CONTROL = 0x0002;
    private const uint MOD_SHIFT = 0x0004;
    private const uint MOD_WIN = 0x0008;
    private const int WM_HOTKEY = 0x0312;

    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    public void RegisterHotkeys(IntPtr handle, Config config, IReadOnlyList<OpenXRRuntime> runtimes)
    {
        foreach (var kvp in config.Mappings)
        {
            var action = kvp.Key;
            var hotkey = kvp.Value;

            if (!TryParseHotkey(hotkey, out var modifiers, out var key))
                continue;

            var id = _nextId++;
            if (RegisterHotKey(handle, id, modifiers, (uint)key))
                _idToAction[id] = action;

            TrayLogger.Log($"Registered hotkey for action '{action}': {hotkey} (id={id})");
        }
    }

    public bool TryGetActionFromMessage(ref Message m, out string action)
    {
        action = string.Empty;
        if (m.Msg != WM_HOTKEY) return false;

        int id = m.WParam.ToInt32();

        TrayLogger.Log($"Received WM_HOTKEY message with id={id}");
        TrayLogger.Log($"Current hotkey mappings: {string.Join(", ", _idToAction)}");

        if (_idToAction.TryGetValue(id, out var act))
        {
            action = act;
            return true;
        }

        return false;
    }

    private static bool TryParseHotkey(string hotkey, out uint modifiers, out Keys key)
    {
        modifiers = 0;
        key = Keys.None;

        var parts = hotkey.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var part in parts)
        {
            switch (part.ToLowerInvariant())
            {
                case "ctrl":
                case "control":
                    modifiers |= MOD_CONTROL;
                    break;
                case "alt":
                    modifiers |= MOD_ALT;
                    break;
                case "shift":
                    modifiers |= MOD_SHIFT;
                    break;
                case "win":
                case "windows":
                    modifiers |= MOD_WIN;
                    break;
                default:
                    if (Enum.TryParse<Keys>(part, true, out var parsed))
                        key = parsed;
                    break;
            }
        }

        TrayLogger.Log($"Parsed hotkey '{hotkey}' into modifiers={modifiers} key={key}");

        return key != Keys.None;
    }
}
