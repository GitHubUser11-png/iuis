using System;
using System.Drawing;
using System.Windows.Forms;
using IUIS.Application.Abstractions.StudentSelfService;
using IUIS.Application.StudentSelfService.Profile;
using IUIS.SharedUI.Controls;
using IUIS.SharedUI.Forms;
using IUIS.SharedUI.Theme;

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
            this.Size = new Size(520, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            UiTheme.ApplyBaseFormStyle(this);
        }

        private void SetupLayout()
        {
            var mainPanel = DialogLayoutHelper.CreateMainPanel();
            var contentPanel = new Panel { Dock = DockStyle.Fill };

            var fieldLabel = DialogLayoutHelper.CreateFieldLabel("Field to Correct", true);
            _fieldComboBox = DialogLayoutHelper.CreateStandardComboBox();
            _fieldComboBox.Width = contentPanel.Width;

            var fields = StudentProfileCorrectionCatalog.GetCorrectableFields();
            foreach (var field in fields)
            {
                _fieldComboBox.Items.Add(field.Value.DisplayName);
                _fieldComboBox.Tag = field.Key;
            }

            var currentValueLabel = DialogLayoutHelper.CreateFieldLabel("Current Value");
            _currentValueTextBox = DialogLayoutHelper.CreateReadOnlyTextBox();
            _currentValueTextBox.Width = contentPanel.Width;

            var requestedValueLabel = DialogLayoutHelper.CreateFieldLabel("Requested Value", true);
            _requestedValueTextBox = DialogLayoutHelper.CreateStandardTextBox();
            _requestedValueTextBox.Width = contentPanel.Width;

            var reasonLabel = DialogLayoutHelper.CreateFieldLabel("Reason for Correction", true);
            _reasonTextBox = DialogLayoutHelper.CreateMultilineTextBox(100);
            _reasonTextBox.Width = contentPanel.Width;

            _submitButton = UiTheme.CreatePrimaryButton("Submit Request", 140, UiMetrics.StandardButtonHeight);
            _submitButton.Click += OnSubmitClick;

            _cancelButton = UiTheme.CreateSecondaryButton("Cancel", 120, UiMetrics.StandardButtonHeight);
            _cancelButton.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            var buttonPanel = DialogLayoutHelper.CreateButtonPanel(_submitButton, _cancelButton);

            int y = 0;
            fieldLabel.Location = new Point(0, y); contentPanel.Controls.Add(fieldLabel); y += 22;
            _fieldComboBox.Location = new Point(0, y); contentPanel.Controls.Add(_fieldComboBox); y += 50;
            currentValueLabel.Location = new Point(0, y); contentPanel.Controls.Add(currentValueLabel); y += 22;
            _currentValueTextBox.Location = new Point(0, y); contentPanel.Controls.Add(_currentValueTextBox); y += 50;
            requestedValueLabel.Location = new Point(0, y); contentPanel.Controls.Add(requestedValueLabel); y += 22;
            _requestedValueTextBox.Location = new Point(0, y); contentPanel.Controls.Add(_requestedValueTextBox); y += 50;
            reasonLabel.Location = new Point(0, y); contentPanel.Controls.Add(reasonLabel); y += 22;
            _reasonTextBox.Location = new Point(0, y); contentPanel.Controls.Add(_reasonTextBox);

            mainPanel.Controls.Add(contentPanel);
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
