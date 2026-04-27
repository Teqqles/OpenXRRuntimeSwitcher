using Microsoft.Win32.TaskScheduler;
using OpenXRRuntimeSwitcher.Services.Abstractions;

namespace OpenXRRuntimeSwitcher.Services;

public sealed class TaskSchedulerService : ITaskSchedulerService
{
    public bool TaskExists(string taskName)
    {
        using var ts = new TaskService();
        return ts.GetTask(taskName) != null;
    }

    public void CreateTask(string taskName, string exePath, string userName)
    {
        using var ts = new TaskService();

        var td = ts.NewTask();
        td.RegistrationInfo.Description = "Automatically starts OpenXRRuntimeSwitcher at logon with elevation.";

        td.Triggers.Add(new LogonTrigger());
        td.Principal.RunLevel = TaskRunLevel.Highest;
        td.Principal.UserId = userName;

        td.Actions.Add(new ExecAction(exePath, null, Path.GetDirectoryName(exePath)));

        ts.RootFolder.RegisterTaskDefinition(taskName, td);
    }

    public void DeleteTask(string taskName)
    {
        using var ts = new TaskService();
        ts.RootFolder.DeleteTask(taskName, false);
    }
}
