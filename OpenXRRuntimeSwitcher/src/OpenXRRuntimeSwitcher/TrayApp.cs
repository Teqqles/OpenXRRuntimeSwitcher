using Microsoft.Win32;
using OpenXRRuntimeSwitcher.Models;
using OpenXRRuntimeSwitcher.Properties;
using OpenXRRuntimeSwitcher.Services;
using OpenXRRuntimeSwitcher.Services.Abstractions;

namespace OpenXRRuntimeSwitcher
{
    public sealed partial class TrayApp : Form
    {
        private readonly IOpenXRRuntimeService _runtimeService;
        private readonly IHotkeyService _hotkeyService;
        private readonly IRuntimeInfoProvider _runtimeInfoProvider;
        private readonly Config _config;

        private IReadOnlyList<OpenXRRuntime> _runtimes = Array.Empty<OpenXRRuntime>();

        private const int PollIntervalMs = 2000;
        private const string CustomRuntimeText = "Custom Runtime";

        private readonly RegistryChangeDetector _registryDetector;

        public TrayApp(
            IOpenXRRuntimeService runtimeService,
            IHotkeyService hotkeyService,
            Config config,
            IRuntimeInfoProvider runtimeInfoProvider) : this()
        {
            _runtimeService = runtimeService ?? throw new ArgumentNullException(nameof(runtimeService));
            _hotkeyService = hotkeyService ?? throw new ArgumentNullException(nameof(hotkeyService));
            _runtimeInfoProvider = runtimeInfoProvider ?? throw new ArgumentNullException(nameof(runtimeInfoProvider));
            _config = config ?? throw new ArgumentNullException(nameof(config));

            // Create runtime-only NotifyIcon here (removed from the designer partial).
            _trayIcon.Visible = true;
            _trayIcon.Text = "OpenXR Runtime Switcher";
            _trayIcon.ContextMenuStrip = BuildContextMenu();
            _trayIcon.MouseClick += TrayIcon_MouseClick;

            // hide form on shown at runtime
            Shown += OnShownHide;

            LoadRuntimes();
            // Make sure our initial tray icon is set to the current runtime, without a toast notification (since this is just the initial state).
            UpdateRuntimeVisuals(changed: true, noToast: true);

            _hotkeyService.RegisterHotkeys(Handle, config, _runtimes);

            _registryDetector = new RegistryChangeDetector(_runtimeService, TimeSpan.FromMilliseconds(PollIntervalMs), System.Threading.SynchronizationContext.Current);
            _registryDetector.Changed += OnRegistryChanged;
            _registryDetector.Start();

            _startupCheckbox.Checked = IsStartupEnabled();

        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // use correct field name
            _registryDetector.Changed -= OnRegistryChanged;
            _registryDetector.Dispose();
            _trayIcon.Visible = false;
            base.OnFormClosing(e);
        }

        // Load available runtimes into the combo box
        private void LoadRuntimes()
        {
            try
            {
                _runtimes = _runtimeService.GetAvailableRuntimes();
                _runtimeCombo.Items.Clear();

                foreach (var r in _runtimes)
                {
                    _runtimeCombo.Items.Add(string.IsNullOrWhiteSpace(r.Name) ? CustomRuntimeText : r.Name);
                }

                // Try to select the currently active runtime if present, otherwise pick first item.
                var activeManifest = _runtimeService.GetActiveRuntimeManifest() ?? string.Empty;
                var activeIndex = -1;
                if (!string.IsNullOrEmpty(activeManifest))
                {
                    for (var i = 0; i < _runtimes.Count; i++)
                    {
                        if (string.Equals(_runtimes[i].ManifestPath, activeManifest, StringComparison.OrdinalIgnoreCase))
                        {
                            activeIndex = i;
                            break;
                        }
                    }
                }

                if (activeIndex >= 0)
                    _runtimeCombo.SelectedIndex = activeIndex;
                else if (_runtimeCombo.Items.Count > 0)
                    _runtimeCombo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                TrayLogger.LogException(nameof(LoadRuntimes), ex);
            }
        }

