using Xunit;
using OpenXRRuntimeSwitcher.Models;
using OpenXRRuntimeSwitcher.Services;
using OpenXRRuntimeSwitcher.Tests.Fakes;

namespace OpenXRRuntimeSwitcher.Tests.Unit.Services;

public sealed class CustomRuntimeServiceTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly string _testJsonPath;

    public CustomRuntimeServiceTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"OpenXRRuntimeSwitcherTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
        _testJsonPath = Path.Combine(_testDirectory, "test_custom_runtimes.json");
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
            Directory.Delete(_testDirectory, recursive: true);
    }

    [Fact]
    public void LoadCustomRuntimes_NonExistentFile_ReturnsEmptyList()
    {
        var fake = new FakeRegistryService();
        var service = new CustomRuntimeService(fake);

        var result = service.LoadCustomRuntimes(Path.Combine(_testDirectory, "nonexistent.json"));

        Assert.Empty(result);
    }

    [Fact]
    public void SaveCustomRuntimes_ThenLoad_ReturnsOriginalData()
    {
        var fake = new FakeRegistryService();
        var service = new CustomRuntimeService(fake);

        var testManifest1 = Path.Combine(_testDirectory, "test1.json");
        var testManifest2 = Path.Combine(_testDirectory, "test2.json");
        var testIcon = Path.Combine(_testDirectory, "icon1.png");

        var runtimes = new List<CustomRuntimeDefinition>
        {
            new("Test Runtime 1", testManifest1, testIcon),
            new("Test Runtime 2", testManifest2, null)
        };

        service.SaveCustomRuntimes(_testJsonPath, runtimes);
        var loaded = service.LoadCustomRuntimes(_testJsonPath);

        Assert.Equal(2, loaded.Count);
        Assert.Equal("Test Runtime 1", loaded[0].Name);
        Assert.Equal(testManifest1, loaded[0].ManifestPath);
        Assert.Equal(testIcon, loaded[0].ImagePath);
        Assert.Equal("Test Runtime 2", loaded[1].Name);
        Assert.Null(loaded[1].ImagePath);
    }

    [Fact]
    public void AddCustomRuntime_AppendsToExistingList()
    {
        var fake = new FakeRegistryService();
        var service = new CustomRuntimeService(fake);

        var runtime1Path = Path.Combine(_testDirectory, "runtime1.json");
        var runtime2Path = Path.Combine(_testDirectory, "runtime2.json");

        var initial = new List<CustomRuntimeDefinition>
        {
            new("Runtime 1", runtime1Path)
        };
        service.SaveCustomRuntimes(_testJsonPath, initial);

        var newRuntime = new CustomRuntimeDefinition("Runtime 2", runtime2Path);
        service.AddCustomRuntime(_testJsonPath, newRuntime);

        var loaded = service.LoadCustomRuntimes(_testJsonPath);
        Assert.Equal(2, loaded.Count);
        Assert.Equal("Runtime 1", loaded[0].Name);
        Assert.Equal("Runtime 2", loaded[1].Name);
    }

    [Fact]
    public void AddCustomRuntime_EmptyFile_CreatesNewList()
    {
        var fake = new FakeRegistryService();
        var service = new CustomRuntimeService(fake);

        var runtimePath = Path.Combine(_testDirectory, "first.json");
        var newRuntime = new CustomRuntimeDefinition("First Runtime", runtimePath);
        service.AddCustomRuntime(_testJsonPath, newRuntime);

        var loaded = service.LoadCustomRuntimes(_testJsonPath);
        Assert.Single(loaded);
        Assert.Equal("First Runtime", loaded[0].Name);
    }

    [Fact]
    public void LoadCustomRuntimes_InvalidJson_ReturnsEmptyList()
    {
        File.WriteAllText(_testJsonPath, "{ invalid json content");

        var fake = new FakeRegistryService();
        var service = new CustomRuntimeService(fake);

        var result = service.LoadCustomRuntimes(_testJsonPath);

        Assert.Empty(result);
    }

    [Fact]
    public void RegisterCustomRuntimesInRegistry_ValidManifest_WritesToRegistry()
    {
        var testManifestPath = Path.Combine(_testDirectory, "test_manifest.json");
        File.WriteAllText(testManifestPath, "{}");

        var fake = new FakeRegistryService();
        var service = new CustomRuntimeService(fake);

        var runtimes = new List<CustomRuntimeDefinition>
        {
            new("Test Runtime", testManifestPath)
        };

        service.RegisterCustomRuntimesInRegistry(runtimes);

        var registryValue = fake.ReadValue(@"SOFTWARE\Khronos\OpenXR\1\AvailableRuntimes", testManifestPath);
        Assert.Equal("0", registryValue);
    }

    [Fact]
    public void RegisterCustomRuntimesInRegistry_NonExistentManifest_SkipsRuntime()
    {
        var fake = new FakeRegistryService();
        var service = new CustomRuntimeService(fake);

        var nonExistentPath = Path.Combine(_testDirectory, "nonexistent.json");
        var runtimes = new List<CustomRuntimeDefinition>
        {
            new("Missing Runtime", nonExistentPath)
        };

        service.RegisterCustomRuntimesInRegistry(runtimes);

        var registryValue = fake.ReadValue(@"SOFTWARE\Khronos\OpenXR\1\AvailableRuntimes", nonExistentPath);
        Assert.Null(registryValue);
    }

    [Fact]
    public void RegisterCustomRuntimesInRegistry_EmptyManifestPath_SkipsRuntime()
    {
        var fake = new FakeRegistryService();
        var service = new CustomRuntimeService(fake);

        var runtimes = new List<CustomRuntimeDefinition>
        {
            new("Invalid Runtime", "")
        };

        service.RegisterCustomRuntimesInRegistry(runtimes);

        var allValues = fake.ReadKeyValues(@"SOFTWARE\Khronos\OpenXR\1\AvailableRuntimes");
        Assert.Empty(allValues);
    }

    [Fact]
    public void RegisterCustomRuntimesInRegistry_MultipleRuntimes_RegistersAll()
    {
        var testManifest1 = Path.Combine(_testDirectory, "manifest1.json");
        var testManifest2 = Path.Combine(_testDirectory, "manifest2.json");
        File.WriteAllText(testManifest1, "{}");
        File.WriteAllText(testManifest2, "{}");

        var fake = new FakeRegistryService();
        var service = new CustomRuntimeService(fake);

        var runtimes = new List<CustomRuntimeDefinition>
        {
            new("Runtime 1", testManifest1),
            new("Runtime 2", testManifest2)
        };

        service.RegisterCustomRuntimesInRegistry(runtimes);

        var value1 = fake.ReadValue(@"SOFTWARE\Khronos\OpenXR\1\AvailableRuntimes", testManifest1);
        var value2 = fake.ReadValue(@"SOFTWARE\Khronos\OpenXR\1\AvailableRuntimes", testManifest2);

        Assert.Equal("0", value1);
        Assert.Equal("0", value2);
    }

    [Fact]
    public void AddCustomRuntime_DuplicateManifestPath_DoesNotAddDuplicate()
    {
        var fake = new FakeRegistryService();
        var service = new CustomRuntimeService(fake);

        var runtimePath = Path.Combine(_testDirectory, "runtime.json");
        var runtime1 = new CustomRuntimeDefinition("Runtime 1", runtimePath);
        var runtime2 = new CustomRuntimeDefinition("Runtime 2 (Duplicate)", runtimePath);

        service.AddCustomRuntime(_testJsonPath, runtime1);
        service.AddCustomRuntime(_testJsonPath, runtime2);

        var loaded = service.LoadCustomRuntimes(_testJsonPath);
        Assert.Single(loaded);
        Assert.Equal("Runtime 1", loaded[0].Name);
    }

    [Fact]
    public void AddCustomRuntime_DuplicateManifestPathDifferentCase_DoesNotAddDuplicate()
    {
        var fake = new FakeRegistryService();
        var service = new CustomRuntimeService(fake);

        var runtimePath = Path.Combine(_testDirectory, "runtime.json");
        var runtime1 = new CustomRuntimeDefinition("Runtime 1", runtimePath.ToLower());
        var runtime2 = new CustomRuntimeDefinition("Runtime 2", runtimePath.ToUpper());

        service.AddCustomRuntime(_testJsonPath, runtime1);
        service.AddCustomRuntime(_testJsonPath, runtime2);

        var loaded = service.LoadCustomRuntimes(_testJsonPath);
        Assert.Single(loaded);
    }

    [Fact]
    public void RegisterCustomRuntimesInRegistry_RuntimeAlreadyInRegistry_SkipsRegistration()
    {
        var testManifest = Path.Combine(_testDirectory, "manifest.json");
        File.WriteAllText(testManifest, "{}");

        var fake = new FakeRegistryService();
        fake.WriteValue(@"SOFTWARE\Khronos\OpenXR\1\AvailableRuntimes", testManifest, "0");

        var service = new CustomRuntimeService(fake);

        var runtimes = new List<CustomRuntimeDefinition>
        {
            new("Test Runtime", testManifest)
        };

        service.RegisterCustomRuntimesInRegistry(runtimes);

        var allValues = fake.ReadKeyValues(@"SOFTWARE\Khronos\OpenXR\1\AvailableRuntimes");
        Assert.Single(allValues);
    }

    [Fact]
    public void AddCustomRuntime_ManifestAlreadyInRegistry_ReturnsFalse()
    {
        var testManifest = Path.Combine(_testDirectory, "manifest.json");
        File.WriteAllText(testManifest, "{}");

        var fake = new FakeRegistryService();
        fake.WriteValue(@"SOFTWARE\Khronos\OpenXR\1\AvailableRuntimes", testManifest, "0");

        var service = new CustomRuntimeService(fake);
        var runtime = new CustomRuntimeDefinition("Test Runtime", testManifest);

        var added = service.AddCustomRuntime(_testJsonPath, runtime);

        Assert.False(added);
        var loaded = service.LoadCustomRuntimes(_testJsonPath);
        Assert.Empty(loaded);
    }

    [Fact]
    public void IsManifestAlreadyRegistered_ManifestInRegistry_ReturnsTrue()
    {
        var testManifest = Path.Combine(_testDirectory, "manifest.json");
        var fake = new FakeRegistryService();
        fake.WriteValue(@"SOFTWARE\Khronos\OpenXR\1\AvailableRuntimes", testManifest, "0");

        var service = new CustomRuntimeService(fake);

        Assert.True(service.IsManifestAlreadyRegistered(testManifest));
    }

    [Fact]
    public void IsManifestAlreadyRegistered_ManifestNotInRegistry_ReturnsFalse()
    {
        var testManifest = Path.Combine(_testDirectory, "manifest.json");
        var fake = new FakeRegistryService();

        var service = new CustomRuntimeService(fake);

        Assert.False(service.IsManifestAlreadyRegistered(testManifest));
    }
}
