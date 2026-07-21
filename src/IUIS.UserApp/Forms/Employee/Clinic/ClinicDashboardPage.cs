using System;
using System.Drawing;
using System.Windows.Forms;
using IUIS.SharedUI.Controls;
using IUIS.SharedUI.DataGridViews;

namespace IUIS.UserApp.Forms.Employee.Clinic
{
    public partial class ClinicDashboardPage : UserControl
    {
        private readonly string _sessionId;
        
        private DashboardMetricCardControl _totalConsultationsCard;
        private DashboardMetricCardControl _scheduledTodayCard;
        private DashboardMetricCardControl _pendingClearancesCard;
        private DashboardMetricCardControl _activeMedicalCasesCard;
        private DashboardMetricCardControl _completedThisMonthCard;
        private DashboardMetricCardControl _emergencyCasesCard;
        private DataGridView _todayScheduleGrid;
        private DataGridView _pendingClearancesGrid;

        public ClinicDashboardPage(string sessionId)
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
                Text = "Clinic Dashboard",
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

            _totalConsultationsCard = new DashboardMetricCardControl
            {
                Title = "Total Consultations",
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

            _pendingClearancesCard = new DashboardMetricCardControl
            {
                Title = "Pending Clearances",
                Value = "0",
                Subtitle = "Awaiting Review"
            };
            _pendingClearancesCard.SetWarningState();

            _activeMedicalCasesCard = new DashboardMetricCardControl
            {
                Title = "Active Medical Cases",
                Value = "0",
                Subtitle = "Ongoing Treatment"
            };

            _completedThisMonthCard = new DashboardMetricCardControl
            {
                Title = "Completed This Month",
                Value = "0",
                Subtitle = "Consultations"
            };
            _completedThisMonthCard.SetSuccessState();

            _emergencyCasesCard = new DashboardMetricCardControl
            {
                Title = "Emergency Cases",
                Value = "0",
                Subtitle = "This Week"
            };
            _emergencyCasesCard.SetErrorState();

            metricsPanel.Controls.Add(_totalConsultationsCard);
            metricsPanel.Controls.Add(_scheduledTodayCard);
            metricsPanel.Controls.Add(_pendingClearancesCard);
            metricsPanel.Controls.Add(_activeMedicalCasesCard);
            metricsPanel.Controls.Add(_completedThisMonthCard);
            metricsPanel.Controls.Add(_emergencyCasesCard);

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
            AppDataGridViewFactory.AddTextBoxColumn(_todayScheduleGrid, "ConsultationId", "ID", 120);
            AppDataGridViewFactory.AddTextBoxColumn(_todayScheduleGrid, "PatientName", "Patient", 150);
            AppDataGridViewFactory.AddTextBoxColumn(_todayScheduleGrid, "ConsultationType", "Type", 120);
            AppDataGridViewFactory.AddDateColumn(_todayScheduleGrid, "ScheduledAtUtc", "Time", "MM/dd/yyyy", 120);

            var pendingClearancesLabel = new Label
            {
                Text = "Pending Medical Clearances",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                Location = new Point(590, 180),
                AutoSize = true
            };

            _pendingClearancesGrid = AppDataGridViewFactory.CreateStyledDataGridView();
            _pendingClearancesGrid.Location = new Point(590, 210);
            _pendingClearancesGrid.Size = new Size(570, 250);
            AppDataGridViewFactory.AddTextBoxColumn(_pendingClearancesGrid, "ClearanceId", "ID", 120);
            AppDataGridViewFactory.AddTextBoxColumn(_pendingClearancesGrid, "StudentName", "Student", 150);
            AppDataGridViewFactory.AddTextBoxColumn(_pendingClearancesGrid, "ClearanceType", "Type", 120);
            AppDataGridViewFactory.AddDateColumn(_pendingClearancesGrid, "RequestedAtUtc", "Date", "MM/dd/yyyy", 120);

            mainPanel.Controls.Add(headerLabel);
            mainPanel.Controls.Add(metricsPanel);
            mainPanel.Controls.Add(todayScheduleLabel);
            mainPanel.Controls.Add(_todayScheduleGrid);
            mainPanel.Controls.Add(pendingClearancesLabel);
            mainPanel.Controls.Add(_pendingClearancesGrid);

            this.Controls.Add(mainPanel);
        }

        private void LoadDashboard()
        {
            try
            {
                // TODO: Load actual dashboard data from service
                _totalConsultationsCard.Value = "2,156";
                _scheduledTodayCard.Value = "12";
                _pendingClearancesCard.Value = "28";
                _activeMedicalCasesCard.Value = "45";
                _completedThisMonthCard.Value = "89";
                _emergencyCasesCard.Value = "4";
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
