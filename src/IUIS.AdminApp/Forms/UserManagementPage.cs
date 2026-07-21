using System;
using System.Drawing;
using System.Windows.Forms;
using IUIS.SharedUI.Controls;
using IUIS.SharedUI.DataGridViews;

namespace IUIS.AdminApp.Forms
{
    public partial class UserManagementPage : UserControl
    {
        private readonly string _sessionId;
        
        private TabControl _tabControl;
        private Button _createUserButton;
        private Button _refreshButton;

        public UserManagementPage(string sessionId)
        {
            _sessionId = sessionId ?? throw new ArgumentNullException(nameof(sessionId));
            
            InitializeComponent();
            SetupLayout();
            LoadUsers();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(1400, 900);
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
                Size = new Size(1360, 40)
            };

            var headerLabel = new Label
            {
                Text = "User Management",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                Location = new Point(0, 5),
                AutoSize = true
            };

            _createUserButton = new Button
            {
                Text = "Create User",
                Location = new Point(1100, 0),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(79, 70, 229),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F)
            };
            _createUserButton.FlatAppearance.BorderSize = 0;
            _createUserButton.Click += OnCreateUserClick;

            _refreshButton = new Button
            {
                Text = "Refresh",
                Location = new Point(1230, 0),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(229, 231, 235),
                ForeColor = Color.FromArgb(55, 65, 81),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F)
            };
            _refreshButton.FlatAppearance.BorderSize = 0;
            _refreshButton.Click += (s, e) => LoadUsers();

            headerPanel.Controls.Add(headerLabel);
            headerPanel.Controls.Add(_createUserButton);
            headerPanel.Controls.Add(_refreshButton);

            _tabControl = new TabControl
            {
                Location = new Point(0, 50),
                Size = new Size(1360, 800),
                Font = new Font("Segoe UI", 9F)
            };

            var tabs = new[] { "All Users", "Students", "Employees", "Administrators", "Pending Approval" };
            foreach (var tabName in tabs)
            {
                var tabPage = new TabPage(tabName);
                var grid = CreateUsersGrid();
                grid.Dock = DockStyle.Fill;
                tabPage.Controls.Add(grid);
                tabPage.Tag = grid;
                _tabControl.TabPages.Add(tabPage);
            }

            _tabControl.SelectedIndexChanged += (s, e) => LoadUsers();

            mainPanel.Controls.Add(headerPanel);
            mainPanel.Controls.Add(_tabControl);

            this.Controls.Add(mainPanel);
        }

        private DataGridView CreateUsersGrid()
        {
            var grid = AppDataGridViewFactory.CreateStyledDataGridView();
            AppDataGridViewFactory.AddTextBoxColumn(grid, "UserId", "ID", 120);
            AppDataGridViewFactory.AddTextBoxColumn(grid, "LoginId", "Login ID", 120);
            AppDataGridViewFactory.AddTextBoxColumn(grid, "PersonName", "Name", 200);
            AppDataGridViewFactory.AddTextBoxColumn(grid, "PrimaryRole", "Role", 120);
            AppDataGridViewFactory.AddTextBoxColumn(grid, "Status", "Status", 100);
            AppDataGridViewFactory.AddDateColumn(grid, "CreatedAtUtc", "Created", "MM/dd/yyyy", 120);
            AppDataGridViewFactory.AddDateColumn(grid, "LastLoginAtUtc", "Last Login", "MM/dd/yyyy", 120);
            AppDataGridViewFactory.AddButtonColumn(grid, "View", "View", "View", 60);
            AppDataGridViewFactory.AddButtonColumn(grid, "Edit", "Edit", "Edit", 60);
            AppDataGridViewFactory.AddButtonColumn(grid, "Permissions", "Permissions", "Permissions", 100);
            
            grid.CellClick += OnUsersGridCellClick;
            return grid;
        }

        private void LoadUsers()
        {
            try
            {
                var currentTab = _tabControl.SelectedTab;
                if (currentTab?.Tag is DataGridView grid)
                {
                    // TODO: Load actual users from service based on tab
                    grid.DataSource = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading users: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void OnCreateUserClick(object sender, EventArgs e)
        {
            // TODO: Implement UserCreateEditDialog
            MessageBox.Show("User creation dialog not yet implemented.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OnUsersGridCellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            var grid = sender as DataGridView;
            var userId = grid?.Rows[e.RowIndex].Cells["UserId"].Value?.ToString();
            if (string.IsNullOrEmpty(userId)) return;

            if (grid.Columns[e.ColumnIndex].Name == "View")
            {
                // TODO: Implement UserDetailsDialog
                MessageBox.Show("User details dialog not yet implemented.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (grid.Columns[e.ColumnIndex].Name == "Edit")
            {
                // TODO: Implement UserCreateEditDialog
                MessageBox.Show("User edit dialog not yet implemented.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (grid.Columns[e.ColumnIndex].Name == "Permissions")
            {
                // TODO: Implement UserPermissionsDialog
                MessageBox.Show("User permissions dialog not yet implemented.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
