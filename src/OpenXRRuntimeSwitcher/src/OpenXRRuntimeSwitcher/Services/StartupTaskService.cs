using OpenXRRuntimeSwitcher.Services.Abstractions;
using System.Security.Principal;

namespace OpenXRRuntimeSwitcher.Services;
public sealed class StartupTaskService
{
    private const string TaskName = "OpenXRRuntimeSwitcher_AutoStart";

    private readonly ITaskSchedulerService _scheduler;
    private readonly string _exePath;
    private readonly string _userName;

    public StartupTaskService(ITaskSchedulerService scheduler, string? exePathOverride = null)
    {
        _scheduler = scheduler;
        _exePath = exePathOverride
                   ?? System.Diagnostics.Process.GetCurrentProcess().MainModule!.FileName
                   ?? throw new InvalidOperationException("Unable to determine executable path.");

        _userName = WindowsIdentity.GetCurrent().Name;
    }

    public bool TaskExists() => _scheduler.TaskExists(TaskName);

    public void CreateTask()
    {
        if (TaskExists())
        {
            TrayLogger.Log("Startup task already exists, skipping creation.");
            return;
        }
        _scheduler.CreateTask(TaskName, _exePath, _userName);
        TrayLogger.Log("Startup task created successfully, with executable path: " + _exePath);
    }

    public void DeleteTask() => _scheduler.DeleteTask(TaskName);
}