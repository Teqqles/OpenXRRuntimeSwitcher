using OpenXRRuntimeSwitcher.Models;
using System.Text;

namespace OpenXRRuntimeSwitcher.Services
{
    /// <summary>
    /// Polls the registry via <see cref="IOpenXRRuntimeService"/> and raises change events.
    /// Runs on a threadpool timer and marshals notifications via the captured SynchronizationContext when provided.
    /// </summary>
    public sealed class RegistryChangeDetector : IDisposable
    {
        private readonly IOpenXRRuntimeService _runtimeService;
        private readonly System.Threading.Timer _timer; // <-- Fully qualify Timer here
        private readonly TimeSpan _interval;
        private readonly SynchronizationContext? _syncContext;

        private string _lastAvailableSignature = string.Empty;
        private string? _lastActiveManifest;

        public event EventHandler<RegistryChangedEventArgs>? Changed;

        public RegistryChangeDetector(IOpenXRRuntimeService runtimeService, TimeSpan pollInterval, SynchronizationContext? syncContext = null)
        {
            _runtimeService = runtimeService ?? throw new ArgumentNullException(nameof(runtimeService));
            _interval = pollInterval;
            _syncContext = syncContext;

            // Timer will be started explicitly by Start() to avoid firing before caller captures initial state.
            _timer = new System.Threading.Timer(TimerCallback, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        }

        public void Start()
        {
            // capture initial state
            try
            {
                var runtimes = _runtimeService.GetAvailableRuntimes();
                _lastAvailableSignature = ComputeSignature(runtimes);
                _lastActiveManifest = _runtimeService.GetActiveRuntimeManifest();
            }
            catch
            {
                // swallow - we'll detect on next tick
                _lastAvailableSignature = string.Empty;
                _lastActiveManifest = null;
            }

            _timer.Change(TimeSpan.Zero, _interval);
        }

        public void Stop() => _timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

        private void TimerCallback(object? state)
        {
            try
            {
                var runtimes = _runtimeService.GetAvailableRuntimes();
                var active = _runtimeService.GetActiveRuntimeManifest();

                var sig = ComputeSignature(runtimes);

                var availableChanged = !string.Equals(sig, _lastAvailableSignature, StringComparison.Ordinal);
                var activeChanged = !string.Equals(active, _lastActiveManifest, StringComparison.OrdinalIgnoreCase);

                if (availableChanged || activeChanged)
                {
                    _lastAvailableSignature = sig;
                    _lastActiveManifest = active;
                    var args = new RegistryChangedEventArgs(availableChanged, activeChanged, runtimes, active);

                    if (_syncContext != null)
                        _syncContext.Post(_ => Changed?.Invoke(this, args), null);
                    else
                        Changed?.Invoke(this, args);
                }
            }
            catch
            {
                // Do not allow timer to die; ignore transient errors
            }
        }

        private static string ComputeSignature(IReadOnlyList<OpenXRRuntime> runtimes)
        {
            if (runtimes == null || runtimes.Count == 0) return string.Empty;
            var sb = new StringBuilder(runtimes.Count * 64);
            foreach (var r in runtimes)
            {
                sb.Append(r.ManifestPath ?? string.Empty).Append(':').Append(r.Name ?? string.Empty).Append('|');
            }
            return sb.ToString();
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }

    public sealed class RegistryChangedEventArgs : EventArgs
    {
        public RegistryChangedEventArgs(bool availableChanged, bool activeChanged, IReadOnlyList<OpenXRRuntime> runtimes, string? activeManifest)
        {
            AvailableChanged = availableChanged;
            ActiveChanged = activeChanged;
            AvailableRuntimes = runtimes;
            ActiveManifest = activeManifest;
        }

        public bool AvailableChanged { get; }
        public bool ActiveChanged { get; }
        public IReadOnlyList<OpenXRRuntime> AvailableRuntimes { get; }
        public string? ActiveManifest { get; }
    }
}