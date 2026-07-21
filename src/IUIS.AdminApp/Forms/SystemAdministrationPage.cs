using System;
using System.Drawing;
using System.Windows.Forms;
using IUIS.SharedUI.DataGridViews;

namespace IUIS.AdminApp.Forms
{
    public partial class SystemAdministrationPage : UserControl
    {
        private readonly string _sessionId;
        
        private TabControl _tabControl;
        private DataGridView _repositoriesGrid;
        private DataGridView _auditLogsGrid;
        private DataGridView _backupHistoryGrid;
        private Button _backupNowButton;
        private Button _restoreButton;

        public SystemAdministrationPage(string sessionId)
        {
            _sessionId = sessionId ?? throw new ArgumentNullException(nameof(sessionId));
            
            InitializeComponent();
            SetupLayout();
            LoadSystemData();
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

            var headerLabel = new Label
            {
                Text = "System Administration",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                Location = new Point(0, 0),
                AutoSize = true
            };

            _tabControl = new TabControl
            {
                Location = new Point(0, 40),
                Size = new Size(1360, 810),
                Font = new Font("Segoe UI", 9F)
            };

            var repositoriesTab = new TabPage("Repository Status");
            _repositoriesGrid = CreateRepositoriesGrid();
            _repositoriesGrid.Dock = DockStyle.Fill;
            repositoriesTab.Controls.Add(_repositoriesGrid);

            var auditLogsTab = new TabPage("Audit Logs");
            _auditLogsGrid = CreateAuditLogsGrid();
            _auditLogsGrid.Dock = DockStyle.Fill;
            auditLogsTab.Controls.Add(_auditLogsGrid);

            var backupTab = new TabPage("Backup & Restore");
            var backupPanel = new Panel { Dock = DockStyle.Fill };
            
            var buttonPanel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(1320, 50)
            };

            _backupNowButton = new Button
            {
                Text = "Backup Now",
                Location = new Point(0, 5),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(79, 70, 229),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F)
            };
            _backupNowButton.FlatAppearance.BorderSize = 0;
            _backupNowButton.Click += OnBackupNowClick;

            _restoreButton = new Button
            {
                Text = "Restore",
                Location = new Point(130, 5),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(245, 158, 11),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F)
            };
            _restoreButton.FlatAppearance.BorderSize = 0;
            _restoreButton.Click += OnRestoreClick;

            buttonPanel.Controls.Add(_backupNowButton);
            buttonPanel.Controls.Add(_restoreButton);

            _backupHistoryGrid = CreateBackupHistoryGrid();
            _backupHistoryGrid.Location = new Point(0, 60);
            _backupHistoryGrid.Size = new Size(1320, 700);

            backupPanel.Controls.Add(buttonPanel);
            backupPanel.Controls.Add(_backupHistoryGrid);
            backupTab.Controls.Add(backupPanel);

            _tabControl.TabPages.Add(repositoriesTab);
            _tabControl.TabPages.Add(auditLogsTab);
            _tabControl.TabPages.Add(backupTab);

            mainPanel.Controls.Add(headerLabel);
            mainPanel.Controls.Add(_tabControl);

            this.Controls.Add(mainPanel);
        }

        private DataGridView CreateRepositoriesGrid()
        {
            var grid = AppDataGridViewFactory.CreateStyledDataGridView();
            AppDataGridViewFactory.AddTextBoxColumn(grid, "RepositoryName", "Repository", 200);
            AppDataGridViewFactory.AddTextBoxColumn(grid, "FileName", "File Name", 200);
            AppDataGridViewFactory.AddTextBoxColumn(grid, "RecordCount", "Records", 100);
            AppDataGridViewFactory.AddTextBoxColumn(grid, "Revision", "Revision", 80);
            AppDataGridViewFactory.AddDateColumn(grid, "LastUpdatedUtc", "Last Updated", "MM/dd/yyyy", 150);
            AppDataGridViewFactory.AddTextBoxColumn(grid, "Status", "Status", 100);
            AppDataGridViewFactory.AddButtonColumn(grid, "View", "View", "View", 60);
            
            return grid;
        }

        private DataGridView CreateAuditLogsGrid()
        {
            var grid = AppDataGridViewFactory.CreateStyledDataGridView();
            AppDataGridViewFactory.AddDateColumn(grid, "TimestampUtc", "Timestamp", "MM/dd/yyyy", 150);
            AppDataGridViewFactory.AddTextBoxColumn(grid, "UserId", "User ID", 120);
            AppDataGridViewFactory.AddTextBoxColumn(grid, "UserName", "User", 150);
            AppDataGridViewFactory.AddTextBoxColumn(grid, "Action", "Action", 200);
            AppDataGridViewFactory.AddTextBoxColumn(grid, "Module", "Module", 120);
            AppDataGridViewFactory.AddTextBoxColumn(grid, "EntityId", "Entity ID", 150);
            AppDataGridViewFactory.AddTextBoxColumn(grid, "IpAddress", "IP Address", 120);
            
            return grid;
        }

        private DataGridView CreateBackupHistoryGrid()
        {
            var grid = AppDataGridViewFactory.CreateStyledDataGridView();
            AppDataGridViewFactory.AddTextBoxColumn(grid, "BackupId", "ID", 120);
            AppDataGridViewFactory.AddDateColumn(grid, "CreatedAtUtc", "Created", "MM/dd/yyyy", 150);
            AppDataGridViewFactory.AddTextBoxColumn(grid, "CreatedBy", "Created By", 150);
            AppDataGridViewFactory.AddTextBoxColumn(grid, "Type", "Type", 100);
            AppDataGridViewFactory.AddTextBoxColumn(grid, "Size", "Size", 100);
            AppDataGridViewFactory.AddTextBoxColumn(grid, "Status", "Status", 100);
            AppDataGridViewFactory.AddButtonColumn(grid, "Restore", "Restore", "Restore", 80);
            AppDataGridViewFactory.AddButtonColumn(grid, "Download", "Download", "Download", 80);
            
            grid.CellClick += OnBackupGridCellClick;
            return grid;
        }

        private void LoadSystemData()
        {
            try
            {
                // TODO: Load actual system data from service
                _repositoriesGrid.DataSource = null;
                _auditLogsGrid.DataSource = null;
                _backupHistoryGrid.DataSource = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading system data: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void OnBackupNowClick(object sender, EventArgs e)
        {
            MessageBox.Show("Backup initiated.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OnRestoreClick(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Restore operation will replace current data. Continue?",
                "Confirm Restore",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);
            
            if (result == DialogResult.Yes)
            {
                MessageBox.Show("Restore operation initiated.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void OnBackupGridCellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            var grid = sender as DataGridView;
            var backupId = grid?.Rows[e.RowIndex].Cells["BackupId"].Value?.ToString();
            if (string.IsNullOrEmpty(backupId)) return;

            if (grid.Columns[e.ColumnIndex].Name == "Restore")
            {
                var result = MessageBox.Show(
                    $"Restore backup {backupId}? This will replace current data.",
                    "Confirm Restore",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);
                
                if (result == DialogResult.Yes)
                {
                    MessageBox.Show("Restore operation initiated.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else if (grid.Columns[e.ColumnIndex].Name == "Download")
            {
                MessageBox.Show("Download functionality to be implemented.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
