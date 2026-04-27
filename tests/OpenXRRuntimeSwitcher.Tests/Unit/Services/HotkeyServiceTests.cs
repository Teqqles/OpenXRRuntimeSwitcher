using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using Xunit;
using OpenXRRuntimeSwitcher.Services;

namespace OpenXRRuntimeSwitcher.Tests.Unit.Services;

public sealed class HotkeyServiceTests
{
    [Fact]
    public void TryParseHotkey_PrivateMethod_ParsesKeyAndModifiers()
    {
        var mi = typeof(HotkeyService).GetMethod("TryParseHotkey", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(mi);

        object[] parameters = new object[] { "Ctrl+Alt+F12", 0u, Keys.None };
        var result = (bool)mi!.Invoke(null, parameters)!;

        Assert.True(result);
        var modifiers = (uint)parameters[1]!;
        var key = (Keys)parameters[2]!;
        Assert.Equal(Keys.F12, key);
        Assert.NotEqual(0u, modifiers);
    }

    [Fact]
    public void TryGetActionFromMessage_ReturnsMappedAction()
    {
        var svc = new HotkeyService();

        // inject an id->action mapping via reflection
        var field = typeof(HotkeyService).GetField("_idToAction", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field);
        var map = new Dictionary<int, string> { [42] = "Switch" };
        field!.SetValue(svc, map);

        var msg = Message.Create(nint.Zero, 0x0312, new nint(42), nint.Zero); // WM_HOTKEY = 0x0312
        var ok = svc.TryGetActionFromMessage(ref msg, out var action);

        Assert.True(ok);
        Assert.Equal("Switch", action);

        var notHot = Message.Create(nint.Zero, 0x0100, nint.Zero, nint.Zero); // WM_KEYDOWN
        var nok = svc.TryGetActionFromMessage(ref notHot, out var noAction);
        Assert.False(nok);
        Assert.Equal(string.Empty, noAction);
    }
}