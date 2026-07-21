using System;
using System.Drawing;
using System.Windows.Forms;
using IUIS.Application.StudentSelfService.Finance;
using IUIS.SharedUI.Forms;

namespace IUIS.UserApp.Forms.Student.Dialogs
{
    public partial class PaymentReceiptDialog : AppDialogBase
    {
        private readonly StudentPaymentReceiptView _receipt;
        
        private Panel _receiptPanel;
        private Button _printButton;
        private Button _closeButton;

        public PaymentReceiptDialog(StudentPaymentReceiptView receipt)
        {
            _receipt = receipt ?? throw new ArgumentNullException(nameof(receipt));
            
            InitializeComponent();
            SetupLayout();
            BuildReceiptPanel();
        }

        private void InitializeComponent()
        {
            this.Text = "Payment Receipt";
            this.Size = new Size(500, 500);
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

            _receiptPanel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(440, 380),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var buttonPanel = new Panel
            {
                Location = new Point(0, 400),
                Size = new Size(440, 40)
            };

            _printButton = new Button
            {
                Text = "Print",
                Location = new Point(240, 0),
                Size = new Size(90, 35),
                BackColor = Color.FromArgb(79, 70, 229),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F)
            };
            _printButton.FlatAppearance.BorderSize = 0;
            _printButton.Click += OnPrintClick;

            _closeButton = new Button
            {
                Text = "Close",
                Location = new Point(340, 0),
                Size = new Size(90, 35),
                BackColor = Color.FromArgb(229, 231, 235),
                ForeColor = Color.FromArgb(55, 65, 81),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F)
            };
            _closeButton.FlatAppearance.BorderSize = 0;
            _closeButton.Click += (s, e) => this.DialogResult = DialogResult.OK;

            buttonPanel.Controls.Add(_printButton);
            buttonPanel.Controls.Add(_closeButton);

            mainPanel.Controls.Add(_receiptPanel);
            mainPanel.Controls.Add(buttonPanel);

            this.Controls.Add(mainPanel);
        }

        private void BuildReceiptPanel()
        {
            _receiptPanel.Controls.Clear();

            var headerLabel = new Label
            {
                Text = "OFFICIAL RECEIPT",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                Location = new Point(120, 20),
                AutoSize = true
            };

            var dividerPanel = new Panel
            {
                Location = new Point(20, 50),
                Size = new Size(400, 2),
                BackColor = Color.FromArgb(79, 70, 229)
            };

            int y = 70;
 var labels = new[]
            {
                ("Receipt Number", _receipt.ReceiptNumber),
                ("Payment Date", _receipt.PaymentDate.ToString("MM/dd/yyyy HH:mm")),
                ("Student Name", _receipt.StudentName),
                ("Student ID", _receipt.StudentId),
                ("Amount", $"₱{_receipt.Amount:N2}"),
                ("Payment Method", _receipt.PaymentMethod),
                ("Reference Number", _receipt.ReferenceNumber),
                ("Academic Year", _receipt.AcademicYear),
                ("Semester", _receipt.Semester),
                ("Processed By", _receipt.ProcessedBy)
            };

            foreach (var (label, value) in labels)
            {
                var labelControl = new Label
                {
                    Text = label,
                    Font = new Font("Segoe UI", 8F),
                    ForeColor = Color.FromArgb(107, 114, 128),
                    Location = new Point(20, y),
                    AutoSize = true
                };

                var valueControl = new Label
                {
                    Text = value,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(17, 24, 39),
                    Location = new Point(20, y + 18),
                    AutoSize = true
                };

                _receiptPanel.Controls.Add(labelControl);
                _receiptPanel.Controls.Add(valueControl);

                y += 45;
            }

            if (!string.IsNullOrEmpty(_receipt.Remarks))
            {
                var remarksLabel = new Label
                {
                    Text = "Remarks:",
                    Font = new Font("Segoe UI", 8F),
                    ForeColor = Color.FromArgb(107, 114, 128),
                    Location = new Point(20, y),
                    AutoSize = true
                };

                var remarksValue = new Label
                {
                    Text = _receipt.Remarks,
                    Font = new Font("Segoe UI", 9F),
                    ForeColor = Color.FromArgb(17, 24, 39),
                    Location = new Point(20, y + 18),
                    Width = 400,
                    Height = 40
                };

                _receiptPanel.Controls.Add(remarksLabel);
                _receiptPanel.Controls.Add(remarksValue);
            }

            _receiptPanel.Controls.Add(headerLabel);
            _receiptPanel.Controls.Add(dividerPanel);
        }

        private void OnPrintClick(object sender, EventArgs e)
        {
            MessageBox.Show("Print functionality to be implemented.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
