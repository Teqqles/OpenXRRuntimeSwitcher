using Xunit;
using OpenXRRuntimeSwitcher.Services;
using OpenXRRuntimeSwitcher.Tests.Fakes;

namespace OpenXRRuntimeSwitcher.Tests.Unit.Services;

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
        var testDir = Path.Combine(Path.GetTempPath(), $"OpenXRTest_{Guid.NewGuid()}");
        try
        {
            Directory.CreateDirectory(testDir);
            var binDir = Path.Combine(testDir, "bin");
            Directory.CreateDirectory(binDir);
            var dllPath = Path.Combine(binDir, "test.dll");
            File.WriteAllText(dllPath, "dummy");

            var manifestPath = Path.Combine(testDir, "test.json");
            File.WriteAllText(manifestPath, @"{
  ""file_format_version"": ""1.0.0"",
  ""runtime"": {
    ""name"": ""Test"",
    ""library_path"": ""bin\\test.dll""
  }
}");

            var fake = new FakeRegistryService();
            var svc = new OpenXRRuntimeService(fake);

            svc.SetActiveRuntime(manifestPath);
            Assert.Equal(manifestPath, svc.GetActiveRuntimeManifest());
        }
        finally
        {
            if (Directory.Exists(testDir))
                Directory.Delete(testDir, recursive: true);
        }
    }

    [Fact] // Boundary
    public void GetAvailableRuntimes_EmptyKey_ReturnsEmptyList()
    {
        var svc = new OpenXRRuntimeService(new FakeRegistryService());
        Assert.Empty(svc.GetAvailableRuntimes());
    }

    [Fact]
    public void SetActiveRuntime_InvalidManifest_ThrowsInvalidOperationException()
    {
        var fake = new FakeRegistryService();
        var svc = new OpenXRRuntimeService(fake);

        var ex = Assert.Throws<InvalidOperationException>(() =>
            svc.SetActiveRuntime("C:\\nonexistent.json"));

        Assert.Contains("invalid runtime manifest", ex.Message);
    }

    [Fact]
    public void SetActiveRuntime_ValidManifest_Succeeds()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"OpenXRTest_{Guid.NewGuid()}");
        try
        {
            Directory.CreateDirectory(testDir);
            var binDir = Path.Combine(testDir, "bin");
            Directory.CreateDirectory(binDir);
            var dllPath = Path.Combine(binDir, "test.dll");
            File.WriteAllText(dllPath, "dummy");

            var manifestPath = Path.Combine(testDir, "valid.json");
            File.WriteAllText(manifestPath, @"{
  ""file_format_version"": ""1.0.0"",
  ""runtime"": {
    ""name"": ""Test"",
    ""library_path"": ""bin\\test.dll""
  }
}");

            var fake = new FakeRegistryService();
            var svc = new OpenXRRuntimeService(fake);

            svc.SetActiveRuntime(manifestPath);

            Assert.Equal(manifestPath, svc.GetActiveRuntimeManifest());
        }
        finally
        {
            if (Directory.Exists(testDir))
                Directory.Delete(testDir, recursive: true);
        }
    }

    [Fact]
    public void SetActiveRuntime_MissingRuntimeDll_ThrowsInvalidOperationException()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"OpenXRTest_{Guid.NewGuid()}");
        try
        {
            Directory.CreateDirectory(testDir);

            var manifestPath = Path.Combine(testDir, "missing_dll.json");
            File.WriteAllText(manifestPath, @"{
  ""file_format_version"": ""1.0.0"",
  ""runtime"": {
    ""name"": ""Test"",
    ""library_path"": ""bin\\nonexistent.dll""
  }
}");

            var fake = new FakeRegistryService();
            var svc = new OpenXRRuntimeService(fake);

            var ex = Assert.Throws<InvalidOperationException>(() =>
                svc.SetActiveRuntime(manifestPath));

            Assert.Contains("does not exist", ex.Message);
        }
        finally
        {
            if (Directory.Exists(testDir))
                Directory.Delete(testDir, recursive: true);
        }
    }
}
