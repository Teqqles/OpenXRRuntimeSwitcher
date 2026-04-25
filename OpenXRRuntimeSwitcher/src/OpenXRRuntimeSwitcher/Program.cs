using OpenXRRuntimeSwitcher.Models;
using OpenXRRuntimeSwitcher.Services;
using OpenXRRuntimeSwitcher.Services.Abstractions;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Windows.Forms;

[assembly: SupportedOSPlatform("windows10.0")]

namespace OpenXRRuntimeSwitcher;

internal static class Program
{
    [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
    private static extern bool FreeConsole();
    
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        
        IRegistryService registryService = new WindowsRegistryService();

        var runtimeService = new OpenXRRuntimeService(registryService);

        var exePath = Process.GetCurrentProcess().MainModule!.FileName;
        var exeDir = Path.GetDirectoryName(exePath)!;
        var configPath = Path.Combine(exeDir, "config.ini");

        var configService = new ConfigService();
        Config config = configService.Load(configPath);

        var hotkeyService = new HotkeyService();

        IRuntimeInfoProvider runtimeInfoProvider = new RuntimeInfoProvider();

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        FreeConsole();
        Application.Run(new TrayApp(runtimeService, hotkeyService, config, runtimeInfoProvider));
    }
}