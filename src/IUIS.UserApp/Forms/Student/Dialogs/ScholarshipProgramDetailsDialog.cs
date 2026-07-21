using System;
using System.Drawing;
using System.Windows.Forms;
using IUIS.Application.StudentSelfService.Scholarship;
using IUIS.SharedUI.Forms;

namespace IUIS.UserApp.Forms.Student.Dialogs
{
    public partial class ScholarshipProgramDetailsDialog : AppDialogBase
    {
        private readonly string _programId;
        private readonly string _programName;
        
        private Panel _detailsPanel;
        private Button _applyButton;
        private Button _closeButton;

        public ScholarshipProgramDetailsDialog(string programId, string programName)
        {
            _programId = programId ?? throw new ArgumentNullException(nameof(programId));
            _programName = programName ?? throw new ArgumentNullException(nameof(programName));
            
            InitializeComponent();
            SetupLayout();
            LoadProgramDetails();
        }

        private void InitializeComponent()
        {
            this.Text = "Scholarship Program Details";
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

            _detailsPanel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(440, 360),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var buttonPanel = new Panel
            {
                Location = new Point(0, 380),
                Size = new Size(440, 40)
            };

            _applyButton = new Button
            {
                Text = "Apply Now",
                Location = new Point(240, 0),
                Size = new Size(90, 35),
                BackColor = Color.FromArgb(79, 70, 229),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F)
            };
            _applyButton.FlatAppearance.BorderSize = 0;
            _applyButton.Click += (s, e) => this.DialogResult = DialogResult.OK;

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
            _closeButton.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            buttonPanel.Controls.Add(_applyButton);
            buttonPanel.Controls.Add(_closeButton);

            mainPanel.Controls.Add(_detailsPanel);
            mainPanel.Controls.Add(buttonPanel);

            this.Controls.Add(mainPanel);
        }

        private void LoadProgramDetails()
        {
            try
            {
                // TODO: Load actual program details from service
                // For now, display placeholder
                var labels = new[]
                {
                    ("Program Name", _programName),
                    ("Provider", "University Foundation"),
                    ("Coverage Type", "Full Tuition"),
                    ("Coverage Amount", "₱50,000.00"),
                    ("Coverage Percentage", "100%"),
                    ("Application Deadline", "12/31/2026"),
                    ("Requirements", "GPA 3.0+, No failing grades, Active enrollment")
                };

                int y = 20;
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
                        Width = 400,
                        Height = label == "Requirements" ? 60 : 20
                    };

                    _detailsPanel.Controls.Add(labelControl);
                    _detailsPanel.Controls.Add(valueControl);

                    y += label == "Requirements" ? 85 : 45;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading program details: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
