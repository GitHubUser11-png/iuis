using System;
using System.Drawing;
using System.Windows.Forms;
using IUIS.SharedUI.Base;

namespace IUIS.UserApp.Forms.Employee.Library
{
    public partial class BookReturnDialog : AppDialogBase
    {
        private readonly string _borrowingId;
        
        private Panel _detailsPanel;
        private DateTimePicker _returnDatePicker;
        private ComboBox _conditionComboBox;
        private TextBox _conditionRemarksTextBox;
        private Label _overdueDaysLabel;
        private Label _penaltyLabel;
        private TextBox _internalRemarksTextBox;
        private Button _returnButton;
        private Button _cancelButton;

        public BookReturnDialog(string borrowingId)
        {
            _borrowingId = borrowingId ?? throw new ArgumentNullException(nameof(borrowingId));
            
            InitializeComponent();
            SetupLayout();
            LoadBorrowingDetails();
        }

        private void InitializeComponent()
        {
            this.Text = "Return Book";
            this.Size = new Size(600, 550);
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
                Size = new Size(540, 120),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var returnDateLabel = new Label
            {
                Text = "Return Date:",
                Location = new Point(0, 130),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F)
            };

            _returnDatePicker = new DateTimePicker
            {
                Location = new Point(100, 127),
                Width = 150,
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today
            };

            var conditionLabel = new Label
            {
                Text = "Return Condition:",
                Location = new Point(0, 165),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F)
            };

            _conditionComboBox = new ComboBox
            {
                Location = new Point(100, 162),
                Width = 200,
                Height = 28,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F)
            };
            _conditionComboBox.Items.AddRange(new[] { "Good", "Fair", "Damaged", "Lost" });
            _conditionComboBox.SelectedIndex = 0;

            var conditionRemarksLabel = new Label
            {
                Text = "Condition Remarks:",
                Location = new Point(0, 200),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F)
            };

            _conditionRemarksTextBox = new TextBox
            {
                Location = new Point(0, 225),
                Width = 540,
                Height = 60,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 9F)
            };

            _overdueDaysLabel = new Label
            {
                Text = "Overdue Days: 0",
                Location = new Point(0, 295),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39)
            };

            _penaltyLabel = new Label
            {
                Text = "Outstanding Penalty: ₱0.00",
                Location = new Point(200, 295),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39)
            };

            var internalRemarksLabel = new Label
            {
                Text = "Internal Remarks:",
                Location = new Point(0, 325),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F)
            };

            _internalRemarksTextBox = new TextBox
            {
                Location = new Point(0, 350),
                Width = 540,
                Height = 60,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 9F)
            };

            var buttonPanel = new Panel
            {
                Location = new Point(0, 420),
                Size = new Size(540, 40)
            };

            _returnButton = new Button
            {
                Text = "Return Book",
                Location = new Point(340, 0),
                Size = new Size(90, 35),
                BackColor = Color.FromArgb(79, 70, 229),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F)
            };
            _returnButton.FlatAppearance.BorderSize = 0;
            _returnButton.Click += OnReturnClick;

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

            buttonPanel.Controls.Add(_returnButton);
            buttonPanel.Controls.Add(_cancelButton);

            mainPanel.Controls.Add(_detailsPanel);
            mainPanel.Controls.Add(returnDateLabel);
            mainPanel.Controls.Add(_returnDatePicker);
            mainPanel.Controls.Add(conditionLabel);
            mainPanel.Controls.Add(_conditionComboBox);
            mainPanel.Controls.Add(conditionRemarksLabel);
            mainPanel.Controls.Add(_conditionRemarksTextBox);
            mainPanel.Controls.Add(_overdueDaysLabel);
            mainPanel.Controls.Add(_penaltyLabel);
            mainPanel.Controls.Add(internalRemarksLabel);
            mainPanel.Controls.Add(_internalRemarksTextBox);
            mainPanel.Controls.Add(buttonPanel);

            this.Controls.Add(mainPanel);
        }

        private void LoadBorrowingDetails()
        {
            try
            {
                // TODO: Load actual borrowing details
                // For now, display placeholder
                _detailsPanel.Controls.Clear();

                var labels = new[]
                {
                    ("Borrowing ID", _borrowingId),
                    ("Student", "John Doe (STU-2026-000001)"),
                    ("Book", "Introduction to Algorithms"),
                    ("Issue Date", "01/15/2026"),
                    ("Due Date", "01/29/2026")
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

        private void OnReturnClick(object sender, EventArgs e)
        {
            // TODO: Implement return logic
            MessageBox.Show("Book returned successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.DialogResult = DialogResult.OK;
        }
    }
}
