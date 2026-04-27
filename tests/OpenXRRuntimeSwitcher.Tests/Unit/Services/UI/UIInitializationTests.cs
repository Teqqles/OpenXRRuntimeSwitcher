using OpenXRRuntimeSwitcher;
using OpenXRRuntimeSwitcher.Services.UI;
using System.Windows.Forms;
using Xunit;

#pragma warning disable WFO5001
public sealed class UIInitializationTests
{
    private sealed class FakeSetter : IColorModeSetter
    {
        public SystemColorMode? LastSet { get; private set; }

        public void Set(SystemColorMode mode)
        {
            LastSet = mode;
        }
    }

    [Fact]
    public void ApplySystemColorMode_UsesSystemColorMode()
    {
        var fake = new FakeSetter();

        UIInitialization.ApplySystemColorMode(fake);

        Assert.Equal(SystemColorMode.System, fake.LastSet);
    }
}