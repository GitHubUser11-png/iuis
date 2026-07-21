using System;
using System.Drawing;
using System.Windows.Forms;
using IUIS.SharedUI.Controls;
using IUIS.SharedUI.DataGridViews;

namespace IUIS.UserApp.Forms.Employee.Discipline
{
    public partial class DisciplineDashboardPage : UserControl
    {
        private readonly string _sessionId;
        
        private DashboardMetricCardControl _totalIncidentsCard;
        private DashboardMetricCardControl _pendingReviewCard;
        private DashboardMetricCardControl _activeCasesCard;
        private DashboardMetricCardControl _resolvedThisMonthCard;
        private DashboardMetricCardControl _highSeverityCard;
        private DashboardMetricCardControl _appealsPendingCard;
        private DataGridView _recentIncidentsGrid;
        private DataGridView _pendingReviewGrid;

        public DisciplineDashboardPage(string sessionId)
        {
            _sessionId = sessionId ?? throw new ArgumentNullException(nameof(sessionId));
            
            InitializeComponent();
            SetupLayout();
            LoadDashboard();
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
                BackColor = Color.FromArgb(249, 250, 251),
                AutoScroll = true
            };

            var headerLabel = new Label
            {
                Text = "Discipline Dashboard",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                Location = new Point(0, 0),
                AutoSize = true
            };

            var metricsPanel = new FlowLayoutPanel
            {
                Location = new Point(0, 40),
                Size = new Size(1160, 120),
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.FromArgb(249, 250, 251),
                WrapContents = true
            };

            _totalIncidentsCard = new DashboardMetricCardControl
            {
                Title = "Total Incidents",
                Value = "0",
                Subtitle = "All Time"
            };

            _pendingReviewCard = new DashboardMetricCardControl
            {
                Title = "Pending Review",
                Value = "0",
                Subtitle = "Awaiting Action"
            };
            _pendingReviewCard.SetWarningState();

            _activeCasesCard = new DashboardMetricCardControl
            {
                Title = "Active Cases",
                Value = "0",
                Subtitle = "Ongoing"
            };

            _resolvedThisMonthCard = new DashboardMetricCardControl
            {
                Title = "Resolved This Month",
                Value = "0",
                Subtitle = "Cases"
            };
            _resolvedThisMonthCard.SetSuccessState();

            _highSeverityCard = new DashboardMetricCardControl
            {
                Title = "High Severity",
                Value = "0",
                Subtitle = "Critical"
            };
            _highSeverityCard.SetErrorState();

            _appealsPendingCard = new DashboardMetricCardControl
            {
                Title = "Appeals Pending",
                Value = "0",
                Subtitle = "Under Review"
            };
            _appealsPendingCard.SetWarningState();

            metricsPanel.Controls.Add(_totalIncidentsCard);
            metricsPanel.Controls.Add(_pendingReviewCard);
            metricsPanel.Controls.Add(_activeCasesCard);
            metricsPanel.Controls.Add(_resolvedThisMonthCard);
            metricsPanel.Controls.Add(_highSeverityCard);
            metricsPanel.Controls.Add(_appealsPendingCard);

            var recentIncidentsLabel = new Label
            {
                Text = "Recent Incidents",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                Location = new Point(0, 180),
                AutoSize = true
            };

            _recentIncidentsGrid = AppDataGridViewFactory.CreateStyledDataGridView();
            _recentIncidentsGrid.Location = new Point(0, 210);
            _recentIncidentsGrid.Size = new Size(570, 250);
            AppDataGridViewFactory.AddTextBoxColumn(_recentIncidentsGrid, "IncidentId", "ID", 120);
            AppDataGridViewFactory.AddTextBoxColumn(_recentIncidentsGrid, "StudentName", "Student", 150);
            AppDataGridViewFactory.AddTextBoxColumn(_recentIncidentsGrid, "ViolationType", "Violation", 150);
            AppDataGridViewFactory.AddDateColumn(_recentIncidentsGrid, "ReportedAtUtc", "Date", "MM/dd/yyyy", 120);

            var pendingReviewLabel = new Label
            {
                Text = "Pending Review",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                Location = new Point(590, 180),
                AutoSize = true
            };

            _pendingReviewGrid = AppDataGridViewFactory.CreateStyledDataGridView();
            _pendingReviewGrid.Location = new Point(590, 210);
            _pendingReviewGrid.Size = new Size(570, 250);
            AppDataGridViewFactory.AddTextBoxColumn(_pendingReviewGrid, "IncidentId", "ID", 120);
            AppDataGridViewFactory.AddTextBoxColumn(_pendingReviewGrid, "StudentName", "Student", 150);
            AppDataGridViewFactory.AddTextBoxColumn(_pendingReviewGrid, "Severity", "Severity", 100);
            AppDataGridViewFactory.AddDateColumn(_pendingReviewGrid, "ReportedAtUtc", "Date", "MM/dd/yyyy", 120);

            mainPanel.Controls.Add(headerLabel);
            mainPanel.Controls.Add(metricsPanel);
            mainPanel.Controls.Add(recentIncidentsLabel);
            mainPanel.Controls.Add(_recentIncidentsGrid);
            mainPanel.Controls.Add(pendingReviewLabel);
            mainPanel.Controls.Add(_pendingReviewGrid);

            this.Controls.Add(mainPanel);
        }

        private void LoadDashboard()
        {
            try
            {
                // TODO: Load actual dashboard data from service
                _totalIncidentsCard.Value = "345";
                _pendingReviewCard.Value = "12";
                _activeCasesCard.Value = "28";
                _resolvedThisMonthCard.Value = "45";
                _highSeverityCard.Value = "5";
                _appealsPendingCard.Value = "3";
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
