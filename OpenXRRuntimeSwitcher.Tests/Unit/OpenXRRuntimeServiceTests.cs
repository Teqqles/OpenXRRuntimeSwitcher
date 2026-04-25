using Xunit;
using OpenXRRuntimeSwitcher.Services;
using OpenXRRuntimeSwitcher.Tests.Fakes;

namespace OpenXRRuntimeSwitcher.Tests.Unit;

public sealed class OpenXRRuntimeServiceTests
{
    [Fact] // Right
    public void GetAvailableRuntimes_ReturnsCorrectValues()
    {
        var fake = new FakeRegistryService();
        fake.WriteValue(@"SOFTWARE\Khronos\OpenXR\1\AvailableRuntimes", "PimaxXR", "C:\\pimax.json");

        var svc = new OpenXRRuntimeService(fake);
        var runtimes = svc.GetAvailableRuntimes();

        Assert.Single(runtimes);
        Assert.Equal("C:\\pimax.json", runtimes[0].Name);
    }

    [Fact] // Inverse
    public void SetActiveRuntime_ThenGetActiveRuntime_ReturnsSameValue()
    {
        var fake = new FakeRegistryService();
        var svc = new OpenXRRuntimeService(fake);

        svc.SetActiveRuntime("C:\\test.json");
        Assert.Equal("C:\\test.json", svc.GetActiveRuntimeManifest());
    }

    [Fact] // Boundary
    public void GetAvailableRuntimes_EmptyKey_ReturnsEmptyList()
    {
        var svc = new OpenXRRuntimeService(new FakeRegistryService());
        Assert.Empty(svc.GetAvailableRuntimes());
    }
}
