using System;
using System.Drawing;
using System.Windows.Forms;
using IUIS.SharedUI.Base;

namespace IUIS.UserApp.Forms.Employee.Library
{
    public partial class BorrowingRenewDialog : AppDialogBase
    {
        private readonly string _borrowingId;
        
        private Panel _detailsPanel;
        private DateTimePicker _newDueDatePicker;
        private TextBox _remarksTextBox;
        private Label _renewalCountLabel;
        private Button _renewButton;
        private Button _cancelButton;

        public BorrowingRenewDialog(string borrowingId)
        {
            _borrowingId = borrowingId ?? throw new ArgumentNullException(nameof(borrowingId));
            
            InitializeComponent();
            SetupLayout();
            LoadBorrowingDetails();
        }

        private void InitializeComponent()
        {
            this.Text = "Renew Borrowing";
            this.Size = new Size(500, 450);
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
                Padding = new Padding(20)
            };

            _detailsPanel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(440, 120),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            _renewalCountLabel = new Label
            {
                Text = "Renewal Count: 0 of 3",
                Location = new Point(0, 130),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39)
            };

            var newDueDateLabel = new Label
            {
                Text = "New Due Date:",
                Location = new Point(0, 160),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F)
            };

            _newDueDatePicker = new DateTimePicker
            {
                Location = new Point(100, 157),
                Width = 150,
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today.AddDays(14)
            };

            var remarksLabel = new Label
            {
                Text = "Remarks:",
                Location = new Point(0, 195),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F)
            };

            _remarksTextBox = new TextBox
            {
                Location = new Point(0, 220),
                Width = 440,
                Height = 60,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 9F)
            };

            var buttonPanel = new Panel
            {
                Location = new Point(0, 300),
                Size = new Size(440, 40)
            };

            _renewButton = new Button
            {
                Text = "Renew",
                Location = new Point(240, 0),
                Size = new Size(90, 35),
                BackColor = Color.FromArgb(79, 70, 229),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F)
            };
            _renewButton.FlatAppearance.BorderSize = 0;
            _renewButton.Click += OnRenewClick;

            _cancelButton = new Button
            {
                Text = "Cancel",
                Location = new Point(340, 0),
                Size = new Size(90, 35),
                BackColor = Color.FromArgb(229, 231, 235),
                ForeColor = Color.FromArgb(55, 65, 81),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F)
            };
            _cancelButton.FlatAppearance.BorderSize = 0;
            _cancelButton.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            buttonPanel.Controls.Add(_renewButton);
            buttonPanel.Controls.Add(_cancelButton);

            mainPanel.Controls.Add(_detailsPanel);
            mainPanel.Controls.Add(_renewalCountLabel);
            mainPanel.Controls.Add(newDueDateLabel);
            mainPanel.Controls.Add(_newDueDatePicker);
            mainPanel.Controls.Add(remarksLabel);
            mainPanel.Controls.Add(_remarksTextBox);
            mainPanel.Controls.Add(buttonPanel);

            this.Controls.Add(mainPanel);
        }

        private void LoadBorrowingDetails()
        {
            try
            {
                // TODO: Load actual borrowing details from service
                _detailsPanel.Controls.Clear();

                var labels = new[]
                {
                    ("Borrowing ID", _borrowingId),
                    ("Student", "John Doe"),
                    ("Book", "Introduction to Algorithms"),
                    ("Current Due Date", "01/29/2026")
                };

                int y = 20;
                foreach (var (label, value) in labels)
                {
                    var labelControl = new Label
                    {
                        Text = label,
                        Font = new Font("Segoe UI", 8F),
                        ForeColor = Color.FromArgb(107, 114, 128),
                        Location = new Point(20, y),
                        AutoSize = true
                    };

                    var valueControl = new Label
                    {
                        Text = value,
                        Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                        ForeColor = Color.FromArgb(17, 24, 39),
                        Location = new Point(20, y + 18),
                        AutoSize = true
                    };

                    _detailsPanel.Controls.Add(labelControl);
                    _detailsPanel.Controls.Add(valueControl);
                    y += 45;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading borrowing details: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnRenewClick(object sender, EventArgs e)
        {
            // TODO: Implement renew logic
            MessageBox.Show("Borrowing renewed successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.DialogResult = DialogResult.OK;
        }
    }
}
