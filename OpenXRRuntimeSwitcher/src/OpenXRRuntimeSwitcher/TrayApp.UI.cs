using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using SystemIcons = System.Drawing.SystemIcons;

namespace OpenXRRuntimeSwitcher
{
    public sealed partial class TrayApp
    {
        // UI controls (declared here to keep layout separate from behavior)
        private NotifyIcon _trayIcon = default!;
        private ComboBox _runtimeCombo = default!;
        private Button _applyButton = default!;
        private Button _refreshButton = default!;
        private Label _warningLabel = default!;
        private CheckBox _startupCheckbox = default!;
        private PictureBox _runtimeIcon = default!;
        private Label _activeRuntimeLabel = default!;
        private Label _availableRuntimesLabel = default!;
        private PictureBox _alertIcon;
        private IContainer components;
        private Label _runtimeFriendlyLabel = default!;

        // Parameterless constructor required by the WinForms designer.
        // Keep this minimal so the designer's CodeDom reader can parse it.
        // The real constructor (defined in `TrayApp.cs`) will initialize runtime-only fields.
        public TrayApp()
        {
            InitializeComponent();
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                MockDataForDesignView();
                return;
            }
        }

        private void InitializeComponent()
        {
            components = new Container();
            ComponentResourceManager resources = new ComponentResourceManager(typeof(TrayApp));
            _runtimeCombo = new ComboBox();
            _applyButton = new Button();
            _refreshButton = new Button();
            _warningLabel = new Label();
            _startupCheckbox = new CheckBox();
            _runtimeIcon = new PictureBox();
            _runtimeFriendlyLabel = new Label();
            _activeRuntimeLabel = new Label();
            _availableRuntimesLabel = new Label();
            _alertIcon = new PictureBox();
            _trayIcon = new NotifyIcon(components);
            ((ISupportInitialize)_runtimeIcon).BeginInit();
            ((ISupportInitialize)_alertIcon).BeginInit();
            SuspendLayout();
            // 
            // _runtimeCombo
            // 
            _runtimeCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            _runtimeCombo.Location = new Point(166, 80);
            _runtimeCombo.Name = "_runtimeCombo";
            _runtimeCombo.Size = new Size(318, 23);
            _runtimeCombo.TabIndex = 0;
            _runtimeCombo.SelectedIndexChanged += OnComboSelectionChanged;
            // 
            // _applyButton
            // 
            _applyButton.Location = new Point(409, 155);
            _applyButton.Name = "_applyButton";
            _applyButton.Size = new Size(75, 23);
            _applyButton.TabIndex = 1;
            _applyButton.Text = "Apply";
            _applyButton.Click += OnApplyClicked;
            // 
            // _refreshButton
            // 
            _refreshButton.Location = new Point(317, 155);
            _refreshButton.Name = "_refreshButton";
            _refreshButton.Size = new Size(75, 23);
            _refreshButton.TabIndex = 2;
            _refreshButton.Text = "Refresh";
            _refreshButton.Click += OnRefreshClicked;
            // 
            // _warningLabel
            // 
            _warningLabel.ForeColor = Color.Crimson;
            _warningLabel.Location = new Point(197, 109);
            _warningLabel.Name = "_warningLabel";
            _warningLabel.Size = new Size(287, 34);
            _warningLabel.TabIndex = 3;
            _warningLabel.Text = "Don't change this if you are already running a VR session!";
            // 
            // _startupCheckbox
            // 
            _startupCheckbox.Location = new Point(12, 155);
            _startupCheckbox.Name = "_startupCheckbox";
            _startupCheckbox.Size = new Size(154, 24);
            _startupCheckbox.TabIndex = 4;
            _startupCheckbox.Text = "Start with Windows";
            _startupCheckbox.CheckedChanged += StartupCheckbox_CheckedChanged;
            // 
            // _runtimeIcon
            // 
            _runtimeIcon.Location = new Point(12, 12);
            _runtimeIcon.Name = "_runtimeIcon";
            _runtimeIcon.Size = new Size(128, 128);
            _runtimeIcon.SizeMode = PictureBoxSizeMode.Zoom;
            _runtimeIcon.TabIndex = 5;
            _runtimeIcon.TabStop = false;
            _runtimeIcon.MouseEnter += RuntimeIcon_MouseEnter;
            _runtimeIcon.MouseLeave += RuntimeIcon_MouseLeave;
            // 
            // _runtimeFriendlyLabel
            // 
            _runtimeFriendlyLabel.Font = new Font("Segoe UI Semibold", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            _runtimeFriendlyLabel.ForeColor = SystemColors.ControlText;
            _runtimeFriendlyLabel.Location = new Point(166, 30);
            _runtimeFriendlyLabel.Name = "_runtimeFriendlyLabel";
            _runtimeFriendlyLabel.Size = new Size(318, 23);
            _runtimeFriendlyLabel.TabIndex = 6;
            // 
            // _activeRuntimeLabel
            // 
            _activeRuntimeLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            _activeRuntimeLabel.Location = new Point(166, 12);
            _activeRuntimeLabel.Name = "_activeRuntimeLabel";
            _activeRuntimeLabel.Size = new Size(318, 18);
            _activeRuntimeLabel.TabIndex = 7;
            _activeRuntimeLabel.Text = "Active Runtime:";
            // 
            // _availableRuntimesLabel
            // 
            _availableRuntimesLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            _availableRuntimesLabel.Location = new Point(166, 59);
            _availableRuntimesLabel.Name = "_availableRuntimesLabel";
            _availableRuntimesLabel.Size = new Size(318, 18);
            _availableRuntimesLabel.TabIndex = 8;
            _availableRuntimesLabel.Text = "Available Runtimes:";
            // 
            // _alertIcon
            // 
            _alertIcon.Image = (Image)resources.GetObject("_alertIcon.Image");
            _alertIcon.Location = new Point(159, 109);
            _alertIcon.Name = "_alertIcon";
            _alertIcon.Size = new Size(32, 32);
            _alertIcon.SizeMode = PictureBoxSizeMode.Zoom;
            _alertIcon.TabIndex = 9;
            _alertIcon.TabStop = false;
            // 
            // _trayIcon
            // 
            _trayIcon.Icon = (Icon)resources.GetObject("_trayIcon.Icon");
            // 
            // TrayApp
            // 
            ClientSize = new Size(496, 190);
            Controls.Add(_alertIcon);
            Controls.Add(_availableRuntimesLabel);
            Controls.Add(_activeRuntimeLabel);
            Controls.Add(_runtimeCombo);
            Controls.Add(_applyButton);
            Controls.Add(_refreshButton);
            Controls.Add(_warningLabel);
            Controls.Add(_startupCheckbox);
            Controls.Add(_runtimeIcon);
            Controls.Add(_runtimeFriendlyLabel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = true;
            Name = "TrayApp";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "OpenXR Runtime Switcher";
            ((ISupportInitialize)_runtimeIcon).EndInit();
            ((ISupportInitialize)_alertIcon).EndInit();
            ResumeLayout(false);
        }

        private void MockDataForDesignView()
        {
            // Populate the runtime combo with sample data for the designer view.
            _runtimeCombo.Items.Clear();
            _runtimeCombo.Items.AddRange(new object[] { "Sample Runtime 1", "Sample Runtime 2" });
            _runtimeCombo.SelectedIndex = 0;
            // Set sample text for labels and checkbox.
            _warningLabel.Text = "Warning: A session is currently active!";
            _warningLabel.Visible = true;
            _runtimeFriendlyLabel.Text = "Sample Runtime 1";
            _runtimeIcon.Image = SystemIcons.Application.ToBitmap();
        }

        private ContextMenuStrip BuildContextMenu()
        {
            var menu = new ContextMenuStrip();
            menu.Items.Add("Open", null, MenuOpen_Click);
            menu.Items.Add("Refresh", null, MenuRefresh_Click);
            menu.Items.Add("Exit", null, MenuExit_Click);
            return menu;
        }

        // Event handler methods - avoid inline lambdas so the WinForms designer's CodeDom reader can parse the file.
        private void TrayIcon_MouseClick(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                ShowWindow();
        }

        private void OnShownHide(object? sender, System.EventArgs e)
        {
            Hide();
        }

        private void StartupCheckbox_CheckedChanged(object? sender, System.EventArgs e)
        {
            ToggleStartup(_startupCheckbox.Checked);
        }

        private void RuntimeIcon_MouseEnter(object? sender, System.EventArgs e)
        {
            if (_runtimeIcon.Image != null)
                _runtimeIcon.Image = TintIcon(_runtimeIcon.Image, Color.White);
        }

        private void RuntimeIcon_MouseLeave(object? sender, System.EventArgs e)
        {
            // restore visuals from the main update logic
            UpdateRuntimeVisuals();
        }

        private void MenuOpen_Click(object? sender, System.EventArgs e) => ShowWindow();
        private void MenuRefresh_Click(object? sender, System.EventArgs e) => ManualRefresh();
        private void MenuExit_Click(object? sender, System.EventArgs e) => Application.Exit();

        private void ShowWindow()
        {
            Show();
            WindowState = FormWindowState.Normal;
            Activate();
        }

        private Image TintIcon(Image original, Color tint)
        {
            var tinted = new Bitmap(original.Width, original.Height);
            using (var g = Graphics.FromImage(tinted))
            {
                var colorMatrix = new System.Drawing.Imaging.ColorMatrix
                {
                    Matrix00 = tint.R / 255f,
                    Matrix11 = tint.G / 255f,
                    Matrix22 = tint.B / 255f,
                    Matrix33 = 1f
                };
                var attributes = new System.Drawing.Imaging.ImageAttributes();
                attributes.SetColorMatrix(colorMatrix);
                g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
                    0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);
            }
            return tinted;
        }
    }
}