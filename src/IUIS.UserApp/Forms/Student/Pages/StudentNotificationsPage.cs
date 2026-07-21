using System;
using System.Drawing;
using System.Windows.Forms;
using IUIS.Application.Abstractions.StudentSelfService;
using IUIS.Application.StudentSelfService.Notifications;
using IUIS.SharedUI.DataGridViews;

namespace IUIS.UserApp.Forms.Student.Pages
{
    public partial class StudentNotificationsPage : UserControl
    {
        private readonly IStudentNotificationService _notificationService;
        private readonly string _sessionId;
        
        private FilterBarControl _filterBar;
        private DataGridView _notificationsGrid;

        public StudentNotificationsPage(
            IStudentNotificationService notificationService,
            string sessionId)
        {
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _sessionId = sessionId ?? throw new ArgumentNullException(nameof(sessionId));
            
            InitializeComponent();
            SetupLayout();
            LoadNotifications();
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
                Text = "Notifications",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                Location = new Point(0, 0),
                AutoSize = true
            };

            _filterBar = new FilterBarControl
            {
                Location = new Point(0, 40),
                Size = new Size(960, 40)
            };
            _filterBar.SetFilterOptions(new[] { "All", "Unread", "Read", "Archived" });

            _notificationsGrid = AppDataGridViewFactory.CreateStyledDataGridView();
            _notificationsGrid.Location = new Point(0, 90);
            _notificationsGrid.Size = new Size(960, 570);
            AppDataGridViewFactory.AddTextBoxColumn(_notificationsGrid, "Title", "Title", 250);
            AppDataGridViewFactory.AddTextBoxColumn(_notificationsGrid, "Message", "Message", 350);
            AppDataGridViewFactory.AddTextBoxColumn(_notificationsGrid, "Category", "Category", 120);
            AppDataGridViewFactory.AddDateColumn(_notificationsGrid, "CreatedAtUtc", "Date", 120);
            AppDataGridViewFactory.AddTextBoxColumn(_notificationsGrid, "IsRead", "Status", 80);
            AppDataGridViewFactory.AddButtonColumn(_notificationsGrid, "MarkRead", "Mark Read", 100);
            AppDataGridViewFactory.AddButtonColumn(_notificationsGrid, "Archive", "Archive", 80);

            _filterBar.SearchRequested += (s, e) => LoadNotifications();
            _filterBar.ClearRequested += (s, e) => LoadNotifications();
            _notificationsGrid.CellClick += OnNotificationsGridCellClick;

            mainPanel.Controls.Add(headerLabel);
            mainPanel.Controls.Add(_filterBar);
            mainPanel.Controls.Add(_notificationsGrid);

            this.Controls.Add(mainPanel);
        }

        private void LoadNotifications()
        {
            try
            {
                var notifications = _notificationService.GetNotifications(_sessionId);
                _notificationsGrid.DataSource = notifications;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading notifications: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void OnNotificationsGridCellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            var notificationId = _notificationsGrid.Rows[e.RowIndex].Cells["NotificationId"].Value?.ToString();
            if (string.IsNullOrEmpty(notificationId)) return;

            if (_notificationsGrid.Columns[e.ColumnIndex].Name == "MarkRead")
            {
                var result = _notificationService.MarkAsRead(_sessionId, notificationId);
                if (result.IsSuccess)
                {
                    LoadNotifications();
                }
            }
            else if (_notificationsGrid.Columns[e.ColumnIndex].Name == "Archive")
            {
                var result = _notificationService.ArchiveNotification(_sessionId, notificationId);
                if (result.IsSuccess)
                {
                    LoadNotifications();
                }
            }
        }
    }
}
