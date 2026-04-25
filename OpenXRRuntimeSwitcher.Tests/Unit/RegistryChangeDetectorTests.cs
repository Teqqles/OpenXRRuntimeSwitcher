using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;
using OpenXRRuntimeSwitcher.Services;
using OpenXRRuntimeSwitcher.Models;

namespace OpenXRRuntimeSwitcher.Tests.Unit;

public sealed class RegistryChangeDetectorTests
{
    private sealed class FakeRuntimeService : IOpenXRRuntimeService
    {
        public IReadOnlyList<OpenXRRuntime> Runtimes { get; set; } = new List<OpenXRRuntime>();
        public string? Active { get; set; }

        public IReadOnlyList<OpenXRRuntime> GetAvailableRuntimes() => Runtimes;
        public string? GetActiveRuntimeManifest() => Active;
        public void SetActiveRuntime(string manifestPath) => Active = manifestPath;
    }

    [Fact]
    public void TimerCallback_RaisesChangedEvent_WhenRuntimesOrActiveChange()
    {
        var fake = new FakeRuntimeService();
        var detector = new RegistryChangeDetector(fake, TimeSpan.FromSeconds(1));

        RegistryChangedEventArgs? received = null;
        detector.Changed += (_, args) => received = args;

        // initial state: nothing
        fake.Runtimes = new List<OpenXRRuntime>();
        fake.Active = null;

        // ensure previous signature is empty so change will be detected
        var lastSignatureField = typeof(RegistryChangeDetector).GetField("_lastAvailableSignature", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(lastSignatureField);
        lastSignatureField!.SetValue(detector, string.Empty);

        // change underlying service
        fake.Runtimes = new List<OpenXRRuntime> { new OpenXRRuntime("Pimax", "C:\\pimax.json") };
        fake.Active = "C:\\pimax.json";

        // invoke private TimerCallback directly
        var mi = typeof(RegistryChangeDetector).GetMethod("TimerCallback", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(mi);
        mi!.Invoke(detector, new object?[] { null });

        Assert.NotNull(received);
        Assert.True(received!.AvailableChanged);
        Assert.True(received.ActiveChanged);
        Assert.Single(received.AvailableRuntimes);
        Assert.Equal("C:\\pimax.json", received.AvailableRuntimes[0].ManifestPath);
        Assert.Equal("C:\\pimax.json", received.ActiveManifest);
    }
}