using Xunit;
using OpenXRRuntimeSwitcher.Services.Abstractions;

namespace OpenXRRuntimeSwitcher.Tests.Integration;

public sealed class RegistryIntegrationTests
{
    [Fact]
    public void Registry_AvailableRuntimes_KeyExists()
    {
        var reg = new WindowsRegistryService();
        var values = reg.ReadKeyValues(@"SOFTWARE\\Khronos\\OpenXR\\1\\AvailableRuntimes");

        Assert.NotNull(values);
    }
}
