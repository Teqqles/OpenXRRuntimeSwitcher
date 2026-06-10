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
            catch (InvalidOperationException ex)
            {
                TrayLogger.LogException(nameof(OnApplyClicked), ex);
                MessageBox.Show(this,
                    $"Cannot switch to this runtime:\n\n{ex.Message}\n\nThe runtime manifest may be corrupted or the runtime files may have been moved or deleted.",
                    "Invalid Runtime",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
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

        private void OnComboSelectionChanged(object? sender, EventArgs e)
        {
            try
            {
                if (_runtimeCombo.SelectedItem?.ToString() == AddCustomRuntimeText)
                {
                    ShowAddCustomRuntimeDialog();
                    return;
                }

                UpdateRuntimeVisuals();
                UpdateApplyButtonState();
            }
            catch (Exception ex)
            {
                TrayLogger.LogException(nameof(OnComboSelectionChanged), ex);
            }
        }

        private void ShowAddCustomRuntimeDialog()
        {
            using var dialog = new Forms.AddCustomRuntimeForm();
            if (dialog.ShowDialog(this) == DialogResult.OK && dialog.Result != null)
            {
                try
                {
                    var added = _customRuntimeService.AddCustomRuntime(_customRuntimesPath, dialog.Result);

                    if (!added)
                    {
                        MessageBox.Show(this,
                            $"A runtime with manifest path:\n\n{dialog.Result.ManifestPath}\n\nis already registered.\n\nIt may already be in your custom runtimes list or installed by another application (like SteamVR, Meta, etc.).",
                            "Runtime Already Registered",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        if (_runtimeCombo.Items.Count > 1)
                            _runtimeCombo.SelectedIndex = 0;
                        return;
                    }

                    _customRuntimeService.RegisterCustomRuntimesInRegistry(new[] { dialog.Result });

                    LoadRuntimes();

                    MessageBox.Show(this,
                        $"Custom runtime '{dialog.Result.Name}' has been added successfully.\n\nIt will now appear in the Available Runtimes dropdown.",
                        "Success",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    TrayLogger.LogException(nameof(ShowAddCustomRuntimeDialog), ex);
                    MessageBox.Show(this,
                        $"Failed to add custom runtime: {ex.Message}",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            else
            {
                if (_runtimeCombo.Items.Count > 1)
                    _runtimeCombo.SelectedIndex = 0;
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
                    try
                    {
                        _runtimeService.SetActiveRuntime(resolved.ManifestPath);
                        LoadRuntimes();
                    }
                    catch (InvalidOperationException ex)
                    {
                        TrayLogger.LogException("Hotkey validation error", ex);
                        MessageBox.Show(this,
                            $"Cannot switch to runtime '{resolved.Name}':\n\n{ex.Message}\n\nThe runtime manifest may be corrupted or the runtime files may have been moved or deleted.",
                            "Invalid Runtime",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
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