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
        private bool ToggleStartup(bool enable)
        {
            try
            {
                if (enable)
                    _startupTaskService.CreateTask();
                else
                    _startupTaskService.DeleteTask();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to update startup task:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                TrayLogger.LogException("Error toggling startup task", ex);

                // Revert checkbox to actual state
                return _startupTaskService.TaskExists();
            }

            return _startupTaskService.TaskExists();
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