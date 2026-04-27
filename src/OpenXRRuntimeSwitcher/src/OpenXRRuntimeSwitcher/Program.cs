using OpenXRRuntimeSwitcher.Models;
using OpenXRRuntimeSwitcher.Services;
using OpenXRRuntimeSwitcher.Services.Abstractions;
using OpenXRRuntimeSwitcher.Services.UI;
using System.Runtime.Versioning;

[assembly: SupportedOSPlatform("windows10.0")]

namespace OpenXRRuntimeSwitcher;

internal static class Program
{
    [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
    private static extern bool FreeConsole();

    [STAThread]
    private static void Main()
    {
        Application.EnableVisualStyles();
        UIInitialization.ApplySystemColorMode(new ColorModeProvider());
        Application.SetCompatibleTextRenderingDefault(false);
        FreeConsole();

        ApplicationConfiguration.Initialize();

        IRegistryService registryService = new WindowsRegistryService();

        var runtimeService = new OpenXRRuntimeService(registryService);

        var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule!.FileName;
        var exeDir = Path.GetDirectoryName(exePath)!;
        var configPath = Path.Combine(exeDir, "config.ini");

        var configService = new ConfigService();
        Config config = configService.Load(configPath);

        var hotkeyService = new HotkeyService();

        IRuntimeIconResources resources = new RuntimeIconResources();
        IRuntimeInfoProvider runtimeInfoProvider = new RuntimeInfoProvider(new DefaultRuntimeIconFactory(new ColorModeProvider(), resources));

        Application.Run(new TrayApp(runtimeService, hotkeyService, config, runtimeInfoProvider));
    }
}