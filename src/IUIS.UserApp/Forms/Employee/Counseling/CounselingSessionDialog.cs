using System;
using System.Drawing;
using System.Windows.Forms;
using IUIS.SharedUI.Controls;
using IUIS.SharedUI.DataGridViews;
using IUIS.SharedUI.Forms;
using IUIS.SharedUI.Theme;

namespace IUIS.UserApp.Forms.Employee.Counseling
{
    public partial class CounselingSessionDialog : AppDialogBase
    {
        private readonly string _sessionId;
        private readonly string _sessionIdParam;
        
        private TextBox _studentSearchTextBox;
        private DataGridView _studentResultsGrid;
        private ComboBox _sessionTypeComboBox;
        private ComboBox _priorityComboBox;
        private DateTimePicker _scheduledDatePicker;
        private DateTimePicker _scheduledTimePicker;
        private TextBox _locationTextBox;
        private TextBox _notesTextBox;
        private TextBox _internalRemarksTextBox;
        private Button _saveButton;
        private Button _cancelButton;

        public CounselingSessionDialog(string sessionId, string sessionIdParam = null)
        {
            _sessionId = sessionId ?? throw new ArgumentNullException(nameof(sessionId));
            _sessionIdParam = sessionIdParam;
            
            InitializeComponent();
            SetupLayout();
            
            if (!string.IsNullOrEmpty(_sessionIdParam))
            {
                this.Text = "Edit Counseling Session";
                LoadSessionData();
            }
            else
            {
                this.Text = "Create Counseling Session";
            }
        }

