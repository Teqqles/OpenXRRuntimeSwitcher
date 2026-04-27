using OpenXRRuntimeSwitcher.Services;
using OpenXRRuntimeSwitcher.Tests.Fakes;

public sealed class StartupTaskServiceTests
{
    private StartupTaskService CreateService(FakeTaskSchedulerService fake)
        => new StartupTaskService(fake);

    [Fact]
    public void TaskExists_ReturnsFalse_WhenNoTask()
    {
        var fake = new FakeTaskSchedulerService();
        var sut = CreateService(fake);

        Assert.False(sut.TaskExists());
    }

    [Fact]
    public void CreateTask_AddsTaskWithCorrectParameters()
    {
        var fake = new FakeTaskSchedulerService();
        var fakeExe = @"C:\Apps\OpenXRRuntimeSwitcher.exe";

        var sut = new StartupTaskService(fake, fakeExe);

        sut.CreateTask();

        Assert.True(fake.TaskExists("OpenXRRuntimeSwitcher_AutoStart"));

        var record = fake.Tasks["OpenXRRuntimeSwitcher_AutoStart"];

        Assert.Equal(fakeExe, record.ExePath);
        Assert.Contains(Environment.UserName, record.UserName);
    }

    [Fact]
    public void DeleteTask_RemovesTask()
    {
        var fake = new FakeTaskSchedulerService();
        var sut = CreateService(fake);

        sut.CreateTask();
        Assert.True(sut.TaskExists());

        sut.DeleteTask();
        Assert.False(sut.TaskExists());
    }

    [Fact]
    public void CreateTask_OverwritesExistingTaskDoesNotHappen()
    {
        var fake = new FakeTaskSchedulerService();
        var sut = CreateService(fake);

        sut.CreateTask();
        var first = fake.Tasks["OpenXRRuntimeSwitcher_AutoStart"];

        sut.CreateTask();
        var second = fake.Tasks["OpenXRRuntimeSwitcher_AutoStart"];

        Assert.Same(first, second);
    }

    [Fact]
    public void DeleteTask_DoesNotThrow_WhenTaskDoesNotExist()
    {
        var fake = new FakeTaskSchedulerService();
        var sut = CreateService(fake);

        var ex = Record.Exception(() => sut.DeleteTask());
        Assert.Null(ex);
    }
}
