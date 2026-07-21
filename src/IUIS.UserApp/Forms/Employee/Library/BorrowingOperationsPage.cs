using System;
using System.Drawing;
using System.Windows.Forms;
using IUIS.SharedUI.Controls;
using IUIS.SharedUI.DataGridViews;

namespace IUIS.UserApp.Forms.Employee.Library
{
    public partial class BorrowingOperationsPage : UserControl
    {
        private readonly string _sessionId;
        
        private TabControl _tabControl;
        private FilterBarControl _filterBar;
        private DataGridView _borrowingsGrid;
        private Button _issueBookButton;
        private Button _refreshButton;

        public BorrowingOperationsPage(string sessionId)
        {
            _sessionId = sessionId ?? throw new ArgumentNullException(nameof(sessionId));
            
            InitializeComponent();
            SetupLayout();
            LoadBorrowings();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(1200, 800);
            this.BackColor = Color.FromArgb(249, 250, 251);
        }

        private void SetupLayout()
        {
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                BackColor = Color.FromArgb(249, 250, 251)
            };

            var headerPanel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(1160, 40)
            };

            var headerLabel = new Label
            {
                Text = "Borrowing Operations",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                Location = new Point(0, 5),
                AutoSize = true
            };

            _issueBookButton = new Button
            {
                Text = "Issue Book",
                Location = new Point(900, 0),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(79, 70, 229),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F)
            };
            _issueBookButton.FlatAppearance.BorderSize = 0;
            _issueBookButton.Click += OnIssueBookClick;

            _refreshButton = new Button
            {
                Text = "Refresh",
                Location = new Point(1030, 0),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(229, 231, 235),
                ForeColor = Color.FromArgb(55, 65, 81),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F)
            };
            _refreshButton.FlatAppearance.BorderSize = 0;
            _refreshButton.Click += (s, e) => LoadBorrowings();

            headerPanel.Controls.Add(headerLabel);
            headerPanel.Controls.Add(_issueBookButton);
            headerPanel.Controls.Add(_refreshButton);

            _tabControl = new TabControl
            {
                Location = new Point(0, 50),
                Size = new Size(1160, 700),
                Font = new Font("Segoe UI", 9F)
            };

            var tabs = new[] { "Active", "Due Soon", "Overdue", "Returned", "Lost", "All Records" };
            foreach (var tabName in tabs)
            {
                var tabPage = new TabPage(tabName);
                var grid = CreateBorrowingsGrid();
                grid.Dock = DockStyle.Fill;
                tabPage.Controls.Add(grid);
                tabPage.Tag = grid;
                _tabControl.TabPages.Add(tabPage);
            }

            _tabControl.SelectedIndexChanged += (s, e) => LoadBorrowings();

            mainPanel.Controls.Add(headerPanel);
            mainPanel.Controls.Add(_tabControl);

            this.Controls.Add(mainPanel);
        }

        private DataGridView CreateBorrowingsGrid()
        {
            var grid = AppDataGridViewFactory.CreateStyledDataGridView();
            AppDataGridViewFactory.AddTextBoxColumn(grid, "BorrowingId", "ID", 120);
            AppDataGridViewFactory.AddTextBoxColumn(grid, "StudentName", "Student", 150);
            AppDataGridViewFactory.AddTextBoxColumn(grid, "StudentId", "Student ID", 120);
            AppDataGridViewFactory.AddTextBoxColumn(grid, "BookTitle", "Book", 200);
            AppDataGridViewFactory.AddTextBoxColumn(grid, "BookId", "Book ID", 120);
            AppDataGridViewFactory.AddDateColumn(grid, "BorrowedAtUtc", "Issue Date", "MM/dd/yyyy", 120);
            AppDataGridViewFactory.AddDateColumn(grid, "DueAtUtc", "Due Date", "MM/dd/yyyy", 120);
            AppDataGridViewFactory.AddTextBoxColumn(grid, "StatusDisplay", "Status", 100);
            AppDataGridViewFactory.AddButtonColumn(grid, "View", "View", 60);
            AppDataGridViewFactory.AddButtonColumn(grid, "Renew", "Renew", 60);
            AppDataGridViewFactory.AddButtonColumn(grid, "Return", "Return", 60);
            
            grid.CellClick += OnBorrowingsGridCellClick;
            return grid;
        }

        private void LoadBorrowings()
        {
            try
            {
                var currentTab = _tabControl.SelectedTab;
                if (currentTab?.Tag is DataGridView grid)
                {
                    // TODO: Load actual borrowings from service based on tab
                    grid.DataSource = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading borrowings: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void OnIssueBookClick(object sender, EventArgs e)
        {
            var dialog = new BookIssueDialog(_sessionId);
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                LoadBorrowings();
            }
        }

        private void OnBorrowingsGridCellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            var grid = sender as DataGridView;
            var borrowingId = grid?.Rows[e.RowIndex].Cells["BorrowingId"].Value?.ToString();
            if (string.IsNullOrEmpty(borrowingId)) return;

            if (grid.Columns[e.ColumnIndex].Name == "View")
            {
                var dialog = new BorrowingDetailsDialog(borrowingId);
                dialog.ShowDialog();
            }
            else if (grid.Columns[e.ColumnIndex].Name == "Renew")
            {
                var dialog = new BorrowingRenewDialog(borrowingId);
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    LoadBorrowings();
                }
            }
            else if (grid.Columns[e.ColumnIndex].Name == "Return")
            {
                var dialog = new BookReturnDialog(borrowingId);
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    LoadBorrowings();
                }
            }
        }
    }
}
