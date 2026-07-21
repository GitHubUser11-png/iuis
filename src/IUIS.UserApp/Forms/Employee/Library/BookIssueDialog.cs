using System;
using System.Drawing;
using System.Windows.Forms;
using IUIS.SharedUI.Controls;
using IUIS.SharedUI.DataGridViews;
using IUIS.SharedUI.Forms;
using IUIS.SharedUI.Theme;

namespace IUIS.UserApp.Forms.Employee.Library
{
    public partial class BookIssueDialog : AppDialogBase
    {
        private readonly string _sessionId;
        
        private TextBox _studentSearchTextBox;
        private DataGridView _studentResultsGrid;
        private Panel _studentEligibilityPanel;
        private TextBox _bookSearchTextBox;
        private DataGridView _bookResultsGrid;
        private Panel _bookAvailabilityPanel;
        private DataGridView _currentBorrowingsGrid;
        private DateTimePicker _issueDatePicker;
        private DateTimePicker _dueDatePicker;
        private TextBox _studentRemarksTextBox;
        private TextBox _internalRemarksTextBox;
        private Button _issueButton;
        private Button _cancelButton;

        public BookIssueDialog(string sessionId)
        {
            _sessionId = sessionId ?? throw new ArgumentNullException(nameof(sessionId));
            
            InitializeComponent();
            SetupLayout();
        }

