using OpenXRRuntimeSwitcher.Services.Abstractions;

namespace OpenXRRuntimeSwitcher.Tests.Fakes;

public sealed class FakeTaskSchedulerService : ITaskSchedulerService
{
    public sealed record TaskRecord(string TaskName, string ExePath, string UserName);

    public readonly Dictionary<string, TaskRecord> Tasks = new();

    public bool TaskExists(string taskName) => Tasks.ContainsKey(taskName);

    public void CreateTask(string taskName, string exePath, string userName)
    {
        Tasks[taskName] = new TaskRecord(taskName, exePath, userName);
    }

    public void DeleteTask(string taskName)
    {
        Tasks.Remove(taskName);
    }
}
