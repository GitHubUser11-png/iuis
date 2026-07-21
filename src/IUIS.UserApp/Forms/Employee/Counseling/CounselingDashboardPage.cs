using System;
using System.Drawing;
using System.Windows.Forms;
using IUIS.SharedUI.Controls;
using IUIS.SharedUI.DataGridViews;

namespace IUIS.UserApp.Forms.Employee.Counseling
{
    public partial class CounselingDashboardPage : UserControl
    {
        private readonly string _sessionId;
        
        private DashboardMetricCardControl _totalSessionsCard;
        private DashboardMetricCardControl _scheduledTodayCard;
        private DashboardMetricCardControl _pendingRequestsCard;
        private DashboardMetricCardControl _activeCasesCard;
        private DashboardMetricCardControl _completedThisMonthCard;
        private DashboardMetricCardControl _urgentReferralsCard;
        private DataGridView _todayScheduleGrid;
        private DataGridView _pendingRequestsGrid;

        public CounselingDashboardPage(string sessionId)
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
                Text = "Counseling Dashboard",
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

            _totalSessionsCard = new DashboardMetricCardControl
            {
                Title = "Total Sessions",
                Value = "0",
                Subtitle = "All Time"
            };

            _scheduledTodayCard = new DashboardMetricCardControl
            {
                Title = "Scheduled Today",
                Value = "0",
                Subtitle = "Appointments"
            };
            _scheduledTodayCard.SetSuccessState();

            _pendingRequestsCard = new DashboardMetricCardControl
            {
                Title = "Pending Requests",
                Value = "0",
                Subtitle = "Awaiting Action"
            };
            _pendingRequestsCard.SetWarningState();

            _activeCasesCard = new DashboardMetricCardControl
            {
                Title = "Active Cases",
                Value = "0",
                Subtitle = "Ongoing Support"
            };

            _completedThisMonthCard = new DashboardMetricCardControl
            {
                Title = "Completed This Month",
                Value = "0",
                Subtitle = "Sessions"
            };
            _completedThisMonthCard.SetSuccessState();

            _urgentReferralsCard = new DashboardMetricCardControl
            {
                Title = "Urgent Referrals",
                Value = "0",
                Subtitle = "Needs Attention"
            };
            _urgentReferralsCard.SetErrorState();

            metricsPanel.Controls.Add(_totalSessionsCard);
            metricsPanel.Controls.Add(_scheduledTodayCard);
            metricsPanel.Controls.Add(_pendingRequestsCard);
            metricsPanel.Controls.Add(_activeCasesCard);
            metricsPanel.Controls.Add(_completedThisMonthCard);
            metricsPanel.Controls.Add(_urgentReferralsCard);

            var todayScheduleLabel = new Label
            {
                Text = "Today's Schedule",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                Location = new Point(0, 180),
                AutoSize = true
            };

            _todayScheduleGrid = AppDataGridViewFactory.CreateStyledDataGridView();
            _todayScheduleGrid.Location = new Point(0, 210);
            _todayScheduleGrid.Size = new Size(570, 250);
            AppDataGridViewFactory.AddTextBoxColumn(_todayScheduleGrid, "SessionId", "ID", 120);
            AppDataGridViewFactory.AddTextBoxColumn(_todayScheduleGrid, "StudentName", "Student", 150);
            AppDataGridViewFactory.AddTextBoxColumn(_todayScheduleGrid, "SessionType", "Type", 120);
            AppDataGridViewFactory.AddDateColumn(_todayScheduleGrid, "ScheduledAtUtc", "Time", "MM/dd/yyyy", 120);

            var pendingRequestsLabel = new Label
            {
                Text = "Pending Session Requests",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                Location = new Point(590, 180),
                AutoSize = true
            };

            _pendingRequestsGrid = AppDataGridViewFactory.CreateStyledDataGridView();
            _pendingRequestsGrid.Location = new Point(590, 210);
            _pendingRequestsGrid.Size = new Size(570, 250);
            AppDataGridViewFactory.AddTextBoxColumn(_pendingRequestsGrid, "RequestId", "ID", 120);
            AppDataGridViewFactory.AddTextBoxColumn(_pendingRequestsGrid, "StudentName", "Student", 150);
            AppDataGridViewFactory.AddDateColumn(_pendingRequestsGrid, "RequestedAtUtc", "Date", 120);
            AppDataGridViewFactory.AddTextBoxColumn(_pendingRequestsGrid, "Priority", "Priority", 100);

            mainPanel.Controls.Add(headerLabel);
            mainPanel.Controls.Add(metricsPanel);
            mainPanel.Controls.Add(todayScheduleLabel);
            mainPanel.Controls.Add(_todayScheduleGrid);
            mainPanel.Controls.Add(pendingRequestsLabel);
            mainPanel.Controls.Add(_pendingRequestsGrid);

            this.Controls.Add(mainPanel);
        }

        private void LoadDashboard()
        {
            try
            {
                // TODO: Load actual dashboard data from service
                _totalSessionsCard.Value = "1,245";
                _scheduledTodayCard.Value = "8";
                _pendingRequestsCard.Value = "15";
                _activeCasesCard.Value = "42";
                _completedThisMonthCard.Value = "67";
                _urgentReferralsCard.Value = "3";
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
