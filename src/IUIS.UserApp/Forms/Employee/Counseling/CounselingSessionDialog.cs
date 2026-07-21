using System;
using System.Drawing;
using System.Windows.Forms;
using IUIS.SharedUI.Forms;
using IUIS.SharedUI.DataGridViews;

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
            this.Size = new Size(700, 600);
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

            var studentLabel = new Label
            {
                Text = "Student",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                Location = new Point(0, 0),
                AutoSize = true
            };

            _studentSearchTextBox = new TextBox
            {
                Location = new Point(0, 25),
                Width = 400,
                Height = 28,
                Text = "Search by Student ID or Name...",
                Font = new Font("Segoe UI", 9F)
            };
            _studentSearchTextBox.GotFocus += (s, e) => { if (_studentSearchTextBox.Text == "Search by Student ID or Name...") _studentSearchTextBox.Text = ""; };
            _studentSearchTextBox.LostFocus += (s, e) => { if (string.IsNullOrWhiteSpace(_studentSearchTextBox.Text)) _studentSearchTextBox.Text = "Search by Student ID or Name..."; };

            _studentResultsGrid = AppDataGridViewFactory.CreateStyledDataGridView();
            _studentResultsGrid.Location = new Point(0, 60);
            _studentResultsGrid.Size = new Size(640, 100);
            AppDataGridViewFactory.AddTextBoxColumn(_studentResultsGrid, "StudentId", "ID", 120);
            AppDataGridViewFactory.AddTextBoxColumn(_studentResultsGrid, "Name", "Name", 250);

            int y = 175;
            
            AddComboBoxField(mainPanel, "Session Type:", ref _sessionTypeComboBox, new[] { "Individual", "Group", "Crisis Intervention", "Follow-up" }, ref y);
            AddComboBoxField(mainPanel, "Priority:", ref _priorityComboBox, new[] { "Low", "Medium", "High", "Urgent" }, ref y);
            
            var scheduledDateLabel = new Label
            {
                Text = "Scheduled Date:",
                Location = new Point(0, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F)
            };
            
            _scheduledDatePicker = new DateTimePicker
            {
                Location = new Point(120, y - 3),
                Width = 150,
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today
            };
            
            mainPanel.Controls.Add(scheduledDateLabel);
            mainPanel.Controls.Add(_scheduledDatePicker);
            y += 35;
            
            var scheduledTimeLabel = new Label
            {
                Text = "Scheduled Time:",
                Location = new Point(0, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F)
            };
            
            _scheduledTimePicker = new DateTimePicker
            {
                Location = new Point(120, y - 3),
                Width = 150,
                Format = DateTimePickerFormat.Time,
                ShowUpDown = true,
                Value = DateTime.Now
            };
            
            mainPanel.Controls.Add(scheduledTimeLabel);
            mainPanel.Controls.Add(_scheduledTimePicker);
            y += 35;
            
            AddField(mainPanel, "Location:", ref _locationTextBox, ref y);
            
            var notesLabel = new Label
            {
                Text = "Session Notes:",
                Location = new Point(0, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F)
            };
            
            _notesTextBox = new TextBox
            {
                Location = new Point(0, y + 25),
                Width = 640,
                Height = 60,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 9F)
            };
            
            mainPanel.Controls.Add(notesLabel);
            mainPanel.Controls.Add(_notesTextBox);
            y += 95;
            
            var internalRemarksLabel = new Label
            {
                Text = "Internal Remarks:",
                Location = new Point(0, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F)
            };
            
            _internalRemarksTextBox = new TextBox
            {
                Location = new Point(0, y + 25),
                Width = 640,
                Height = 60,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 9F)
            };
            
            mainPanel.Controls.Add(internalRemarksLabel);
            mainPanel.Controls.Add(_internalRemarksTextBox);
            y += 95;
            
            var buttonPanel = new Panel
            {
                Location = new Point(0, y),
                Size = new Size(640, 40)
            };
            
            _saveButton = new Button
            {
                Text = "Save",
                Location = new Point(440, 0),
                Size = new Size(90, 35),
                BackColor = Color.FromArgb(79, 70, 229),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F)
            };
            _saveButton.FlatAppearance.BorderSize = 0;
            _saveButton.Click += OnSaveClick;
            
            _cancelButton = new Button
            {
                Text = "Cancel",
                Location = new Point(540, 0),
                Size = new Size(90, 35),
                BackColor = Color.FromArgb(229, 231, 235),
                ForeColor = Color.FromArgb(55, 65, 81),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F)
            };
            _cancelButton.FlatAppearance.BorderSize = 0;
            _cancelButton.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            
            buttonPanel.Controls.Add(_saveButton);
            buttonPanel.Controls.Add(_cancelButton);
            
            mainPanel.Controls.Add(studentLabel);
            mainPanel.Controls.Add(_studentSearchTextBox);
            mainPanel.Controls.Add(_studentResultsGrid);
            mainPanel.Controls.Add(buttonPanel);
            
            this.Controls.Add(mainPanel);
        }

        private void AddField(Panel panel, string labelText, ref TextBox textBox, ref int y)
        {
            var label = new Label
            {
                Text = labelText,
                Location = new Point(0, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F)
            };
            
            textBox = new TextBox
            {
                Location = new Point(120, y - 3),
                Width = 520,
                Height = 28,
                Font = new Font("Segoe UI", 9F)
            };
            
            panel.Controls.Add(label);
            panel.Controls.Add(textBox);
            y += 35;
        }

        private void AddComboBoxField(Panel panel, string labelText, ref ComboBox comboBox, string[] items, ref int y)
        {
            var label = new Label
            {
                Text = labelText,
                Location = new Point(0, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F)
            };
            
            comboBox = new ComboBox
            {
                Location = new Point(120, y - 3),
                Width = 200,
                Height = 28,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F)
            };
            comboBox.Items.AddRange(items);
            comboBox.SelectedIndex = 0;
            
            panel.Controls.Add(label);
            panel.Controls.Add(comboBox);
            y += 35;
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