        private void InitializeComponent()
        {
            this.Text = "Issue Book";
            this.Size = new Size(920, 750);
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
            var studentLabel = DialogLayoutHelper.CreateSectionHeader("Student Search");
            studentLabel.Location = new Point(0, y); contentPanel.Controls.Add(studentLabel); y += 30;

            _studentSearchTextBox = DialogLayoutHelper.CreateStandardTextBox();
            _studentSearchTextBox.Width = 400;
            _studentSearchTextBox.Text = "Search by Student ID or Name...";
            _studentSearchTextBox.Location = new Point(0, y);
            _studentSearchTextBox.GotFocus += (s, e) => { if (_studentSearchTextBox.Text == "Search by Student ID or Name...") _studentSearchTextBox.Text = ""; };
            _studentSearchTextBox.LostFocus += (s, e) => { if (string.IsNullOrWhiteSpace(_studentSearchTextBox.Text)) _studentSearchTextBox.Text = "Search by Student ID or Name..."; };
            contentPanel.Controls.Add(_studentSearchTextBox); y += 45;

            _studentResultsGrid = AppDataGridViewFactory.CreateStyledDataGridView();
            _studentResultsGrid.Location = new Point(0, y);
            _studentResultsGrid.Size = new Size(400, 150);
            AppDataGridViewFactory.AddTextBoxColumn(_studentResultsGrid, "StudentId", "ID", 100);
            AppDataGridViewFactory.AddTextBoxColumn(_studentResultsGrid, "Name", "Name", 250);
            contentPanel.Controls.Add(_studentResultsGrid); y += 160;

            _studentEligibilityPanel = new Panel
            {
                Location = new Point(420, y - 205),
                Size = new Size(420, 185),
                BackColor = UiTheme.ElevatedSurface,
                BorderStyle = BorderStyle.FixedSingle
            };
            contentPanel.Controls.Add(_studentEligibilityPanel);

            // Book Section
            var bookLabel = DialogLayoutHelper.CreateSectionHeader("Book Search");
            bookLabel.Location = new Point(0, y); contentPanel.Controls.Add(bookLabel); y += 30;

            _bookSearchTextBox = DialogLayoutHelper.CreateStandardTextBox();
            _bookSearchTextBox.Width = 400;
            _bookSearchTextBox.Text = "Search by ISBN, Title, or Author...";
            _bookSearchTextBox.Location = new Point(0, y);
            _bookSearchTextBox.GotFocus += (s, e) => { if (_bookSearchTextBox.Text == "Search by ISBN, Title, or Author...") _bookSearchTextBox.Text = ""; };
            _bookSearchTextBox.LostFocus += (s, e) => { if (string.IsNullOrWhiteSpace(_bookSearchTextBox.Text)) _bookSearchTextBox.Text = "Search by ISBN, Title, or Author..."; };
            contentPanel.Controls.Add(_bookSearchTextBox); y += 45;

            _bookResultsGrid = AppDataGridViewFactory.CreateStyledDataGridView();
            _bookResultsGrid.Location = new Point(0, y);
            _bookResultsGrid.Size = new Size(400, 150);
            AppDataGridViewFactory.AddTextBoxColumn(_bookResultsGrid, "BookId", "ID", 100);
            AppDataGridViewFactory.AddTextBoxColumn(_bookResultsGrid, "Title", "Title", 200);
            AppDataGridViewFactory.AddTextBoxColumn(_bookResultsGrid, "CopiesAvailable", "Available", 80);
            contentPanel.Controls.Add(_bookResultsGrid); y += 160;

            _bookAvailabilityPanel = new Panel
            {
                Location = new Point(420, y - 205),
                Size = new Size(420, 185),
                BackColor = UiTheme.ElevatedSurface,
                BorderStyle = BorderStyle.FixedSingle
            };
            contentPanel.Controls.Add(_bookAvailabilityPanel);

            // Current Borrowings Section
            var currentBorrowingsLabel = DialogLayoutHelper.CreateSectionHeader("Current Student Borrowings");
            currentBorrowingsLabel.Location = new Point(0, y); contentPanel.Controls.Add(currentBorrowingsLabel); y += 30;

            _currentBorrowingsGrid = AppDataGridViewFactory.CreateStyledDataGridView();
            _currentBorrowingsGrid.Location = new Point(0, y);
            _currentBorrowingsGrid.Size = new Size(840, 100);
            AppDataGridViewFactory.AddTextBoxColumn(_currentBorrowingsGrid, "BookTitle", "Book", 300);
            AppDataGridViewFactory.AddDateColumn(_currentBorrowingsGrid, "DueAtUtc", "Due Date", "MM/dd/yyyy", 120);
            AppDataGridViewFactory.AddTextBoxColumn(_currentBorrowingsGrid, "Status", "Status", 100);
            contentPanel.Controls.Add(_currentBorrowingsGrid); y += 110;

            // Date Fields
            var issueDateLabel = DialogLayoutHelper.CreateFieldLabel("Issue Date");
            issueDateLabel.Location = new Point(0, y); contentPanel.Controls.Add(issueDateLabel);

            _issueDatePicker = DialogLayoutHelper.CreateStandardDatePicker();
            _issueDatePicker.Location = new Point(80, y - 22);
            _issueDatePicker.Value = DateTime.Today;
            contentPanel.Controls.Add(_issueDatePicker);

            var dueDateLabel = DialogLayoutHelper.CreateFieldLabel("Due Date");
            dueDateLabel.Location = new Point(250, y); contentPanel.Controls.Add(dueDateLabel);

            _dueDatePicker = DialogLayoutHelper.CreateStandardDatePicker();
            _dueDatePicker.Location = new Point(320, y - 22);
            _dueDatePicker.Value = DateTime.Today.AddDays(14);
            contentPanel.Controls.Add(_dueDatePicker); y += 50;

            // Remarks
            var studentRemarksLabel = DialogLayoutHelper.CreateFieldLabel("Student-Visible Remarks");
            studentRemarksLabel.Location = new Point(0, y); contentPanel.Controls.Add(studentRemarksLabel);

            _studentRemarksTextBox = DialogLayoutHelper.CreateStandardTextBox();
            _studentRemarksTextBox.Width = 400;
            _studentRemarksTextBox.Location = new Point(0, y + 22);
            contentPanel.Controls.Add(_studentRemarksTextBox);

            var internalRemarksLabel = DialogLayoutHelper.CreateFieldLabel("Internal Remarks");
            internalRemarksLabel.Location = new Point(420, y); contentPanel.Controls.Add(internalRemarksLabel);

            _internalRemarksTextBox = DialogLayoutHelper.CreateStandardTextBox();
            _internalRemarksTextBox.Width = 400;
            _internalRemarksTextBox.Location = new Point(420, y + 22);
            contentPanel.Controls.Add(_internalRemarksTextBox);

            _issueButton = UiTheme.CreatePrimaryButton("Issue Book", 120, UiMetrics.StandardButtonHeight);
            _issueButton.Click += OnIssueClick;

            _cancelButton = UiTheme.CreateSecondaryButton("Cancel", 120, UiMetrics.StandardButtonHeight);
            _cancelButton.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            var buttonPanel = DialogLayoutHelper.CreateButtonPanel(_issueButton, _cancelButton);

            mainPanel.Controls.Add(contentPanel);
            mainPanel.Controls.Add(buttonPanel);
            this.Controls.Add(mainPanel);
        }

        private void OnIssueClick(object sender, EventArgs e)
        {
            // TODO: Implement issue logic
            MessageBox.Show("Book issued successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.DialogResult = DialogResult.OK;
        }
    }
}
