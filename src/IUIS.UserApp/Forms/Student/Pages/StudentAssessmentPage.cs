using System;
using System.Drawing;
using System.Windows.Forms;
using IUIS.Application.Abstractions.StudentSelfService;
using IUIS.Application.StudentSelfService.Finance;
using IUIS.SharedUI.Controls;
using IUIS.SharedUI.DataGridViews;

namespace IUIS.UserApp.Forms.Student.Pages
{
    public partial class StudentAssessmentPage : UserControl
    {
        private readonly IStudentFinanceService _financeService;
        private readonly string _sessionId;
        
        private Panel _summaryPanel;
        private DataGridView _chargesGrid;
        private DataGridView _paymentsGrid;

        public StudentAssessmentPage(
            IStudentFinanceService financeService,
            string sessionId)
        {
            _financeService = financeService ?? throw new ArgumentNullException(nameof(financeService));
            _sessionId = sessionId ?? throw new ArgumentNullException(nameof(sessionId));
            
            InitializeComponent();
            SetupLayout();
            LoadAssessment();
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
                Text = "Tuition Assessment",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                Location = new Point(0, 0),
                AutoSize = true
            };

            _summaryPanel = new Panel
            {
                Location = new Point(0, 40),
                Size = new Size(960, 120),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var chargesLabel = new Label
            {
                Text = "Assessment Charges",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                Location = new Point(0, 180),
                AutoSize = true
            };

            _chargesGrid = AppDataGridViewFactory.CreateStyledDataGridView();
            _chargesGrid.Location = new Point(0, 210);
            _chargesGrid.Size = new Size(960, 200);
            AppDataGridViewFactory.AddTextBoxColumn(_chargesGrid, "ChargeType", "Type", 150);
            AppDataGridViewFactory.AddTextBoxColumn(_chargesGrid, "Description", "Description", 400);
            AppDataGridViewFactory.AddCurrencyColumn(_chargesGrid, "Amount", "Amount", 120);

            var paymentsLabel = new Label
            {
                Text = "Recent Payments",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                Location = new Point(0, 430),
                AutoSize = true
            };

            _paymentsGrid = AppDataGridViewFactory.CreateStyledDataGridView();
            _paymentsGrid.Location = new Point(0, 460);
            _paymentsGrid.Size = new Size(960, 200);
            AppDataGridViewFactory.AddDateColumn(_paymentsGrid, "PaymentDate", "Date", 120);
            AppDataGridViewFactory.AddCurrencyColumn(_paymentsGrid, "Amount", "Amount", 120);
            AppDataGridViewFactory.AddTextBoxColumn(_paymentsGrid, "PaymentMethod", "Method", 150);
            AppDataGridViewFactory.AddTextBoxColumn(_paymentsGrid, "ReceiptNumber", "Receipt", 150);
            AppDataGridViewFactory.AddButtonColumn(_paymentsGrid, "ViewReceipt", "Receipt", 100);

            _paymentsGrid.CellClick += OnPaymentsGridCellClick;

            mainPanel.Controls.Add(headerLabel);
            mainPanel.Controls.Add(_summaryPanel);
            mainPanel.Controls.Add(chargesLabel);
            mainPanel.Controls.Add(_chargesGrid);
            mainPanel.Controls.Add(paymentsLabel);
            mainPanel.Controls.Add(_paymentsGrid);

            this.Controls.Add(mainPanel);
        }

        private void LoadAssessment()
        {
            try
            {
                var summary = _financeService.GetFinanceSummary(_sessionId);
                BuildSummaryPanel(summary);

                if (summary.CurrentAssessmentId != null)
                {
                    var details = _financeService.GetAssessmentDetails(
                        _sessionId,
                        summary.CurrentAssessmentId);
                    _chargesGrid.DataSource = details.Charges;
                    _paymentsGrid.DataSource = details.Payments;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading assessment: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void BuildSummaryPanel(StudentFinanceSummaryView summary)
        {
            _summaryPanel.Controls.Clear();

            var totalCard = new DashboardMetricCardControl
            {
                Location = new Point(20, 20),
                Title = "Total Assessment",
                Value = $"₱{summary.TotalAssessment:N2}",
                Subtitle = summary.AcademicYear
            };

            var paidCard = new DashboardMetricCardControl
            {
                Location = new Point(240, 20),
                Title = "Total Paid",
                Value = $"₱{summary.TotalPaid:N2}",
                Subtitle = "To Date"
            };
            paidCard.SetSuccessState();

            var balanceCard = new DashboardMetricCardControl
            {
                Location = new Point(460, 20),
                Title = "Outstanding Balance",
                Value = $"₱{summary.OutstandingBalance:N2}",
                Subtitle = summary.AssessmentStatus
            };

            if (summary.OutstandingBalance > 0)
                balanceCard.SetWarningState();
            else
                balanceCard.SetSuccessState();

            var scholarshipCard = new DashboardMetricCardControl
            {
                Location = new Point(680, 20),
                Title = "Scholarship Deduction",
                Value = $"₱{summary.ScholarshipDeduction:N2}",
                Subtitle = "Applied"
            };
            scholarshipCard.SetNormalState();

            _summaryPanel.Controls.Add(totalCard);
            _summaryPanel.Controls.Add(paidCard);
            _summaryPanel.Controls.Add(balanceCard);
            _summaryPanel.Controls.Add(scholarshipCard);
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