        private void UpdateApplyButtonState()
        {
            try
            {
                if (_runtimeCombo.SelectedIndex < 0 || _runtimeCombo.SelectedIndex >= _runtimes.Count)
                {
                    _applyButton.Enabled = false;
                    return;
                }

                var selected = _runtimes[_runtimeCombo.SelectedIndex];
                var active = _runtimeService.GetActiveRuntimeManifest() ?? string.Empty;

                _applyButton.Enabled = !string.Equals(selected.ManifestPath, active, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                TrayLogger.LogException(nameof(UpdateApplyButtonState), ex);
            }
        }

        private OpenXRRuntime? GetRuntime(string? runtimeName)
        {
            return _runtimes.FirstOrDefault(r => r.ManifestPath.Contains(runtimeName ?? "NOT VALID", StringComparison.OrdinalIgnoreCase));
        }

        private RuntimeInfo? GetRuntimeInfo(OpenXRRuntime? runtimeMap)
        {
            var manifestKey = Path.GetFileNameWithoutExtension(runtimeMap?.ManifestPath ?? string.Empty);

            if (!string.IsNullOrEmpty(manifestKey) && _runtimeInfoProvider.TryGetInfo(manifestKey, out var byManifest))
            {
                return byManifest;
            }
            else if (!string.IsNullOrEmpty(runtimeMap?.Name) && _runtimeInfoProvider.TryGetInfo(runtimeMap.Name, out var byName))
            {
                return byName;
            }
            else
            {
                // Fallback fuzzy match against known provider keys
                var all = _runtimeInfoProvider.GetAll();
                foreach (var kv in all)
                {
                    var key = kv.Key ?? string.Empty;
                    if ((!string.IsNullOrEmpty(manifestKey) && manifestKey.IndexOf(key, StringComparison.OrdinalIgnoreCase) >= 0)
                        || (!string.IsNullOrEmpty(runtimeMap?.Name) && runtimeMap.Name.IndexOf(key, StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        return kv.Value;
                    }
                }
            }
            return null;
        }

        private void UpdateRuntimeVisuals(bool changed = false, bool noToast = false)
        {
            try
            {

                var activeManifest = _runtimeService.GetActiveRuntimeManifest() ?? string.Empty;
                var activeRuntime = _runtimes.FirstOrDefault(r => string.Equals(r.ManifestPath, activeManifest, StringComparison.OrdinalIgnoreCase));
                _runtimeFriendlyLabel.Text = activeRuntime is null
                    ? string.Empty
                    : (activeRuntime.Name ?? Path.GetFileNameWithoutExtension(activeRuntime.ManifestPath));

                // If selection is invalid, clear icon and return.
                if (_runtimeCombo.SelectedIndex < 0 || _runtimeCombo.SelectedIndex >= _runtimes.Count)
                {
                    _runtimeIcon.Image = null;
                    return;
                }

                // No change in selected runtime, let's not waste resources updating visuals or showing toast spam.
                //if (_lastSelectedIndex == _runtimeCombo.SelectedIndex)
                //    return;

                OpenXRRuntime selected = _runtimes[_runtimeCombo.SelectedIndex];

                RuntimeInfo? resolved = GetRuntimeInfo(selected);

                _runtimeIcon.Image = resolved?.Icon ?? Resources.UnknownIcon;

                if (changed)
                {
                    if (!noToast)
                        _trayIcon.ShowBalloonTip(2000, "OpenXR Runtime Switched", $"Active runtime: {resolved?.FriendlyName ?? selected.Name ?? CustomRuntimeText}", ToolTipIcon.Info);
                    // Probably a better way to handle this is to store the resolved icon in the RuntimeInfoProvider and
                    // expose it as an Icon type directly, but this works for now.
                    _trayIcon.Icon = Icon.FromHandle(((Bitmap)(resolved?.Icon ?? Resources.UnknownIcon)).GetHicon());
                }
            }
            catch (Exception ex)
            {
                TrayLogger.LogException(nameof(UpdateRuntimeVisuals), ex);
            }
        }

        private void OnRegistryChanged(object? sender, RegistryChangedEventArgs e)
        {
            try
            {
                if (e.AvailableChanged || e.ActiveChanged)
                {
                    LoadRuntimes();
                    UpdateRuntimeVisuals(changed: true);
                }
            }
            catch (Exception ex)
            {
                TrayLogger.LogException(nameof(OnRegistryChanged), ex);
            }
        }

        // Returns whether the app is configured to run at current-user startup
        private bool IsStartupEnabled()
        {
            try
            {
                const string runKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
                const string valueName = "OpenXRRuntimeSwitcher";

                using var key = Registry.CurrentUser.OpenSubKey(runKey, writable: false);
                if (key is null) return false;

                var value = key.GetValue(valueName)?.ToString();
                if (string.IsNullOrWhiteSpace(value)) return false;

                // Normalize stored value and current exe path for comparison
                var stored = value.Trim().Trim('"');
                var exe = Application.ExecutablePath?.Trim().Trim('"') ?? string.Empty;
                return string.Equals(stored, exe, StringComparison.OrdinalIgnoreCase)
                       || Path.GetFileNameWithoutExtension(stored).Equals(Path.GetFileNameWithoutExtension(exe), StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                TrayLogger.LogException(nameof(IsStartupEnabled), ex);
                return false;
            }
        }
    }
}
