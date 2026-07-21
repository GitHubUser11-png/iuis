using System;
using System.Drawing;
using System.Windows.Forms;
using IUIS.Application.Abstractions.StudentSelfService;
using IUIS.Application.StudentSelfService.Profile;
using IUIS.SharedUI.Forms;

namespace IUIS.UserApp.Forms.Student.Dialogs
{
    public partial class StudentProfileCorrectionDialog : AppDialogBase
    {
        private readonly IStudentProfileService _profileService;
        private readonly string _sessionId;
        
        private ComboBox _fieldComboBox;
        private TextBox _currentValueTextBox;
        private TextBox _requestedValueTextBox;
        private TextBox _reasonTextBox;
        private Button _submitButton;
        private Button _cancelButton;

        public StudentProfileCorrectionDialog(
            IStudentProfileService profileService,
            string sessionId)
        {
            _profileService = profileService ?? throw new ArgumentNullException(nameof(profileService));
            _sessionId = sessionId ?? throw new ArgumentNullException(nameof(sessionId));
            
            InitializeComponent();
            SetupLayout();
        }

        private void InitializeComponent()
        {
            this.Text = "Request Profile Correction";
            this.Size = new Size(500, 400);
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

            var fieldLabel = new Label
            {
                Text = "Field to Correct:",
                Location = new Point(0, 0),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F)
            };

            _fieldComboBox = new ComboBox
            {
                Location = new Point(0, 25),
                Width = 450,
                Height = 28,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F)
            };

            var fields = StudentProfileCorrectionCatalog.GetCorrectableFields();
            foreach (var field in fields)
            {
                _fieldComboBox.Items.Add(field.Value.DisplayName);
                _fieldComboBox.Tag = field.Key;
            }

            var currentValueLabel = new Label
            {
                Text = "Current Value:",
                Location = new Point(0, 65),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F)
            };

            _currentValueTextBox = new TextBox
            {
                Location = new Point(0, 90),
                Width = 450,
                Height = 28,
                ReadOnly = true,
                BackColor = Color.FromArgb(243, 244, 246),
                Font = new Font("Segoe UI", 9F)
            };

            var requestedValueLabel = new Label
            {
                Text = "Requested Value:",
                Location = new Point(0, 130),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F)
            };

            _requestedValueTextBox = new TextBox
            {
                Location = new Point(0, 155),
                Width = 450,
                Height = 28,
                Font = new Font("Segoe UI", 9F)
            };

            var reasonLabel = new Label
            {
                Text = "Reason for Correction:",
                Location = new Point(0, 195),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F)
            };

            _reasonTextBox = new TextBox
            {
                Location = new Point(0, 220),
                Width = 450,
                Height = 80,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 9F)
            };

            var buttonPanel = new Panel
            {
                Location = new Point(0, 320),
                Size = new Size(450, 40)
            };

            _submitButton = new Button
            {
                Text = "Submit Request",
                Location = new Point(250, 0),
                Size = new Size(90, 35),
                BackColor = Color.FromArgb(79, 70, 229),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F)
            };
            _submitButton.FlatAppearance.BorderSize = 0;
            _submitButton.Click += OnSubmitClick;

            _cancelButton = new Button
            {
                Text = "Cancel",
                Location = new Point(350, 0),
                Size = new Size(90, 35),
                BackColor = Color.FromArgb(229, 231, 235),
                ForeColor = Color.FromArgb(55, 65, 81),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F)
            };
            _cancelButton.FlatAppearance.BorderSize = 0;
            _cancelButton.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            buttonPanel.Controls.Add(_submitButton);
            buttonPanel.Controls.Add(_cancelButton);

            mainPanel.Controls.Add(fieldLabel);
            mainPanel.Controls.Add(_fieldComboBox);
            mainPanel.Controls.Add(currentValueLabel);
            mainPanel.Controls.Add(_currentValueTextBox);
            mainPanel.Controls.Add(requestedValueLabel);
            mainPanel.Controls.Add(_requestedValueTextBox);
            mainPanel.Controls.Add(reasonLabel);
            mainPanel.Controls.Add(_reasonTextBox);
            mainPanel.Controls.Add(buttonPanel);

            this.Controls.Add(mainPanel);
        }

        private void OnSubmitClick(object sender, EventArgs e)
        {
            if (_fieldComboBox.SelectedIndex < 0)
            {
                MessageBox.Show("Please select a field to correct.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(_requestedValueTextBox.Text))
            {
                MessageBox.Show("Please enter the requested value.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(_reasonTextBox.Text))
            {
                MessageBox.Show("Please provide a reason for the correction.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var request = new StudentProfileCorrectionRequest
                {
                    RequestedField = _fieldComboBox.SelectedItem?.ToString(),
                    CurrentValue = _currentValueTextBox.Text,
                    RequestedValue = _requestedValueTextBox.Text,
                    Reason = _reasonTextBox.Text
                };

                var result = _profileService.SubmitCorrectionRequest(_sessionId, request);

                if (result.IsSuccess)
                {
                    MessageBox.Show("Correction request submitted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                }
                else
                {
                    MessageBox.Show($"Failed to submit request: {result.ErrorMessage}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error submitting request: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
