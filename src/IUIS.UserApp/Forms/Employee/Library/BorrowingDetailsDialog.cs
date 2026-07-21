using System;
using System.Drawing;
using System.Windows.Forms;
using IUIS.SharedUI.Forms;

namespace IUIS.UserApp.Forms.Employee.Library
{
    public partial class BorrowingDetailsDialog : AppDialogBase
    {
        private readonly string _borrowingId;
        
        private Panel _detailsPanel;
        private Button _closeButton;

        public BorrowingDetailsDialog(string borrowingId)
        {
            _borrowingId = borrowingId ?? throw new ArgumentNullException(nameof(borrowingId));
            
            InitializeComponent();
            SetupLayout();
            LoadBorrowingDetails();
        }

        private void InitializeComponent()
        {
            this.Text = "Borrowing Details";
            this.Size = new Size(600, 450);
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

            _detailsPanel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(540, 330),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            _closeButton = new Button
            {
                Text = "Close",
                Location = new Point(440, 350),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(229, 231, 235),
                ForeColor = Color.FromArgb(55, 65, 81),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F)
            };
            _closeButton.FlatAppearance.BorderSize = 0;
            _closeButton.Click += (s, e) => this.DialogResult = DialogResult.OK;

            mainPanel.Controls.Add(_detailsPanel);
            mainPanel.Controls.Add(_closeButton);

            this.Controls.Add(mainPanel);
        }

        private void LoadBorrowingDetails()
        {
            try
            {
                // TODO: Load actual borrowing details from service
                _detailsPanel.Controls.Clear();

                var labels = new[]
                {
                    ("Borrowing ID", _borrowingId),
                    ("Student", "John Doe (STU-2026-000001)"),
                    ("Book", "Introduction to Algorithms"),
                    ("Issue Date", "01/15/2026"),
                    ("Due Date", "01/29/2026"),
                    ("Return Date", "-"),
                    ("Status", "Active"),
                    ("Issued By", "Librarian Smith")
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

                    _detailsPanel.Controls.Add(labelControl);
                    _detailsPanel.Controls.Add(valueControl);

                    x += 270;
                    if (x > 250)
                    {
                        x = 20;
                        y += 45;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading borrowing details: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
