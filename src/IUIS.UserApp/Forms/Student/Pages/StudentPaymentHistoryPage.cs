using System;
using System.Drawing;
using System.Windows.Forms;
using IUIS.Application.Abstractions.StudentSelfService;
using IUIS.Application.StudentSelfService.Finance;
using IUIS.SharedUI.Controls;
using IUIS.SharedUI.DataGridViews;
using IUIS.UserApp.Forms.Student.Dialogs;

namespace IUIS.UserApp.Forms.Student.Pages
{
    public partial class StudentPaymentHistoryPage : UserControl
    {
        private readonly IStudentFinanceService _financeService;
        private readonly string _sessionId;
        
        private FilterBarControl _filterBar;
        private DataGridView _paymentsGrid;

        public StudentPaymentHistoryPage(
            IStudentFinanceService financeService,
            string sessionId)
        {
            _financeService = financeService;
            _sessionId = sessionId ?? throw new ArgumentNullException(nameof(sessionId));
            
            InitializeComponent();
            SetupLayout();
            LoadPaymentHistory();
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
                Text = "Payment History",
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
            _filterBar.SetFilterOptions(new[] { "All", "This Month", "This Year", "Last Year" });

            _paymentsGrid = AppDataGridViewFactory.CreateStyledDataGridView();
            _paymentsGrid.Location = new Point(0, 90);
            _paymentsGrid.Size = new Size(960, 570);
            AppDataGridViewFactory.AddDateColumn(_paymentsGrid, "PaymentDate", "Date", "MM/dd/yyyy", 120);
            AppDataGridViewFactory.AddCurrencyColumn(_paymentsGrid, "Amount", "Amount", 120);
            AppDataGridViewFactory.AddTextBoxColumn(_paymentsGrid, "PaymentMethod", "Method", 150);
            AppDataGridViewFactory.AddTextBoxColumn(_paymentsGrid, "ReceiptNumber", "Receipt", 150);
            AppDataGridViewFactory.AddTextBoxColumn(_paymentsGrid, "Status", "Status", 100);
            AppDataGridViewFactory.AddButtonColumn(_paymentsGrid, "ViewReceipt", "Receipt", "View", 100);

            _filterBar.SearchRequested += (s, e) => LoadPaymentHistory();
            _filterBar.ClearRequested += (s, e) => LoadPaymentHistory();
            _paymentsGrid.CellClick += OnPaymentsGridCellClick;

            mainPanel.Controls.Add(headerLabel);
            mainPanel.Controls.Add(_filterBar);
            mainPanel.Controls.Add(_paymentsGrid);

            this.Controls.Add(mainPanel);
        }

        private void LoadPaymentHistory()
        {
            try
            {
                var payments = _financeService.GetPaymentHistory(_sessionId);
                _paymentsGrid.DataSource = payments;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading payment history: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void OnPaymentsGridCellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            if (_paymentsGrid.Columns[e.ColumnIndex].Name == "ViewReceipt")
            {
                var paymentId = _paymentsGrid.Rows[e.RowIndex].Cells["PaymentId"].Value?.ToString();
                if (!string.IsNullOrEmpty(paymentId))
                {
                    var receipt = _financeService.GetPaymentReceipt(_sessionId, paymentId);
                    var dialog = new PaymentReceiptDialog(receipt);
                    dialog.ShowDialog();
                }
            }
        }
    }
}
