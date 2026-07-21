using System;
using System.Drawing;
using System.Windows.Forms;
using IUIS.SharedUI.Controls;
using IUIS.SharedUI.DataGridViews;

namespace IUIS.AdminApp.Forms
{
    public partial class AdminDashboardPage : UserControl
    {
        private readonly string _sessionId;
        
        private DashboardMetricCardControl _totalUsersCard;
        private DashboardMetricCardControl _activeStudentsCard;
        private DashboardMetricCardControl _activeEmployeesCard;
        private DashboardMetricCardControl _pendingApplicationsCard;
        private DashboardMetricCardControl _systemHealthCard;
        private DashboardMetricCardControl _repositoryHealthCard;
        private DataGridView _recentActivityGrid;
        private DataGridView _alertsGrid;

        public AdminDashboardPage(string sessionId)
        {
            _sessionId = sessionId ?? throw new ArgumentNullException(nameof(sessionId));
            
            InitializeComponent();
            SetupLayout();
            LoadDashboard();
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
                BackColor = Color.FromArgb(249, 250, 251),
                AutoScroll = true
            };

            var headerLabel = new Label
            {
                Text = "Administrator Dashboard",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                Location = new Point(0, 0),
                AutoSize = true
            };

            var metricsPanel = new FlowLayoutPanel
            {
                Location = new Point(0, 40),
                Size = new Size(1360, 120),
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.FromArgb(249, 250, 251),
                WrapContents = true
            };

            _totalUsersCard = new DashboardMetricCardControl
            {
                Title = "Total Users",
                Value = "0",
                Subtitle = "All Accounts"
            };

            _activeStudentsCard = new DashboardMetricCardControl
            {
                Title = "Active Students",
                Value = "0",
                Subtitle = "Enrolled"
            };
            _activeStudentsCard.SetSuccessState();

            _activeEmployeesCard = new DashboardMetricCardControl
            {
                Title = "Active Employees",
                Value = "0",
                Subtitle = "Faculty & Staff"
            };

            _pendingApplicationsCard = new DashboardMetricCardControl
            {
                Title = "Pending Applications",
                Value = "0",
                Subtitle = "Awaiting Review"
            };
            _pendingApplicationsCard.SetWarningState();

            _systemHealthCard = new DashboardMetricCardControl
            {
                Title = "System Health",
                Value = "Healthy",
                Subtitle = "All Services"
            };
            _systemHealthCard.SetSuccessState();

            _repositoryHealthCard = new DashboardMetricCardControl
            {
                Title = "Repository Health",
                Value = "OK",
                Subtitle = "JSON Files"
            };
            _repositoryHealthCard.SetSuccessState();

            metricsPanel.Controls.Add(_totalUsersCard);
            metricsPanel.Controls.Add(_activeStudentsCard);
            metricsPanel.Controls.Add(_activeEmployeesCard);
            metricsPanel.Controls.Add(_pendingApplicationsCard);
            metricsPanel.Controls.Add(_systemHealthCard);
            metricsPanel.Controls.Add(_repositoryHealthCard);

            var recentActivityLabel = new Label
            {
                Text = "Recent System Activity",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                Location = new Point(0, 180),
                AutoSize = true
            };

            _recentActivityGrid = AppDataGridViewFactory.CreateStyledDataGridView();
            _recentActivityGrid.Location = new Point(0, 210);
            _recentActivityGrid.Size = new Size(670, 250);
            AppDataGridViewFactory.AddTextBoxColumn(_recentActivityGrid, "Timestamp", "Timestamp", 150);
            AppDataGridViewFactory.AddTextBoxColumn(_recentActivityGrid, "User", "User", 150);
            AppDataGridViewFactory.AddTextBoxColumn(_recentActivityGrid, "Action", "Action", 200);
            AppDataGridViewFactory.AddTextBoxColumn(_recentActivityGrid, "Module", "Module", 100);

            var alertsLabel = new Label
            {
                Text = "System Alerts",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                Location = new Point(690, 180),
                AutoSize = true
            };

            _alertsGrid = AppDataGridViewFactory.CreateStyledDataGridView();
            _alertsGrid.Location = new Point(690, 210);
            _alertsGrid.Size = new Size(670, 250);
            AppDataGridViewFactory.AddTextBoxColumn(_alertsGrid, "Severity", "Severity", 100);
            AppDataGridViewFactory.AddTextBoxColumn(_alertsGrid, "AlertType", "Type", 150);
            AppDataGridViewFactory.AddTextBoxColumn(_alertsGrid, "Message", "Message", 300);
            AppDataGridViewFactory.AddDateColumn(_alertsGrid, "CreatedAtUtc", "Date", "MM/dd/yyyy", 120);

            mainPanel.Controls.Add(headerLabel);
            mainPanel.Controls.Add(metricsPanel);
            mainPanel.Controls.Add(recentActivityLabel);
            mainPanel.Controls.Add(_recentActivityGrid);
            mainPanel.Controls.Add(alertsLabel);
            mainPanel.Controls.Add(_alertsGrid);

            this.Controls.Add(mainPanel);
        }

        private void LoadDashboard()
        {
            try
            {
                // TODO: Load actual dashboard data from service
                _totalUsersCard.Value = "3,456";
                _activeStudentsCard.Value = "2,890";
                _activeEmployeesCard.Value = "566";
                _pendingApplicationsCard.Value = "45";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading dashboard: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
