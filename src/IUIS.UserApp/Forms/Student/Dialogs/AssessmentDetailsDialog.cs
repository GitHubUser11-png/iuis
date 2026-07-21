using System;
using System.Drawing;
using System.Windows.Forms;
using IUIS.Application.Abstractions.StudentSelfService;
using IUIS.Application.StudentSelfService.Finance;
using IUIS.SharedUI.Forms;
using IUIS.SharedUI.DataGridViews;

namespace IUIS.UserApp.Forms.Student.Dialogs
{
    public partial class AssessmentDetailsDialog : AppDialogBase
    {
        private readonly IStudentFinanceService _financeService;
        private readonly string _sessionId;
        private readonly string _assessmentId;
        
        private Panel _summaryPanel;
        private DataGridView _chargesGrid;
        private DataGridView _paymentsGrid;
        private Button _closeButton;

        public AssessmentDetailsDialog(
            IStudentFinanceService financeService,
            string sessionId,
            string assessmentId)
        {
            _financeService = financeService ?? throw new ArgumentNullException(nameof(financeService));
            _sessionId = sessionId ?? throw new ArgumentNullException(nameof(sessionId));
            _assessmentId = assessmentId ?? throw new ArgumentNullException(nameof(assessmentId));
            
            InitializeComponent();
            SetupLayout();
            LoadAssessmentDetails();
        }

        private void InitializeComponent()
        {
            this.Text = "Assessment Details";
            this.Size = new Size(700, 650);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
        }

        private void SetupLayout()
        {
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                AutoScroll = true
            };

            _summaryPanel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(640, 100),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var chargesLabel = new Label
            {
                Text = "Charges",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                Location = new Point(0, 120),
                AutoSize = true
            };

            _chargesGrid = AppDataGridViewFactory.CreateStyledDataGridView();
            _chargesGrid.Location = new Point(0, 150);
            _chargesGrid.Size = new Size(640, 180);
            AppDataGridViewFactory.AddTextBoxColumn(_chargesGrid, "ChargeType", "Type", 150);
            AppDataGridViewFactory.AddTextBoxColumn(_chargesGrid, "Description", "Description", 300);
            AppDataGridViewFactory.AddCurrencyColumn(_chargesGrid, "Amount", "Amount", 120);

            var paymentsLabel = new Label
            {
                Text = "Payments",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                Location = new Point(0, 350),
                AutoSize = true
            };

            _paymentsGrid = AppDataGridViewFactory.CreateStyledDataGridView();
            _paymentsGrid.Location = new Point(0, 380);
            _paymentsGrid.Size = new Size(640, 180);
            AppDataGridViewFactory.AddDateColumn(_paymentsGrid, "PaymentDate", "Date", 120);
            AppDataGridViewFactory.AddCurrencyColumn(_paymentsGrid, "Amount", "Amount", 120);
            AppDataGridViewFactory.AddTextBoxColumn(_paymentsGrid, "PaymentMethod", "Method", 150);
            AppDataGridViewFactory.AddTextBoxColumn(_paymentsGrid, "ReceiptNumber", "Receipt", 150);

            _closeButton = new Button
            {
                Text = "Close",
                Location = new Point(540, 570),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(229, 231, 235),
                ForeColor = Color.FromArgb(55, 65, 81),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F)
            };
            _closeButton.FlatAppearance.BorderSize = 0;
            _closeButton.Click += (s, e) => this.DialogResult = DialogResult.OK;

            mainPanel.Controls.Add(_summaryPanel);
            mainPanel.Controls.Add(chargesLabel);
            mainPanel.Controls.Add(_chargesGrid);
            mainPanel.Controls.Add(paymentsLabel);
            mainPanel.Controls.Add(_paymentsGrid);
            mainPanel.Controls.Add(_closeButton);

            this.Controls.Add(mainPanel);
        }

        private void LoadAssessmentDetails()
        {
            try
            {
                var details = _financeService.GetAssessmentDetails(_sessionId, _assessmentId);
                BuildSummaryPanel(details);
                _chargesGrid.DataSource = details.Charges;
                _paymentsGrid.DataSource = details.Payments;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading assessment details: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BuildSummaryPanel(StudentAssessmentDetailsView details)
        {
            _summaryPanel.Controls.Clear();

            var labels = new[]
            {
                ("Assessment ID", details.AssessmentId),
                ("Academic Year", details.AcademicYear),
                ("Semester", details.Semester),
                ("Total Assessment", $"₱{details.TotalAssessment:N2}"),
                ("Total Paid", $"₱{details.TotalPaid:N2}"),
                ("Outstanding Balance", $"₱{details.OutstandingBalance:N2}"),
                ("Status", details.AssessmentStatus)
            };

            int x = 20;
            int y = 20;
            foreach (var (label, value) in labels)
            {
                var labelControl = new Label
                {
                    Text = label,
                    Font = new Font("Segoe UI", 8F),
                    ForeColor = Color.FromArgb(107, 114, 128),
                    Location = new Point(x, y),
                    AutoSize = true
                };

                var valueControl = new Label
                {
                    Text = value,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(17, 24, 39),
                    Location = new Point(x, y + 18),
                    AutoSize = true
                };

                _summaryPanel.Controls.Add(labelControl);
                _summaryPanel.Controls.Add(valueControl);

                x += 310;
                if (x > 300)
                {
                    x = 20;
                    y += 50;
                }
            }
        }
    }
}
