using System;
using System.Drawing;
using System.Windows.Forms;
using IUIS.Application.Abstractions.StudentSelfService;
using IUIS.Application.StudentSelfService.Scholarship;
using IUIS.SharedUI.Forms;

namespace IUIS.UserApp.Forms.Student.Dialogs
{
    public partial class ScholarshipApplicationDialog : AppDialogBase
    {
        private readonly IStudentScholarshipService _scholarshipService;
        private readonly string _sessionId;
        private readonly string _programId;
        
        private ComboBox _academicYearComboBox;
        private ComboBox _semesterComboBox;
        private TextBox _statementTextBox;
        private Button _submitButton;
        private Button _cancelButton;

        public ScholarshipApplicationDialog(
            IStudentScholarshipService scholarshipService,
            string sessionId,
            string programId)
        {
            _scholarshipService = scholarshipService ?? throw new ArgumentNullException(nameof(scholarshipService));
            _sessionId = sessionId ?? throw new ArgumentNullException(nameof(sessionId));
            _programId = programId ?? throw new ArgumentNullException(nameof(programId));
            
            InitializeComponent();
            SetupLayout();
        }

        private void InitializeComponent()
        {
            this.Text = "Scholarship Application";
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

            var yearLabel = new Label
            {
                Text = "Academic Year:",
                Location = new Point(0, 0),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F)
            };

            _academicYearComboBox = new ComboBox
            {
                Location = new Point(0, 25),
                Width = 450,
                Height = 28,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F)
            };
            _academicYearComboBox.Items.AddRange(new[] { "2025-2026", "2026-2027", "2027-2028" });
            _academicYearComboBox.SelectedIndex = 0;

            var semesterLabel = new Label
            {
                Text = "Semester:",
                Location = new Point(0, 65),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F)
            };

            _semesterComboBox = new ComboBox
            {
                Location = new Point(0, 90),
                Width = 450,
                Height = 28,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F)
            };
            _semesterComboBox.Items.AddRange(new[] { "First Semester", "Second Semester", "Summer" });
            _semesterComboBox.SelectedIndex = 0;

            var statementLabel = new Label
            {
                Text = "Statement of Purpose:",
                Location = new Point(0, 130),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F)
            };

            _statementTextBox = new TextBox
            {
                Location = new Point(0, 155),
                Width = 450,
                Height = 120,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 9F)
            };

            var buttonPanel = new Panel
            {
                Location = new Point(0, 300),
                Size = new Size(450, 40)
            };

            _submitButton = new Button
            {
                Text = "Submit Application",
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

            mainPanel.Controls.Add(yearLabel);
            mainPanel.Controls.Add(_academicYearComboBox);
            mainPanel.Controls.Add(semesterLabel);
            mainPanel.Controls.Add(_semesterComboBox);
            mainPanel.Controls.Add(statementLabel);
            mainPanel.Controls.Add(_statementTextBox);
            mainPanel.Controls.Add(buttonPanel);

            this.Controls.Add(mainPanel);
        }

        private void OnSubmitClick(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_statementTextBox.Text))
            {
                MessageBox.Show("Please provide a statement of purpose.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var request = new StudentScholarshipApplicationRequest
                {
                    ScholarshipProgramId = _programId,
                    AcademicYear = _academicYearComboBox.SelectedItem?.ToString(),
                    Semester = _semesterComboBox.SelectedItem?.ToString(),
                    StatementOfPurpose = _statementTextBox.Text,
                    AdditionalInformation = new System.Collections.Generic.Dictionary<string, string>()
                };

                var result = _scholarshipService.SubmitApplication(_sessionId, request);

                if (result.IsSuccess)
                {
                    MessageBox.Show("Scholarship application submitted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                }
                else
                {
                    MessageBox.Show($"Failed to submit application: {result.ErrorMessage}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error submitting application: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
