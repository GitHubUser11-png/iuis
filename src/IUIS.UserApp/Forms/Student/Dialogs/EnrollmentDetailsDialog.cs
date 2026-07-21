using System;
using System.Drawing;
using System.Windows.Forms;
using IUIS.Application.Abstractions.StudentSelfService;
using IUIS.Application.StudentSelfService.Enrollment;
using IUIS.SharedUI.Forms;
using IUIS.SharedUI.DataGridViews;

namespace IUIS.UserApp.Forms.Student.Dialogs
{
    public partial class EnrollmentDetailsDialog : AppDialogBase
    {
        private readonly IStudentEnrollmentService _enrollmentService;
        private readonly string _sessionId;
        private readonly string _enrollmentId;
        
        private Panel _detailsPanel;
        private DataGridView _subjectsGrid;
        private Button _closeButton;

        public EnrollmentDetailsDialog(
            IStudentEnrollmentService enrollmentService,
            string sessionId,
            string enrollmentId)
        {
            _enrollmentService = enrollmentService ?? throw new ArgumentNullException(nameof(enrollmentService));
            _sessionId = sessionId ?? throw new ArgumentNullException(nameof(sessionId));
            _enrollmentId = enrollmentId ?? throw new ArgumentNullException(nameof(enrollmentId));
            
            InitializeComponent();
            SetupLayout();
            LoadEnrollmentDetails();
        }

        private void InitializeComponent()
        {
            this.Text = "Enrollment Details";
            this.Size = new Size(700, 600);
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
                Size = new Size(640, 150),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var subjectsLabel = new Label
            {
                Text = "Enrolled Subjects",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                Location = new Point(0, 170),
                AutoSize = true
            };

            _subjectsGrid = AppDataGridViewFactory.CreateStyledDataGridView();
            _subjectsGrid.Location = new Point(0, 200);
            _subjectsGrid.Size = new Size(640, 300);
            AppDataGridViewFactory.AddTextBoxColumn(_subjectsGrid, "SubjectCode", "Code", 100);
            AppDataGridViewFactory.AddTextBoxColumn(_subjectsGrid, "SubjectTitle", "Title", 200);
            AppDataGridViewFactory.AddTextBoxColumn(_subjectsGrid, "Units", "Units", 60);
            AppDataGridViewFactory.AddTextBoxColumn(_subjectsGrid, "Schedule", "Schedule", 120);
            AppDataGridViewFactory.AddTextBoxColumn(_subjectsGrid, "Room", "Room", 80);
            AppDataGridViewFactory.AddTextBoxColumn(_subjectsGrid, "InstructorName", "Instructor", 150);

            _closeButton = new Button
            {
                Text = "Close",
                Location = new Point(540, 520),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(229, 231, 235),
                ForeColor = Color.FromArgb(55, 65, 81),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F)
            };
            _closeButton.FlatAppearance.BorderSize = 0;
            _closeButton.Click += (s, e) => this.DialogResult = DialogResult.OK;

            mainPanel.Controls.Add(_detailsPanel);
            mainPanel.Controls.Add(subjectsLabel);
            mainPanel.Controls.Add(_subjectsGrid);
            mainPanel.Controls.Add(_closeButton);

            this.Controls.Add(mainPanel);
        }

        private void LoadEnrollmentDetails()
        {
            try
            {
                var details = _enrollmentService.GetEnrollmentDetails(_sessionId, _enrollmentId);
                BuildDetailsPanel(details);
                _subjectsGrid.DataSource = details.Subjects;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading enrollment details: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BuildDetailsPanel(StudentEnrollmentDetailsView details)
        {
            _detailsPanel.Controls.Clear();

            var labels = new[]
            {
                ("Enrollment ID", details.EnrollmentId),
                ("Program", details.ProgramName),
                ("Year Level", details.YearLevel.ToString()),
                ("Section", details.Section),
                ("Academic Year", details.AcademicYear),
                ("Semester", details.Semester),
                ("Status", details.EnrollmentStatus),
                ("Enrollment Date", details.EnrollmentDate.ToString("MM/dd/yyyy"))
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
