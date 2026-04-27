namespace OpenXRRuntimeSwitcher.Services.Abstractions;

public interface ITaskSchedulerService
{
    bool TaskExists(string taskName);
    void CreateTask(string taskName, string exePath, string userName);
    void DeleteTask(string taskName);
}
