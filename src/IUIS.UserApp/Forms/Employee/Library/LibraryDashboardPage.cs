using System;
using System.Drawing;
using System.Windows.Forms;
using IUIS.Application.Abstractions.EmployeeServices;
using IUIS.SharedUI.Controls;
using IUIS.SharedUI.DataGridViews;

namespace IUIS.UserApp.Forms.Employee.Library
{
    public partial class LibraryDashboardPage : UserControl
    {
        private readonly string _sessionId;
        
        private DashboardMetricCardControl _totalBooksCard;
        private DashboardMetricCardControl _totalCopiesCard;
        private DashboardMetricCardControl _availableCopiesCard;
        private DashboardMetricCardControl _activeBorrowingsCard;
        private DashboardMetricCardControl _dueSoonCard;
        private DashboardMetricCardControl _overdueCard;
        private DashboardMetricCardControl _lostBooksCard;
        private DashboardMetricCardControl _inventoryWarningsCard;
        private DataGridView _dueTodayGrid;
        private DataGridView _overdueGrid;

        public LibraryDashboardPage(string sessionId)
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
                Text = "Library Dashboard",
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

            _totalBooksCard = new DashboardMetricCardControl
            {
                Title = "Total Book Titles",
                Value = "0",
                Subtitle = "In Catalog"
            };

            _totalCopiesCard = new DashboardMetricCardControl
            {
                Title = "Total Copies",
                Value = "0",
                Subtitle = "All Titles"
            };

            _availableCopiesCard = new DashboardMetricCardControl
            {
                Title = "Available Copies",
                Value = "0",
                Subtitle = "Ready for Borrowing"
            };
            _availableCopiesCard.SetSuccessState();

            _activeBorrowingsCard = new DashboardMetricCardControl
            {
                Title = "Active Borrowings",
                Value = "0",
                Subtitle = "Currently Out"
            };

            _dueSoonCard = new DashboardMetricCardControl
            {
                Title = "Due Soon",
                Value = "0",
                Subtitle = "Next 3 Days"
            };
            _dueSoonCard.SetWarningState();

            _overdueCard = new DashboardMetricCardControl
            {
                Title = "Overdue",
                Value = "0",
                Subtitle = "Past Due Date"
            };
            _overdueCard.SetErrorState();

            _lostBooksCard = new DashboardMetricCardControl
            {
                Title = "Lost Books",
                Value = "0",
                Subtitle = "Unreturned"
            };
            _lostBooksCard.SetErrorState();

            _inventoryWarningsCard = new DashboardMetricCardControl
            {
                Title = "Inventory Warnings",
                Value = "0",
                Subtitle = "Needs Attention"
            };
            _inventoryWarningsCard.SetWarningState();

            metricsPanel.Controls.Add(_totalBooksCard);
            metricsPanel.Controls.Add(_totalCopiesCard);
            metricsPanel.Controls.Add(_availableCopiesCard);
            metricsPanel.Controls.Add(_activeBorrowingsCard);
            metricsPanel.Controls.Add(_dueSoonCard);
            metricsPanel.Controls.Add(_overdueCard);
            metricsPanel.Controls.Add(_lostBooksCard);
            metricsPanel.Controls.Add(_inventoryWarningsCard);

            var dueTodayLabel = new Label
            {
                Text = "Due Today",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                Location = new Point(0, 180),
                AutoSize = true
            };

            _dueTodayGrid = AppDataGridViewFactory.CreateStyledDataGridView();
            _dueTodayGrid.Location = new Point(0, 210);
            _dueTodayGrid.Size = new Size(570, 250);
            AppDataGridViewFactory.AddTextBoxColumn(_dueTodayGrid, "BorrowingId", "ID", 120);
            AppDataGridViewFactory.AddTextBoxColumn(_dueTodayGrid, "StudentName", "Student", 150);
            AppDataGridViewFactory.AddTextBoxColumn(_dueTodayGrid, "BookTitle", "Book", 150);
            AppDataGridViewFactory.AddDateColumn(_dueTodayGrid, "DueAtUtc", "Due Date", 120);

            var overdueLabel = new Label
            {
                Text = "Overdue Borrowings",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                Location = new Point(590, 180),
                AutoSize = true
            };

            _overdueGrid = AppDataGridViewFactory.CreateStyledDataGridView();
            _overdueGrid.Location = new Point(590, 210);
            _overdueGrid.Size = new Size(570, 250);
            AppDataGridViewFactory.AddTextBoxColumn(_overdueGrid, "BorrowingId", "ID", 120);
            AppDataGridViewFactory.AddTextBoxColumn(_overdueGrid, "StudentName", "Student", 150);
            AppDataGridViewFactory.AddTextBoxColumn(_overdueGrid, "BookTitle", "Book", 150);
            AppDataGridViewFactory.AddDateColumn(_overdueGrid, "DueAtUtc", "Due Date", 120);

            mainPanel.Controls.Add(headerLabel);
            mainPanel.Controls.Add(metricsPanel);
            mainPanel.Controls.Add(dueTodayLabel);
            mainPanel.Controls.Add(_dueTodayGrid);
            mainPanel.Controls.Add(overdueLabel);
            mainPanel.Controls.Add(_overdueGrid);

            this.Controls.Add(mainPanel);
        }

        private void LoadDashboard()
        {
            try
            {
                // TODO: Load actual dashboard data from service
                // For now, set placeholder values
                _totalBooksCard.Value = "1,245";
                _totalCopiesCard.Value = "5,890";
                _availableCopiesCard.Value = "4,234";
                _activeBorrowingsCard.Value = "1,656";
                _dueSoonCard.Value = "45";
                _overdueCard.Value = "23";
                _lostBooksCard.Value = "12";
                _inventoryWarningsCard.Value = "8";
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
