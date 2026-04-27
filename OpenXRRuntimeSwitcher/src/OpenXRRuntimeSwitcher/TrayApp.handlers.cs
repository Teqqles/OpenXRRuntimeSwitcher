using Microsoft.Win32;
using OpenXRRuntimeSwitcher.Services;

namespace OpenXRRuntimeSwitcher
{
    public sealed partial class TrayApp
    {

        // Called when the user clicks "Apply"
        private void OnApplyClicked(object? sender, EventArgs e)
        {
            try
            {
                if (_runtimeCombo.SelectedIndex < 0 || _runtimeCombo.SelectedIndex >= _runtimes.Count)
                    return;

                var runtime = _runtimes[_runtimeCombo.SelectedIndex];
                TrayLogger.Log($"Applying runtime: {runtime.ManifestPath}");
                _runtimeService.SetActiveRuntime(runtime.ManifestPath);

                // Ensure Apply button is rechecked after applying
                UpdateApplyButtonState();
            }
            catch (Exception ex)
            {
                TrayLogger.LogException(nameof(OnApplyClicked), ex);
                MessageBox.Show(this, $"Failed to apply runtime: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Called when the Refresh button is clicked
        private void OnRefreshClicked(object? sender, EventArgs e)
        {
            ManualRefresh();
        }

        // Exposed for the context menu and internal usage
        private void ManualRefresh()
        {
            try
            {
                TrayLogger.Log("Manual refresh requested.");
                LoadRuntimes();
            }
            catch (Exception ex)
            {
                TrayLogger.LogException(nameof(ManualRefresh), ex);
            }
        }

        // Called when the runtime selection changes
        private void OnComboSelectionChanged(object? sender, EventArgs e)
        {
            try
            {
                UpdateRuntimeVisuals();
                UpdateApplyButtonState();
            }
            catch (Exception ex)
            {
                TrayLogger.LogException(nameof(OnComboSelectionChanged), ex);
            }
        }

        // Toggle whether the app runs at current-user startup (HKCU\...\Run)
        // Later on, we will likely only elevate when the user clicks "Apply",
        // for now we will just toggle the registry key and let the user deal with UAC if they have it enabled.
        private void ToggleStartup(bool enable)
        {
            const string runKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
            const string valueName = "OpenXRRuntimeSwitcher";

            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(runKey, writable: true) ?? Registry.CurrentUser.CreateSubKey(runKey);
                if (key is null)
                {
                    TrayLogger.Log("Failed to open/create Run registry key for startup toggle.");
                    return;
                }

                if (enable)
                {
                    var exePath = $"\"{Application.ExecutablePath}\"";
                    key.SetValue(valueName, exePath, RegistryValueKind.String);
                    TrayLogger.Log("Enabled startup.");
                }
                else
                {
                    if (key.GetValueNames().Length > 0 && key.GetValue(valueName) is not null)
                    {
                        key.DeleteValue(valueName, throwOnMissingValue: false);
                        TrayLogger.Log("Disabled startup.");
                    }
                }
            }
            catch (Exception ex)
            {
                TrayLogger.LogException(nameof(ToggleStartup), ex);
                MessageBox.Show(this, $"Failed to update startup setting: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Revert checkbox state to reflect reality - use named handler (designer-safe)
                _startupCheckbox.CheckedChanged -= StartupCheckbox_CheckedChanged;
                _startupCheckbox.Checked = IsStartupEnabled();
                _startupCheckbox.CheckedChanged += StartupCheckbox_CheckedChanged;
            }
        }

        protected override void OnResize(EventArgs e)
        {
            if (FormWindowState.Minimized == this.WindowState)
            {
                this.Hide();
            }

            base.OnResize(e);
        }

        protected override void WndProc(ref Message m)
        {
            if (_hotkeyService.TryGetActionFromMessage(ref m, out var action))
            {
                var resolved = GetRuntime(action);
                TrayLogger.Log($"Hotkey triggered for action: {action}, resolved runtime: {resolved?.Name ?? "None"}");
                if (resolved != null)
                {
                    _runtimeService.SetActiveRuntime(resolved.ManifestPath);
                    try
                    {
                        LoadRuntimes();
                    }
                    catch (Exception ex)
                    {
                        TrayLogger.LogException("Error updating UI after hotkey action", ex);
                    }
                }
            }

            base.WndProc(ref m);
        }
    }
}