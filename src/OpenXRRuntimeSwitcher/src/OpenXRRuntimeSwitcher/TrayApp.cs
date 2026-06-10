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
        private readonly ICustomRuntimeService _customRuntimeService;
        private readonly IRuntimeIconFactory _iconFactory;
        private readonly IConfigService _configService;
        private readonly Config _config;
        private readonly StartupTaskService _startupTaskService = new(new TaskSchedulerService());
        private readonly string _customRuntimesPath;
        private bool _savedDisableToastState;

        private IReadOnlyList<OpenXRRuntime> _runtimes = Array.Empty<OpenXRRuntime>();

        private const int PollIntervalMs = 2000;
        private const string CustomRuntimeText = "Custom Runtime";
        private const string AddCustomRuntimeText = "Add Custom Runtime...";

        private readonly RegistryChangeDetector _registryDetector;

        public TrayApp(
            IOpenXRRuntimeService runtimeService,
            IHotkeyService hotkeyService,
            IConfigService configService,
            Config config,
            IRuntimeInfoProvider runtimeInfoProvider,
            ICustomRuntimeService customRuntimeService,
            IRuntimeIconFactory iconFactory,
            string customRuntimesPath) : this()
        {
            _runtimeService = runtimeService ?? throw new ArgumentNullException(nameof(runtimeService));
            _hotkeyService = hotkeyService ?? throw new ArgumentNullException(nameof(hotkeyService));
            _configService = configService ?? throw new ArgumentNullException(nameof(configService));
            _runtimeInfoProvider = runtimeInfoProvider ?? throw new ArgumentNullException(nameof(runtimeInfoProvider));
            _customRuntimeService = customRuntimeService ?? throw new ArgumentNullException(nameof(customRuntimeService));
            _iconFactory = iconFactory ?? throw new ArgumentNullException(nameof(iconFactory));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _customRuntimesPath = customRuntimesPath;

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

            TrayLogger.Log("Checking for existing startup: " + (_startupTaskService.TaskExists() ? "Exists" : "Does not exist"));
            _startupCheckbox.Checked = _startupTaskService.TaskExists();
            _disableToastCheckbox.Checked = _config.DisableToast;
            _savedDisableToastState = _config.DisableToast;
        }

        private void SaveDisableToastSetting(bool disableToast)
        {
            if (disableToast == _savedDisableToastState)
                return;

            try
            {
                _configService.UpdateDisableToast(disableToast);
                _savedDisableToastState = disableToast;
            }
            catch (Exception ex)
            {
                TrayLogger.LogException(nameof(SaveDisableToastSetting), ex);
                MessageBox.Show(this, $"Failed to save toast setting: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // use correct field name
            _registryDetector.Changed -= OnRegistryChanged;
            _registryDetector.Dispose();
            _trayIcon.Visible = false;
            base.OnFormClosing(e);
        }

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

                _runtimeCombo.Items.Add(AddCustomRuntimeText);

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
                else if (_runtimeCombo.Items.Count > 1)
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

                _runtimeIcon.Image = resolved?.Icon ?? _iconFactory.GetUnknownIcon();

                if (changed)
                {
                    if (!noToast && !_savedDisableToastState)
                        _trayIcon.ShowBalloonTip(2000, "OpenXR Runtime Switched", $"Active runtime: {resolved?.FriendlyName ?? selected.Name ?? CustomRuntimeText}", ToolTipIcon.Info);

                    _trayIcon.Icon = Icon.FromHandle(((Bitmap)(resolved?.Icon ?? _iconFactory.GetUnknownIcon())).GetHicon());
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
    }
}
