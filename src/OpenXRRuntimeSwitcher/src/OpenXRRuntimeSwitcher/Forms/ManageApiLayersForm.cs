using System.Linq;
using OpenXRRuntimeSwitcher.Models;
using OpenXRRuntimeSwitcher.Services;
using OpenXRRuntimeSwitcher.Services.Abstractions;

namespace OpenXRRuntimeSwitcher.Forms;

public sealed class ManageApiLayersForm : Form
{
    private readonly IApiLayerService _layers;
    private readonly bool _isElevated;

    private readonly ListView _list;
    private readonly Button _upButton;
    private readonly Button _downButton;
    private readonly Button _toggleButton;
    private readonly Button _deleteButton;
    private readonly Button _refreshButton;
    private readonly Button _closeButton;

    public ManageApiLayersForm(IApiLayerService layerService, IElevationProvider elevationProvider)
    {
        _layers = layerService ?? throw new ArgumentNullException(nameof(layerService));
        ArgumentNullException.ThrowIfNull(elevationProvider);
        _isElevated = elevationProvider.IsElevated;

        Text = "OpenXR API Layers";
        Size = new Size(720, 420);
        FormBorderStyle = FormBorderStyle.Sizable;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;

        _list = new ListView
        {
            Location = new Point(12, 12),
            Size = new Size(580, 360),
            View = View.Details,
            FullRowSelect = true,
            MultiSelect = false,
            HideSelection = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
        };
        _list.Columns.Add("Name", 180);
        _list.Columns.Add("Path", 250);
        _list.Columns.Add("Scope", 70);
        _list.Columns.Add("Enabled", 70);
        _list.SelectedIndexChanged += (_, _) => UpdateButtonStates();

        var buttonX = 604;
        _upButton = MakeButton("Move Up", 12, buttonX); _upButton.Click += (_, _) => MoveSelected(-1);
        _downButton = MakeButton("Move Down", 44, buttonX); _downButton.Click += (_, _) => MoveSelected(1);
        _toggleButton = MakeButton("Enable/Disable", 88, buttonX); _toggleButton.Click += (_, _) => ToggleSelected();
        _deleteButton = MakeButton("Delete", 120, buttonX); _deleteButton.Click += (_, _) => DeleteSelected();
        _refreshButton = MakeButton("Refresh", 164, buttonX); _refreshButton.Click += (_, _) => ReloadLayers();
        _closeButton = MakeButton("Close", 340, buttonX);
        _closeButton.DialogResult = DialogResult.OK;

        Controls.Add(_list);
        Controls.AddRange(new Control[] { _upButton, _downButton, _toggleButton, _deleteButton, _refreshButton, _closeButton });
        CancelButton = _closeButton;

        Load += (_, _) => ReloadLayers();
    }

    private Button MakeButton(string text, int y, int x) => new()
    {
        Text = text,
        Location = new Point(x, y),
        Width = 96,
        FlatStyle = FlatStyle.System,
        Anchor = AnchorStyles.Top | AnchorStyles.Right
    };

    private ApiLayer? Selected =>
        _list.SelectedItems.Count > 0 ? (ApiLayer)_list.SelectedItems[0].Tag! : null;

    private void ReloadLayers()
    {
        _list.BeginUpdate();
        _list.Items.Clear();
        foreach (var layer in _layers.GetLayers())
        {
            var item = new ListViewItem(new[]
            {
                layer.Name,
                layer.ManifestPath,
                layer.Scope == LayerScope.System ? "System" : "User",
                layer.Enabled ? "Yes" : "No"
            })
            {
                Tag = layer,
                ForeColor = layer.PathExists ? SystemColors.WindowText : Color.Crimson
            };
            _list.Items.Add(item);
        }
        _list.EndUpdate();
        UpdateButtonStates();
    }

    private void UpdateButtonStates()
    {
        var layer = Selected;
        var canEdit = layer is not null && layer.IsEditable(_isElevated);
        _toggleButton.Enabled = canEdit;
        _deleteButton.Enabled = canEdit;
        _upButton.Enabled = layer is not null && canEdit && CanMove(layer, -1);
        _downButton.Enabled = layer is not null && canEdit && CanMove(layer, 1);
        _toggleButton.Text = layer is { Enabled: true } ? "Disable" : "Enable";
    }

    private List<string> ScopePaths(LayerScope scope) => _list.Items.Cast<ListViewItem>()
        .Select(i => (ApiLayer)i.Tag!)
        .Where(l => l.Scope == scope)
        .Select(l => l.ManifestPath)
        .ToList();

    // Move is valid only within the same scope (loader reads each hive separately).
    private bool CanMove(ApiLayer layer, int delta)
    {
        var paths = ScopePaths(layer.Scope);
        var target = paths.IndexOf(layer.ManifestPath) + delta;
        return target >= 0 && target < paths.Count;
    }

    private void EditSelected(Action<ApiLayer> edit)
    {
        var layer = Selected;
        if (layer is null || !layer.IsEditable(_isElevated)) return;
        try
        {
            edit(layer);
            ReloadLayers();
            SelectByPath(layer.ManifestPath);
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }
    }

    private void MoveSelected(int delta) => EditSelected(layer =>
    {
        if (!CanMove(layer, delta)) return;
        var paths = ScopePaths(layer.Scope);
        var from = paths.IndexOf(layer.ManifestPath);
        (paths[from], paths[from + delta]) = (paths[from + delta], paths[from]);
        _layers.Reorder(layer.Scope, paths);
    });

    private void ToggleSelected() => EditSelected(layer => _layers.SetEnabled(layer, !layer.Enabled));

    private void DeleteSelected() => EditSelected(layer =>
    {
        var confirm = MessageBox.Show(this,
            $"Remove this OpenXR API layer registration?\n\n{layer.ManifestPath}\n\n" +
            "This deletes the registry entry. To temporarily turn a layer off instead, use Disable.",
            "Delete API Layer", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (confirm == DialogResult.Yes)
            _layers.Delete(layer);
    });

    private void SelectByPath(string manifestPath)
    {
        foreach (ListViewItem item in _list.Items)
        {
            if (((ApiLayer)item.Tag!).ManifestPath == manifestPath)
            {
                item.Selected = true;
                item.EnsureVisible();
                return;
            }
        }
    }

    private void ShowError(Exception ex)
    {
        TrayLogger.LogException(nameof(ManageApiLayersForm), ex);
        MessageBox.Show(this, ex.Message, "OpenXR API Layers",
            MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
