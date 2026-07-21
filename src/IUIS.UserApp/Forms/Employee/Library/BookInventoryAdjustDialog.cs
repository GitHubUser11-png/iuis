using System;
using System.Drawing;
using System.Windows.Forms;
using IUIS.SharedUI.Base;

namespace IUIS.UserApp.Forms.Employee.Library
{
    public partial class BookInventoryAdjustDialog : AppDialogBase
    {
        private readonly string _bookId;
        
        private Panel _currentInventoryPanel;
        private NumericUpDown _totalCopiesDeltaNumeric;
        private NumericUpDown _availableCopiesDeltaNumeric;
        private NumericUpDown _maintenanceCopiesDeltaNumeric;
        private NumericUpDown _lostCopiesDeltaNumeric;
        private TextBox _reasonTextBox;
        private Button _adjustButton;
        private Button _cancelButton;

        public BookInventoryAdjustDialog(string bookId)
        {
            _bookId = bookId ?? throw new ArgumentNullException(nameof(bookId));
            
            InitializeComponent();
            SetupLayout();
            LoadCurrentInventory();
        }

        private void InitializeComponent()
        {
            this.Text = "Adjust Book Inventory";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
        }

        private void SetupLayout()
        {
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                AutoScroll = true
            };

            _currentInventoryPanel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(540, 120),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var adjustmentsLabel = new Label
            {
                Text = "Inventory Adjustments",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                Location = new Point(0, 140),
                AutoSize = true
            };

            int y = 170;
            
            AddNumericField(mainPanel, "Total Copies Delta:", ref _totalCopiesDeltaNumeric, ref y);
            AddNumericField(mainPanel, "Available Copies Delta:", ref _availableCopiesDeltaNumeric, ref y);
            AddNumericField(mainPanel, "Maintenance Copies Delta:", ref _maintenanceCopiesDeltaNumeric, ref y);
            AddNumericField(mainPanel, "Lost Copies Delta:", ref _lostCopiesDeltaNumeric, ref y);

            var reasonLabel = new Label
            {
                Text = "Reason (Required):",
                Location = new Point(0, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F)
            };

            _reasonTextBox = new TextBox
            {
                Location = new Point(0, y + 25),
                Width = 540,
                Height = 60,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 9F)
            };

            y += 95;

            var buttonPanel = new Panel
            {
                Location = new Point(0, y),
                Size = new Size(540, 40)
            };

            _adjustButton = new Button
            {
                Text = "Adjust",
                Location = new Point(340, 0),
                Size = new Size(90, 35),
                BackColor = Color.FromArgb(79, 70, 229),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F)
            };
            _adjustButton.FlatAppearance.BorderSize = 0;
            _adjustButton.Click += OnAdjustClick;

            _cancelButton = new Button
            {
                Text = "Cancel",
                Location = new Point(440, 0),
                Size = new Size(90, 35),
                BackColor = Color.FromArgb(229, 231, 235),
                ForeColor = Color.FromArgb(55, 65, 81),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F)
            };
            _cancelButton.FlatAppearance.BorderSize = 0;
            _cancelButton.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            buttonPanel.Controls.Add(_adjustButton);
            buttonPanel.Controls.Add(_cancelButton);

            mainPanel.Controls.Add(_currentInventoryPanel);
            mainPanel.Controls.Add(adjustmentsLabel);
            mainPanel.Controls.Add(reasonLabel);
            mainPanel.Controls.Add(_reasonTextBox);
            mainPanel.Controls.Add(buttonPanel);

            this.Controls.Add(mainPanel);
        }

        private void AddNumericField(Panel panel, string labelText, ref NumericUpDown numeric, ref int y)
        {
            var label = new Label
            {
                Text = labelText,
                Location = new Point(0, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F)
            };

            numeric = new NumericUpDown
            {
                Location = new Point(200, y - 3),
                Width = 100,
                Minimum = -100,
                Maximum = 100,
                Value = 0
            };

            panel.Controls.Add(label);
            panel.Controls.Add(numeric);
            y += 35;
        }

        private void LoadCurrentInventory()
        {
            try
            {
                // TODO: Load actual current inventory from service
                _currentInventoryPanel.Controls.Clear();

                var labels = new[]
                {
                    ("Book ID", _bookId),
                    ("Total Copies", "5"),
                    ("Available Copies", "3"),
                    ("Maintenance Copies", "1"),
                    ("Lost Copies", "1")
                };

                int x = 20;
                int y = 20;
                foreach (var (label, value) in labels)
                {
                    var labelControl = new Label
                    {
                        Text = label,
                        Font = new Font("Segoe UI", 8F),
                        ForeColor = Color.FromArgb(107, 114, 128),
                        Location = new Point(x, y),
                        AutoSize = true
                    };

                    var valueControl = new Label
                    {
                        Text = value,
                        Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                        ForeColor = Color.FromArgb(17, 24, 39),
                        Location = new Point(x, y + 18),
                        AutoSize = true
                    };

                    _currentInventoryPanel.Controls.Add(labelControl);
                    _currentInventoryPanel.Controls.Add(valueControl);

                    x += 270;
                    if (x > 250)
                    {
                        x = 20;
                        y += 45;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading inventory: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnAdjustClick(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_reasonTextBox.Text))
            {
                MessageBox.Show("Please provide a reason for the adjustment.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // TODO: Implement adjustment logic
            MessageBox.Show("Inventory adjusted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.DialogResult = DialogResult.OK;
        }
    }
}
