using System.IO;
using Xunit;
using OpenXRRuntimeSwitcher.Services;

namespace OpenXRRuntimeSwitcher.Tests.Unit.Services;

public sealed class OpenXRManifestReaderTests
{
    [Fact]
    public void TryReadRuntimeName_ReturnsName_WhenValidManifest()
    {
        var temp = Path.GetTempFileName();
        try
        {
            File.WriteAllText(temp, @"{ ""runtime"": { ""name"": ""My Runtime"" } }");
            var name = OpenXRManifestReader.TryReadRuntimeName(temp);
            Assert.Equal("My Runtime", name);
        }
        finally
        {
            File.Delete(temp);
        }
    }

    [Fact]
    public void TryReadRuntimeName_ReturnsNull_WhenMissingOrMalformed()
    {
        // Missing file
        var missing = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".json");
        var nameMissing = OpenXRManifestReader.TryReadRuntimeName(missing);
        Assert.Null(nameMissing);

        // Malformed JSON
        var temp = Path.GetTempFileName();
        try
        {
            File.WriteAllText(temp, @"{ invalid json ");
            var nameBad = OpenXRManifestReader.TryReadRuntimeName(temp);
            Assert.Null(nameBad);
        }
        finally
        {
            File.Delete(temp);
        }
    }
}

public sealed class OpenXRManifestReaderApiLayerTests
{
    [Fact]
    public void TryReadApiLayerName_ReturnsName_WhenValidManifest()
    {
        var temp = Path.GetTempFileName();
        try
        {
            File.WriteAllText(temp, @"{ ""api_layer"": { ""name"": ""XR_APILAYER_TEST"" } }");
            Assert.Equal("XR_APILAYER_TEST", OpenXRManifestReader.TryReadApiLayerName(temp));
            Assert.Null(OpenXRManifestReader.TryReadRuntimeName(temp));
        }
        finally
        {
            File.Delete(temp);
        }
    }

    [Fact]
    public void ManifestExists_ExpandsEnvironmentVariablesAndQuotes()
    {
        var temp = Path.GetTempFileName();
        try
        {
            Environment.SetEnvironmentVariable("OXR_MANIFEST_TEST_DIR", Path.GetDirectoryName(temp));
            var viaEnv = "\"%OXR_MANIFEST_TEST_DIR%\\" + Path.GetFileName(temp) + "\"";
            Assert.True(OpenXRManifestReader.ManifestExists(viaEnv));
            Assert.False(OpenXRManifestReader.ManifestExists(""));
            Assert.False(OpenXRManifestReader.ManifestExists(temp + ".missing"));
        }
        finally
        {
            Environment.SetEnvironmentVariable("OXR_MANIFEST_TEST_DIR", null);
            File.Delete(temp);
        }
    }
}
