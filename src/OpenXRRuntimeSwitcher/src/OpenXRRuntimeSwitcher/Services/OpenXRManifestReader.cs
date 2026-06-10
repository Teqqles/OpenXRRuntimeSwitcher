using System.Text.Json;

namespace OpenXRRuntimeSwitcher.Services
{
    public static class OpenXRManifestReader
    {
        public static bool IsValidOpenXRManifest(string manifestPath, out string? errorMessage)
        {
            errorMessage = null;

            try
            {
                if (string.IsNullOrWhiteSpace(manifestPath))
                {
                    errorMessage = "Manifest path is empty.";
                    return false;
                }

                var path = manifestPath.Trim().Trim('"');
                path = Environment.ExpandEnvironmentVariables(path);

                if (!File.Exists(path))
                {
                    errorMessage = "Manifest file does not exist.";
                    return false;
                }

                using var stream = File.OpenRead(path);
                using var doc = JsonDocument.Parse(stream);

                if (!doc.RootElement.TryGetProperty("file_format_version", out _))
                {
                    errorMessage = "Missing required 'file_format_version' field.";
                    return false;
                }

                if (!doc.RootElement.TryGetProperty("runtime", out var runtimeElm) || runtimeElm.ValueKind != JsonValueKind.Object)
                {
                    errorMessage = "Missing or invalid 'runtime' object.";
                    return false;
                }

                if (!runtimeElm.TryGetProperty("library_path", out var libPath) || libPath.ValueKind != JsonValueKind.String)
                {
                    errorMessage = "Missing required 'runtime.library_path' field.";
                    return false;
                }

                var libraryPath = libPath.GetString();
                if (string.IsNullOrWhiteSpace(libraryPath))
                {
                    errorMessage = "The 'runtime.library_path' field is empty.";
                    return false;
                }

                var resolvedLibraryPath = ResolveLibraryPath(path, libraryPath);
                if (!File.Exists(resolvedLibraryPath))
                {
                    errorMessage = $"Runtime library does not exist: {resolvedLibraryPath}";
                    return false;
                }

                return true;
            }
            catch (JsonException ex)
            {
                errorMessage = $"Invalid JSON: {ex.Message}";
                return false;
            }
            catch (Exception ex)
            {
                errorMessage = $"Error reading manifest: {ex.Message}";
                return false;
            }
        }

        private static string ResolveLibraryPath(string manifestPath, string libraryPath)
        {
            var expandedPath = Environment.ExpandEnvironmentVariables(libraryPath);

            if (Path.IsPathRooted(expandedPath))
                return expandedPath;

            var manifestDir = Path.GetDirectoryName(manifestPath);
            if (string.IsNullOrEmpty(manifestDir))
                return expandedPath;

            return Path.GetFullPath(Path.Combine(manifestDir, expandedPath));
        }

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