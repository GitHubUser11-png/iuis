using System;
using System.Drawing;
using System.Windows.Forms;
using IUIS.SharedUI.Controls;
using IUIS.SharedUI.DataGridViews;

namespace IUIS.UserApp.Forms.Employee.Counseling
{
    public partial class CounselingSessionsPage : UserControl
    {
        private readonly string _sessionId;
        
        private TabControl _tabControl;
        private FilterBarControl _filterBar;
        private DataGridView _sessionsGrid;
        private Button _createSessionButton;
        private Button _refreshButton;

        public CounselingSessionsPage(string sessionId)
        {
            _sessionId = sessionId ?? throw new ArgumentNullException(nameof(sessionId));
            
            InitializeComponent();
            SetupLayout();
            LoadSessions();
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
                Text = "Counseling Sessions",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                Location = new Point(0, 5),
                AutoSize = true
            };

            _createSessionButton = new Button
            {
                Text = "Create Session",
                Location = new Point(900, 0),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(79, 70, 229),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F)
            };
            _createSessionButton.FlatAppearance.BorderSize = 0;
            _createSessionButton.Click += OnCreateSessionClick;

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
            _refreshButton.Click += (s, e) => LoadSessions();

            headerPanel.Controls.Add(headerLabel);
            headerPanel.Controls.Add(_createSessionButton);
            headerPanel.Controls.Add(_refreshButton);

            _tabControl = new TabControl
            {
                Location = new Point(0, 50),
                Size = new Size(1160, 700),
                Font = new Font("Segoe UI", 9F)
            };

            var tabs = new[] { "Scheduled", "In Progress", "Completed", "Cancelled", "All Sessions" };
            foreach (var tabName in tabs)
            {
                var tabPage = new TabPage(tabName);
                var grid = CreateSessionsGrid();
                grid.Dock = DockStyle.Fill;
                tabPage.Controls.Add(grid);
                tabPage.Tag = grid;
                _tabControl.TabPages.Add(tabPage);
            }

            _tabControl.SelectedIndexChanged += (s, e) => LoadSessions();

            mainPanel.Controls.Add(headerPanel);
            mainPanel.Controls.Add(_tabControl);

            this.Controls.Add(mainPanel);
        }

        private DataGridView CreateSessionsGrid()
        {
            var grid = AppDataGridViewFactory.CreateStyledDataGridView();
            AppDataGridViewFactory.AddTextBoxColumn(grid, "SessionId", "ID", 120);
            AppDataGridViewFactory.AddTextBoxColumn(grid, "StudentName", "Student", 150);
            AppDataGridViewFactory.AddTextBoxColumn(grid, "StudentId", "Student ID", 120);
            AppDataGridViewFactory.AddTextBoxColumn(grid, "SessionType", "Type", 120);
            AppDataGridViewFactory.AddDateColumn(grid, "ScheduledAtUtc", "Scheduled", 120);
            AppDataGridViewFactory.AddTextBoxColumn(grid, "StatusDisplay", "Status", 100);
            AppDataGridViewFactory.AddTextBoxColumn(grid, "Priority", "Priority", 80);
            AppDataGridViewFactory.AddButtonColumn(grid, "View", "View", 60);
            AppDataGridViewFactory.AddButtonColumn(grid, "Edit", "Edit", 60);
            
            grid.CellClick += OnSessionsGridCellClick;
            return grid;
        }

        private void LoadSessions()
        {
            try
            {
                var currentTab = _tabControl.SelectedTab;
                if (currentTab?.Tag is DataGridView grid)
                {
                    // TODO: Load actual sessions from service based on tab
                    grid.DataSource = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading sessions: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void OnCreateSessionClick(object sender, EventArgs e)
        {
            var dialog = new CounselingSessionDialog(_sessionId);
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                LoadSessions();
            }
        }

        private void OnSessionsGridCellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            var grid = sender as DataGridView;
            var sessionId = grid?.Rows[e.RowIndex].Cells["SessionId"].Value?.ToString();
            if (string.IsNullOrEmpty(sessionId)) return;

            if (grid.Columns[e.ColumnIndex].Name == "View")
            {
                var dialog = new CounselingSessionDetailsDialog(sessionId);
                dialog.ShowDialog();
            }
            else if (grid.Columns[e.ColumnIndex].Name == "Edit")
            {
                var dialog = new CounselingSessionDialog(_sessionId, sessionId);
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    LoadSessions();
                }
            }
        }
    }
}
