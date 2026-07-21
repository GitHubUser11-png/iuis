using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using IUIS.Application.Abstractions.StudentSelfService;
using IUIS.Application.StudentSelfService.Dashboard;
using IUIS.SharedUI.Controls;
using IUIS.SharedUI.DataGridViews;

namespace IUIS.UserApp.Forms.Student.Pages
{
    public partial class StudentDashboardPage : UserControl
    {
        private readonly IStudentDashboardService _dashboardService;
        private readonly string _sessionId;
        
        private StudentStatusCardControl _statusCard;
        private DashboardMetricCardControl _balanceCard;
        private DashboardMetricCardControl _borrowingsCard;
        private DashboardMetricCardControl _scholarshipsCard;
        private DashboardMetricCardControl _notificationsCard;
        private DataGridView _appointmentsGrid;
        private DataGridView _notificationsGrid;

        public StudentDashboardPage(
            IStudentDashboardService dashboardService,
            string sessionId)
        {
            _dashboardService = dashboardService ?? throw new ArgumentNullException(nameof(dashboardService));
            _sessionId = sessionId ?? throw new ArgumentNullException(nameof(sessionId));
            
            InitializeComponent();
            SetupLayout();
            LoadDashboard();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(1000, 700);
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
                Text = "Student Dashboard",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                Location = new Point(0, 0),
                AutoSize = true
            };

            _statusCard = new StudentStatusCardControl
            {
                Location = new Point(0, 40),
                Size = new Size(350, 120)
            };

            var metricsPanel = new FlowLayoutPanel
            {
                Location = new Point(0, 180),
                Size = new Size(960, 120),
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.FromArgb(249, 250, 251)
            };

            _balanceCard = new DashboardMetricCardControl
            {
                Title = "Outstanding Balance",
                Value = "₱0.00",
                Subtitle = "Current Term"
            };

            _borrowingsCard = new DashboardMetricCardControl
            {
                Title = "Library Borrowings",
                Value = "0",
                Subtitle = "Active Books"
            };

            _scholarshipsCard = new DashboardMetricCardControl
            {
                Title = "Scholarships",
                Value = "0",
                Subtitle = "Active Awards"
            };

            _notificationsCard = new DashboardMetricCardControl
            {
                Title = "Notifications",
                Value = "0",
                Subtitle = "Unread"
            };

            metricsPanel.Controls.Add(_balanceCard);
            metricsPanel.Controls.Add(_borrowingsCard);
            metricsPanel.Controls.Add(_scholarshipsCard);
            metricsPanel.Controls.Add(_notificationsCard);

            var appointmentsLabel = new Label
            {
                Text = "Upcoming Appointments",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                Location = new Point(0, 320),
                AutoSize = true
            };

            _appointmentsGrid = AppDataGridViewFactory.CreateStyledDataGridView();
            _appointmentsGrid.Location = new Point(0, 350);
            _appointmentsGrid.Size = new Size(470, 300);
            AppDataGridViewFactory.AddTextBoxColumn(_appointmentsGrid, "AppointmentType", "Type", 150);
            AppDataGridViewFactory.AddDateColumn(_appointmentsGrid, "ScheduledDateUtc", "Date", "MM/dd/yyyy", 120);
            AppDataGridViewFactory.AddTextBoxColumn(_appointmentsGrid, "Location", "Location", 200);

            var notificationsLabel = new Label
            {
                Text = "Recent Notifications",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                Location = new Point(490, 320),
                AutoSize = true
            };

            _notificationsGrid = AppDataGridViewFactory.CreateStyledDataGridView();
            _notificationsGrid.Location = new Point(490, 350);
            _notificationsGrid.Size = new Size(470, 300);
            AppDataGridViewFactory.AddTextBoxColumn(_notificationsGrid, "Title", "Title", 200);
            AppDataGridViewFactory.AddDateColumn(_notificationsGrid, "CreatedAtUtc", "Date", "MM/dd/yyyy", 120);
            AppDataGridViewFactory.AddTextBoxColumn(_notificationsGrid, "IsRead", "Status", 150);

            mainPanel.Controls.Add(headerLabel);
            mainPanel.Controls.Add(_statusCard);
            mainPanel.Controls.Add(metricsPanel);
            mainPanel.Controls.Add(appointmentsLabel);
            mainPanel.Controls.Add(_appointmentsGrid);
            mainPanel.Controls.Add(notificationsLabel);
            mainPanel.Controls.Add(_notificationsGrid);

            this.Controls.Add(mainPanel);
        }

        private async void LoadDashboard()
        {
            try
            {
                var dashboard = _dashboardService.GetDashboard(_sessionId);
                
                _statusCard.StudentName = dashboard.StudentName;
                _statusCard.Program = dashboard.ProgramName;
                _statusCard.YearLevel = $"Year Level: {dashboard.YearLevel}";
                _statusCard.SetActive();

                _balanceCard.Value = $"₱{dashboard.OutstandingBalance:N2}";
                _balanceCard.Subtitle = $"Paid: ₱{dashboard.TotalPaid:N2}";

                if (dashboard.OutstandingBalance > 0)
                    _balanceCard.SetWarningState();
                else
                    _balanceCard.SetSuccessState();

                _borrowingsCard.Value = dashboard.ActiveBorrowings.ToString();
                if (dashboard.OverdueBooks > 0)
                {
                    _borrowingsCard.SetErrorState();
                    _borrowingsCard.Subtitle = $"{dashboard.OverdueBooks} Overdue";
                }
                else
                {
                    _borrowingsCard.SetNormalState();
                }

                _scholarshipsCard.Value = dashboard.ActiveScholarships.ToString();
                _scholarshipsCard.Subtitle = $"{dashboard.PendingApplications} Pending";

                _notificationsCard.Value = dashboard.RecentNotifications.Count.ToString();
                _notificationsCard.Subtitle = $"{dashboard.RecentNotifications.Where(n => !n.IsRead).Count()} Unread";

                _appointmentsGrid.DataSource = dashboard.UpcomingAppointments;
                _notificationsGrid.DataSource = dashboard.RecentNotifications;
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
