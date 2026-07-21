using System;
using System.Drawing;
using System.Windows.Forms;
using IUIS.SharedUI.Forms;

namespace IUIS.UserApp.Forms.Student.Dialogs
{
    public partial class SubjectDetailsDialog : AppDialogBase
    {
        private readonly string _subjectId;
        
        private Panel _detailsPanel;
        private Button _closeButton;

        public SubjectDetailsDialog(string subjectId)
        {
            _subjectId = subjectId ?? throw new ArgumentNullException(nameof(subjectId));
            
            InitializeComponent();
            SetupLayout();
            LoadSubjectDetails();
        }

        private void InitializeComponent()
        {
            this.Text = "Subject Details";
            this.Size = new Size(500, 400);
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
                Padding = new Padding(20)
            };

            _detailsPanel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(440, 280),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            _closeButton = new Button
            {
                Text = "Close",
                Location = new Point(340, 300),
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

        private void LoadSubjectDetails()
        {
            try
            {
                // TODO: Load actual subject details from service
                // For now, display placeholder
                var labels = new[]
                {
                    ("Subject ID", _subjectId),
                    ("Subject Code", "CS101"),
                    ("Subject Title", "Introduction to Computer Science"),
                    ("Units", "3"),
                    ("Schedule", "MWF 9:00-10:00 AM"),
                    ("Room", "Room 101"),
                    ("Instructor", "Dr. Smith")
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
                        AutoSize = true
                    };

                    _detailsPanel.Controls.Add(labelControl);
                    _detailsPanel.Controls.Add(valueControl);

                    y += 45;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading subject details: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