        private void InitializeComponent()
        {
            this.Size = new Size(720, 620);
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

            int y = 0;

            // Student Section
            var studentLabel = DialogLayoutHelper.CreateSectionHeader("Student");
            studentLabel.Location = new Point(0, y); contentPanel.Controls.Add(studentLabel); y += 30;

            _studentSearchTextBox = DialogLayoutHelper.CreateStandardTextBox();
            _studentSearchTextBox.Width = 640;
            _studentSearchTextBox.Text = "Search by Student ID or Name...";
            _studentSearchTextBox.Location = new Point(0, y);
            _studentSearchTextBox.GotFocus += (s, e) => { if (_studentSearchTextBox.Text == "Search by Student ID or Name...") _studentSearchTextBox.Text = ""; };
            _studentSearchTextBox.LostFocus += (s, e) => { if (string.IsNullOrWhiteSpace(_studentSearchTextBox.Text)) _studentSearchTextBox.Text = "Search by Student ID or Name..."; };
            contentPanel.Controls.Add(_studentSearchTextBox); y += 45;

            _studentResultsGrid = AppDataGridViewFactory.CreateStyledDataGridView();
            _studentResultsGrid.Location = new Point(0, y);
            _studentResultsGrid.Size = new Size(640, 100);
            AppDataGridViewFactory.AddTextBoxColumn(_studentResultsGrid, "StudentId", "ID", 120);
            AppDataGridViewFactory.AddTextBoxColumn(_studentResultsGrid, "Name", "Name", 250);
            contentPanel.Controls.Add(_studentResultsGrid); y += 110;

            // Session Details
            var sessionTypeLabel = DialogLayoutHelper.CreateFieldLabel("Session Type", true);
            sessionTypeLabel.Location = new Point(0, y); contentPanel.Controls.Add(sessionTypeLabel);

            _sessionTypeComboBox = DialogLayoutHelper.CreateStandardComboBox();
            _sessionTypeComboBox.Location = new Point(120, y - 22);
            _sessionTypeComboBox.Items.AddRange(new[] { "Individual", "Group", "Crisis Intervention", "Follow-up" });
            _sessionTypeComboBox.SelectedIndex = 0;
            contentPanel.Controls.Add(_sessionTypeComboBox); y += 50;

            var priorityLabel = DialogLayoutHelper.CreateFieldLabel("Priority", true);
            priorityLabel.Location = new Point(0, y); contentPanel.Controls.Add(priorityLabel);

            _priorityComboBox = DialogLayoutHelper.CreateStandardComboBox();
            _priorityComboBox.Location = new Point(120, y - 22);
            _priorityComboBox.Items.AddRange(new[] { "Low", "Medium", "High", "Urgent" });
            _priorityComboBox.SelectedIndex = 1;
            contentPanel.Controls.Add(_priorityComboBox); y += 50;

            var scheduledDateLabel = DialogLayoutHelper.CreateFieldLabel("Scheduled Date", true);
            scheduledDateLabel.Location = new Point(0, y); contentPanel.Controls.Add(scheduledDateLabel);

            _scheduledDatePicker = DialogLayoutHelper.CreateStandardDatePicker();
            _scheduledDatePicker.Location = new Point(120, y - 22);
            _scheduledDatePicker.Value = DateTime.Today;
            contentPanel.Controls.Add(_scheduledDatePicker); y += 50;

            var scheduledTimeLabel = DialogLayoutHelper.CreateFieldLabel("Scheduled Time", true);
            scheduledTimeLabel.Location = new Point(0, y); contentPanel.Controls.Add(scheduledTimeLabel);

            _scheduledTimePicker = new DateTimePicker
            {
                Location = new Point(120, y - 22),
                Width = 150,
                Format = DateTimePickerFormat.Time,
                ShowUpDown = true,
                Value = DateTime.Now,
                Height = UiMetrics.StandardFieldHeight,
                Font = UiTheme.BodyFont
            };
            contentPanel.Controls.Add(_scheduledTimePicker); y += 50;

            var locationLabel = DialogLayoutHelper.CreateFieldLabel("Location", true);
            locationLabel.Location = new Point(0, y); contentPanel.Controls.Add(locationLabel);

            _locationTextBox = DialogLayoutHelper.CreateStandardTextBox();
            _locationTextBox.Width = 520;
            _locationTextBox.Location = new Point(120, y - 22);
            contentPanel.Controls.Add(_locationTextBox); y += 50;

            var notesLabel = DialogLayoutHelper.CreateFieldLabel("Session Notes");
            notesLabel.Location = new Point(0, y); contentPanel.Controls.Add(notesLabel);

            _notesTextBox = DialogLayoutHelper.CreateMultilineTextBox(60);
            _notesTextBox.Width = 640;
            _notesTextBox.Location = new Point(0, y + 22);
            contentPanel.Controls.Add(_notesTextBox); y += 95;

            var internalRemarksLabel = DialogLayoutHelper.CreateFieldLabel("Internal Remarks");
            internalRemarksLabel.Location = new Point(0, y); contentPanel.Controls.Add(internalRemarksLabel);

            _internalRemarksTextBox = DialogLayoutHelper.CreateMultilineTextBox(60);
            _internalRemarksTextBox.Width = 640;
            _internalRemarksTextBox.Location = new Point(0, y + 22);
            contentPanel.Controls.Add(_internalRemarksTextBox);

            _saveButton = UiTheme.CreatePrimaryButton("Save", 120, UiMetrics.StandardButtonHeight);
            _saveButton.Click += OnSaveClick;

            _cancelButton = UiTheme.CreateSecondaryButton("Cancel", 120, UiMetrics.StandardButtonHeight);
            _cancelButton.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            var buttonPanel = DialogLayoutHelper.CreateButtonPanel(_saveButton, _cancelButton);

            mainPanel.Controls.Add(contentPanel);
            mainPanel.Controls.Add(buttonPanel);
            this.Controls.Add(mainPanel);
        }


        private void LoadSessionData()
        {
            // TODO: Load actual session data from service
        }

        private void OnSaveClick(object sender, EventArgs e)
        {
            // TODO: Implement save logic
            MessageBox.Show("Session saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.DialogResult = DialogResult.OK;
        }
    }
}
