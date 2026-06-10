using OpenXRRuntimeSwitcher.Models;
using OpenXRRuntimeSwitcher.Services;

namespace OpenXRRuntimeSwitcher.Forms;

public sealed class AddCustomRuntimeForm : Form
{
    private readonly TextBox _nameTextBox;
    private readonly TextBox _manifestPathTextBox;
    private readonly TextBox _imagePathTextBox;
    private readonly Button _manifestBrowseButton;
    private readonly Button _imageBrowseButton;
    private readonly Button _okButton;
    private readonly Button _cancelButton;

    public CustomRuntimeDefinition? Result { get; private set; }

    public AddCustomRuntimeForm()
    {
        Text = "Add Custom Runtime";
        Size = new Size(500, 220);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;

        var nameLabel = new Label
        {
            Text = "Name:",
            Location = new Point(12, 15),
            AutoSize = true
        };

        _nameTextBox = new TextBox
        {
            Location = new Point(120, 12),
            Width = 350,
            TabIndex = 0
        };

        var manifestLabel = new Label
        {
            Text = "JSON Manifest:",
            Location = new Point(12, 45),
            AutoSize = true
        };

        _manifestPathTextBox = new TextBox
        {
            Location = new Point(120, 42),
            Width = 280,
            TabIndex = 1
        };

        _manifestBrowseButton = new Button
        {
            Text = "Browse...",
            Location = new Point(405, 40),
            Width = 65,
            TabIndex = 2,
            FlatStyle = FlatStyle.System
        };
        _manifestBrowseButton.Click += ManifestBrowse_Click;

        var imageLabel = new Label
        {
            Text = "Image (Optional):",
            Location = new Point(12, 75),
            AutoSize = true
        };

        _imagePathTextBox = new TextBox
        {
            Location = new Point(120, 72),
            Width = 280,
            TabIndex = 3
        };

        _imageBrowseButton = new Button
        {
            Text = "Browse...",
            Location = new Point(405, 70),
            Width = 65,
            TabIndex = 4,
            FlatStyle = FlatStyle.System
        };
        _imageBrowseButton.Click += ImageBrowse_Click;

        _okButton = new Button
        {
            Text = "OK",
            Location = new Point(310, 140),
            Width = 75,
            TabIndex = 5,
            DialogResult = DialogResult.OK,
            FlatStyle = FlatStyle.System
        };
        _okButton.Click += Ok_Click;

        _cancelButton = new Button
        {
            Text = "Cancel",
            Location = new Point(395, 140),
            Width = 75,
            TabIndex = 6,
            DialogResult = DialogResult.Cancel,
            FlatStyle = FlatStyle.System
        };

        Controls.AddRange(new Control[]
        {
            nameLabel, _nameTextBox,
            manifestLabel, _manifestPathTextBox, _manifestBrowseButton,
            imageLabel, _imagePathTextBox, _imageBrowseButton,
            _okButton, _cancelButton
        });

        AcceptButton = _okButton;
        CancelButton = _cancelButton;
    }

    private void ManifestBrowse_Click(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
            Title = "Select OpenXR Runtime Manifest"
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            _manifestPathTextBox.Text = dialog.FileName;
        }
    }

    private void ImageBrowse_Click(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "Image files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|All files (*.*)|*.*",
            Title = "Select Runtime Icon"
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            _imagePathTextBox.Text = dialog.FileName;
        }
    }

    private void Ok_Click(object? sender, EventArgs e)
    {
        var name = _nameTextBox.Text.Trim();
        var manifestPath = _manifestPathTextBox.Text.Trim();
        var imagePath = _imagePathTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show(this, "Please enter a name for the runtime.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            DialogResult = DialogResult.None;
            return;
        }

        if (string.IsNullOrWhiteSpace(manifestPath))
        {
            MessageBox.Show(this, "Please specify the JSON manifest path.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            DialogResult = DialogResult.None;
            return;
        }

        if (!File.Exists(manifestPath))
        {
            MessageBox.Show(this, "The specified manifest file does not exist.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            DialogResult = DialogResult.None;
            return;
        }

        if (!OpenXRManifestReader.IsValidOpenXRManifest(manifestPath, out var manifestError))
        {
            MessageBox.Show(this,
                $"The selected file is not a valid OpenXR runtime manifest.\n\n{manifestError}\n\nPlease select a valid OpenXR runtime manifest JSON file.",
                "Invalid Manifest",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            DialogResult = DialogResult.None;
            return;
        }

        if (!string.IsNullOrWhiteSpace(imagePath) && !File.Exists(imagePath))
        {
            var result = MessageBox.Show(this,
                "The specified image file does not exist. Continue anyway?",
                "Warning",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
            {
                DialogResult = DialogResult.None;
                return;
            }
        }

        Result = new CustomRuntimeDefinition(
            name,
            manifestPath,
            string.IsNullOrWhiteSpace(imagePath) ? null : imagePath
        );
    }
}
