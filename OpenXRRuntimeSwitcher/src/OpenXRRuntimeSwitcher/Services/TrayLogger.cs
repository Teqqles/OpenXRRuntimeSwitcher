using System;
using System.Diagnostics;
using System.IO;

namespace OpenXRRuntimeSwitcher.Services
{
    /// <summary>
    /// Lightweight file + Debug logger used by UI components.
    /// Does not throw.
    /// </summary>
    internal static class TrayLogger
    {
        private static readonly string LogPath = Path.Combine(Path.GetTempPath(), "OpenXRRuntimeSwitcher.log");

        public static void Log(string message)
        {
            try
            {
                var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}";
                Debug.Write(line);
                File.AppendAllText(LogPath, line);
            }
            catch
            {
                // No-op: logging must not throw
            }
        }

        public static void LogException(string context, Exception ex)
        {
            try
            {
                var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR {context}: {ex.GetType().Name} {ex.Message}{Environment.NewLine}{ex.StackTrace}{Environment.NewLine}";
                Debug.Write(line);
                File.AppendAllText(LogPath, line);
            }
            catch
            {
                // No-op
            }
        }
    }
}