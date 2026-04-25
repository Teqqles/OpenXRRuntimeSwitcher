using System;
using System.IO;
using System.Text.Json;

namespace OpenXRRuntimeSwitcher.Services
{
    internal static class OpenXRManifestReader
    {
        /// <summary>
        /// Attempts to read the JSON manifest and extract the "runtime"."name" property.
        /// Returns null when the name is not available or the file cannot be read.
        /// </summary>
        public static string? TryReadRuntimeName(string manifestPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(manifestPath))
                    return null;

                var path = manifestPath.Trim().Trim('"');
                path = Environment.ExpandEnvironmentVariables(path);

                if (!File.Exists(path))
                    return null;

                using var stream = File.OpenRead(path);
                using var doc = JsonDocument.Parse(stream);

                if (doc.RootElement.TryGetProperty("runtime", out var runtimeElm)
                    && runtimeElm.ValueKind == JsonValueKind.Object
                    && runtimeElm.TryGetProperty("name", out var nameElm)
                    && nameElm.ValueKind == JsonValueKind.String)
                {
                    var name = nameElm.GetString();
                    return string.IsNullOrWhiteSpace(name) ? null : name;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}