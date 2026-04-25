using System.IO;
using Xunit;
using OpenXRRuntimeSwitcher.Services;

namespace OpenXRRuntimeSwitcher.Tests.Unit;

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