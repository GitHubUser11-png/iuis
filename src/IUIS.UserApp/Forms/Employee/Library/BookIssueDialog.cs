using System;
using System.Drawing;
using System.Windows.Forms;
using IUIS.SharedUI.Forms;
using IUIS.SharedUI.DataGridViews;

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
            this.Size = new Size(900, 700);
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
                Text = "Student Search",
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
            _studentResultsGrid.Size = new Size(400, 150);
            AppDataGridViewFactory.AddTextBoxColumn(_studentResultsGrid, "StudentId", "ID", 100);
            AppDataGridViewFactory.AddTextBoxColumn(_studentResultsGrid, "Name", "Name", 250);

            _studentEligibilityPanel = new Panel
            {
                Location = new Point(420, 25),
                Size = new Size(420, 185),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var bookLabel = new Label
            {
                Text = "Book Search",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                Location = new Point(0, 225),
                AutoSize = true
            };

            _bookSearchTextBox = new TextBox
            {
                Location = new Point(0, 250),
                Width = 400,
                Height = 28,
                Text = "Search by ISBN, Title, or Author...",
                Font = new Font("Segoe UI", 9F)
            };
            _bookSearchTextBox.GotFocus += (s, e) => { if (_bookSearchTextBox.Text == "Search by ISBN, Title, or Author...") _bookSearchTextBox.Text = ""; };
            _bookSearchTextBox.LostFocus += (s, e) => { if (string.IsNullOrWhiteSpace(_bookSearchTextBox.Text)) _bookSearchTextBox.Text = "Search by ISBN, Title, or Author..."; };

            _bookResultsGrid = AppDataGridViewFactory.CreateStyledDataGridView();
            _bookResultsGrid.Location = new Point(0, 285);
            _bookResultsGrid.Size = new Size(400, 150);
            AppDataGridViewFactory.AddTextBoxColumn(_bookResultsGrid, "BookId", "ID", 100);
            AppDataGridViewFactory.AddTextBoxColumn(_bookResultsGrid, "Title", "Title", 200);
            AppDataGridViewFactory.AddTextBoxColumn(_bookResultsGrid, "CopiesAvailable", "Available", 80);

            _bookAvailabilityPanel = new Panel
            {
                Location = new Point(420, 250),
                Size = new Size(420, 185),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var currentBorrowingsLabel = new Label
            {
                Text = "Current Student Borrowings",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                Location = new Point(0, 450),
                AutoSize = true
            };

            _currentBorrowingsGrid = AppDataGridViewFactory.CreateStyledDataGridView();
            _currentBorrowingsGrid.Location = new Point(0, 475);
            _currentBorrowingsGrid.Size = new Size(840, 100);
            AppDataGridViewFactory.AddTextBoxColumn(_currentBorrowingsGrid, "BookTitle", "Book", 300);
            AppDataGridViewFactory.AddDateColumn(_currentBorrowingsGrid, "DueAtUtc", "Due Date", 120);
            AppDataGridViewFactory.AddTextBoxColumn(_currentBorrowingsGrid, "Status", "Status", 100);

            var issueDateLabel = new Label
            {
                Text = "Issue Date:",
                Location = new Point(0, 585),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F)
            };

            _issueDatePicker = new DateTimePicker
            {
                Location = new Point(80, 582),
                Width = 150,
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today
            };

            var dueDateLabel = new Label
            {
                Text = "Due Date:",
                Location = new Point(250, 585),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F)
            };

            _dueDatePicker = new DateTimePicker
            {
                Location = new Point(320, 582),
                Width = 150,
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today.AddDays(14)
            };

            var studentRemarksLabel = new Label
            {
                Text = "Student-Visible Remarks:",
                Location = new Point(0, 615),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F)
            };

            _studentRemarksTextBox = new TextBox
            {
                Location = new Point(0, 640),
                Width = 400,
                Height = 28,
                Font = new Font("Segoe UI", 9F)
            };

            var internalRemarksLabel = new Label
            {
                Text = "Internal Remarks:",
                Location = new Point(420, 615),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F)
            };

            _internalRemarksTextBox = new TextBox
            {
                Location = new Point(420, 640),
                Width = 400,
                Height = 28,
                Font = new Font("Segoe UI", 9F)
            };

            var buttonPanel = new Panel
            {
                Location = new Point(0, 680),
                Size = new Size(840, 40)
            };

            _issueButton = new Button
            {
                Text = "Issue Book",
                Location = new Point(640, 0),
                Size = new Size(90, 35),
                BackColor = Color.FromArgb(79, 70, 229),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F)
            };
            _issueButton.FlatAppearance.BorderSize = 0;
            _issueButton.Click += OnIssueClick;

            _cancelButton = new Button
            {
                Text = "Cancel",
                Location = new Point(740, 0),
                Size = new Size(90, 35),
                BackColor = Color.FromArgb(229, 231, 235),
                ForeColor = Color.FromArgb(55, 65, 81),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F)
            };
            _cancelButton.FlatAppearance.BorderSize = 0;
            _cancelButton.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            buttonPanel.Controls.Add(_issueButton);
            buttonPanel.Controls.Add(_cancelButton);

            mainPanel.Controls.Add(studentLabel);
            mainPanel.Controls.Add(_studentSearchTextBox);
            mainPanel.Controls.Add(_studentResultsGrid);
            mainPanel.Controls.Add(_studentEligibilityPanel);
            mainPanel.Controls.Add(bookLabel);
            mainPanel.Controls.Add(_bookSearchTextBox);
            mainPanel.Controls.Add(_bookResultsGrid);
            mainPanel.Controls.Add(_bookAvailabilityPanel);
            mainPanel.Controls.Add(currentBorrowingsLabel);
            mainPanel.Controls.Add(_currentBorrowingsGrid);
            mainPanel.Controls.Add(issueDateLabel);
            mainPanel.Controls.Add(_issueDatePicker);
            mainPanel.Controls.Add(dueDateLabel);
            mainPanel.Controls.Add(_dueDatePicker);
            mainPanel.Controls.Add(studentRemarksLabel);
            mainPanel.Controls.Add(_studentRemarksTextBox);
            mainPanel.Controls.Add(internalRemarksLabel);
            mainPanel.Controls.Add(_internalRemarksTextBox);
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
