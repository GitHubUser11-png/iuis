using System;
using System.Drawing;
using System.Windows.Forms;
using IUIS.SharedUI.Forms;

namespace IUIS.UserApp.Forms.Employee.Counseling
{
    public partial class CounselingSessionDetailsDialog : AppDialogBase
    {
        private readonly string _sessionId;
        
        private Panel _detailsPanel;
        private Button _closeButton;

        public CounselingSessionDetailsDialog(string sessionId)
        {
            _sessionId = sessionId ?? throw new ArgumentNullException(nameof(sessionId));
            
            InitializeComponent();
            SetupLayout();
            LoadSessionDetails();
        }

        private void InitializeComponent()
        {
            this.Text = "Counseling Session Details";
            this.Size = new Size(600, 500);
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
                Size = new Size(540, 380),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            _closeButton = new Button
            {
                Text = "Close",
                Location = new Point(440, 400),
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

        private void LoadSessionDetails()
        {
            try
            {
                // TODO: Load actual session details from service
                _detailsPanel.Controls.Clear();

                var labels = new[]
                {
                    ("Session ID", _sessionId),
                    ("Student", "John Doe (STU-2026-000001)"),
                    ("Session Type", "Individual"),
                    ("Priority", "Medium"),
                    ("Scheduled Date", "01/20/2026"),
                    ("Scheduled Time", "10:00 AM"),
                    ("Location", "Counseling Room 1"),
                    ("Status", "Scheduled"),
                    ("Counselor", "Dr. Smith")
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
                MessageBox.Show($"Error loading session details: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
