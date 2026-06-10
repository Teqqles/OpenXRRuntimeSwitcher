using Xunit;
using OpenXRRuntimeSwitcher.Services;

namespace OpenXRRuntimeSwitcher.Tests.Unit.Services;

public sealed class OpenXRManifestReaderValidationTests : IDisposable
{
    private readonly string _testDirectory;

    public OpenXRManifestReaderValidationTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"OpenXRManifestTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
            Directory.Delete(_testDirectory, recursive: true);
    }

    [Fact]
    public void IsValidOpenXRManifest_ValidManifest_ReturnsTrue()
    {
        var binDir = Path.Combine(_testDirectory, "bin", "x64");
        Directory.CreateDirectory(binDir);
        var dllPath = Path.Combine(binDir, "test.dll");
        File.WriteAllText(dllPath, "dummy");

        var manifestPath = Path.Combine(_testDirectory, "valid.json");
        File.WriteAllText(manifestPath, @"{
  ""file_format_version"": ""1.0.0"",
  ""runtime"": {
    ""name"": ""Test Runtime"",
    ""library_path"": ""bin\\x64\\test.dll""
  }
}");

        var result = OpenXRManifestReader.IsValidOpenXRManifest(manifestPath, out var error);

        Assert.True(result);
        Assert.Null(error);
    }

    [Fact]
    public void IsValidOpenXRManifest_MissingFileFormatVersion_ReturnsFalse()
    {
        var manifestPath = Path.Combine(_testDirectory, "no_version.json");
        File.WriteAllText(manifestPath, @"{
  ""runtime"": {
    ""name"": ""Test Runtime"",
    ""library_path"": ""bin\\x64\\test.dll""
  }
}");

        var result = OpenXRManifestReader.IsValidOpenXRManifest(manifestPath, out var error);

        Assert.False(result);
        Assert.Contains("file_format_version", error);
    }

    [Fact]
    public void IsValidOpenXRManifest_MissingRuntime_ReturnsFalse()
    {
        var manifestPath = Path.Combine(_testDirectory, "no_runtime.json");
        File.WriteAllText(manifestPath, @"{
  ""file_format_version"": ""1.0.0""
}");

        var result = OpenXRManifestReader.IsValidOpenXRManifest(manifestPath, out var error);

        Assert.False(result);
        Assert.Contains("runtime", error);
    }

    [Fact]
    public void IsValidOpenXRManifest_MissingLibraryPath_ReturnsFalse()
    {
        var manifestPath = Path.Combine(_testDirectory, "no_lib_path.json");
        File.WriteAllText(manifestPath, @"{
  ""file_format_version"": ""1.0.0"",
  ""runtime"": {
    ""name"": ""Test Runtime""
  }
}");

        var result = OpenXRManifestReader.IsValidOpenXRManifest(manifestPath, out var error);

        Assert.False(result);
        Assert.Contains("library_path", error);
    }

    [Fact]
    public void IsValidOpenXRManifest_InvalidJson_ReturnsFalse()
    {
        var manifestPath = Path.Combine(_testDirectory, "invalid.json");
        File.WriteAllText(manifestPath, "{ invalid json }");

        var result = OpenXRManifestReader.IsValidOpenXRManifest(manifestPath, out var error);

        Assert.False(result);
        Assert.Contains("Invalid JSON", error);
    }

    [Fact]
    public void IsValidOpenXRManifest_NonExistentFile_ReturnsFalse()
    {
        var manifestPath = Path.Combine(_testDirectory, "doesnotexist.json");

        var result = OpenXRManifestReader.IsValidOpenXRManifest(manifestPath, out var error);

        Assert.False(result);
        Assert.Contains("does not exist", error);
    }

    [Fact]
    public void IsValidOpenXRManifest_EmptyPath_ReturnsFalse()
    {
        var result = OpenXRManifestReader.IsValidOpenXRManifest("", out var error);

        Assert.False(result);
        Assert.Contains("empty", error);
    }

    [Fact]
    public void IsValidOpenXRManifest_EmptyLibraryPath_ReturnsFalse()
    {
        var manifestPath = Path.Combine(_testDirectory, "empty_lib.json");
        File.WriteAllText(manifestPath, @"{
  ""file_format_version"": ""1.0.0"",
  ""runtime"": {
    ""name"": ""Test Runtime"",
    ""library_path"": """"
  }
}");

        var result = OpenXRManifestReader.IsValidOpenXRManifest(manifestPath, out var error);

        Assert.False(result);
        Assert.Contains("library_path", error);
        Assert.Contains("empty", error);
    }

    [Fact]
    public void IsValidOpenXRManifest_RuntimeDllDoesNotExist_ReturnsFalse()
    {
        var manifestPath = Path.Combine(_testDirectory, "missing_dll.json");
        File.WriteAllText(manifestPath, @"{
  ""file_format_version"": ""1.0.0"",
  ""runtime"": {
    ""name"": ""Test Runtime"",
    ""library_path"": ""bin\\x64\\nonexistent.dll""
  }
}");

        var result = OpenXRManifestReader.IsValidOpenXRManifest(manifestPath, out var error);

        Assert.False(result);
        Assert.Contains("does not exist", error);
    }

    [Fact]
    public void IsValidOpenXRManifest_RuntimeDllExistsRelativePath_ReturnsTrue()
    {
        var binDir = Path.Combine(_testDirectory, "bin", "x64");
        Directory.CreateDirectory(binDir);
        var dllPath = Path.Combine(binDir, "test.dll");
        File.WriteAllText(dllPath, "dummy");

        var manifestPath = Path.Combine(_testDirectory, "valid_relative.json");
        File.WriteAllText(manifestPath, @"{
  ""file_format_version"": ""1.0.0"",
  ""runtime"": {
    ""name"": ""Test Runtime"",
    ""library_path"": ""bin\\x64\\test.dll""
  }
}");

        var result = OpenXRManifestReader.IsValidOpenXRManifest(manifestPath, out var error);

        Assert.True(result);
        Assert.Null(error);
    }

    [Fact]
    public void IsValidOpenXRManifest_RuntimeDllExistsAbsolutePath_ReturnsTrue()
    {
        var dllPath = Path.Combine(_testDirectory, "absolute.dll");
        File.WriteAllText(dllPath, "dummy");

        var manifestPath = Path.Combine(_testDirectory, "valid_absolute.json");
        var manifestContent = $@"{{
  ""file_format_version"": ""1.0.0"",
  ""runtime"": {{
    ""name"": ""Test Runtime"",
    ""library_path"": ""{dllPath.Replace("\\", "\\\\")}""
  }}
}}";
        File.WriteAllText(manifestPath, manifestContent);

        var result = OpenXRManifestReader.IsValidOpenXRManifest(manifestPath, out var error);

        Assert.True(result);
        Assert.Null(error);
    }
}
