using System;
using System.Drawing;
using System.Windows.Forms;
using IUIS.Application.Abstractions.StudentSelfService;
using IUIS.Application.StudentSelfService.Enrollment;
using IUIS.SharedUI.DataGridViews;
using IUIS.UserApp.Forms.Student.Dialogs;

namespace IUIS.UserApp.Forms.Student.Pages
{
    public partial class StudentEnrollmentPage : UserControl
    {
        private readonly IStudentEnrollmentService _enrollmentService;
        private readonly string _sessionId;
        
        private Panel _summaryPanel;
        private DataGridView _enrollmentsGrid;
        private DataGridView _subjectsGrid;

        public StudentEnrollmentPage(
            IStudentEnrollmentService enrollmentService,
            string sessionId)
        {
            _enrollmentService = enrollmentService ?? throw new ArgumentNullException(nameof(enrollmentService));
            _sessionId = sessionId ?? throw new ArgumentNullException(nameof(sessionId));
            
            InitializeComponent();
            SetupLayout();
            LoadEnrollmentData();
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
                Text = "Enrollment History",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                Location = new Point(0, 0),
                AutoSize = true
            };

            _summaryPanel = new Panel
            {
                Location = new Point(0, 40),
                Size = new Size(960, 100),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var enrollmentsLabel = new Label
            {
                Text = "Enrollment History",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                Location = new Point(0, 160),
                AutoSize = true
            };

            _enrollmentsGrid = AppDataGridViewFactory.CreateStyledDataGridView();
            _enrollmentsGrid.Location = new Point(0, 190);
            _enrollmentsGrid.Size = new Size(960, 200);
            AppDataGridViewFactory.AddTextBoxColumn(_enrollmentsGrid, "AcademicYear", "Academic Year", 120);
            AppDataGridViewFactory.AddTextBoxColumn(_enrollmentsGrid, "Semester", "Semester", 100);
            AppDataGridViewFactory.AddTextBoxColumn(_enrollmentsGrid, "ProgramName", "Program", 200);
            AppDataGridViewFactory.AddTextBoxColumn(_enrollmentsGrid, "YearLevel", "Year", 80);
            AppDataGridViewFactory.AddTextBoxColumn(_enrollmentsGrid, "Section", "Section", 80);
            AppDataGridViewFactory.AddTextBoxColumn(_enrollmentsGrid, "Status", "Status", 100);
            AppDataGridViewFactory.AddDateColumn(_enrollmentsGrid, "EnrollmentDate", "Date", "MM/dd/yyyy", 120);
            AppDataGridViewFactory.AddButtonColumn(_enrollmentsGrid, "ViewDetails", "Details", "View", 80);

            var subjectsLabel = new Label
            {
                Text = "Current Subjects",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                Location = new Point(0, 410),
                AutoSize = true
            };

            _subjectsGrid = AppDataGridViewFactory.CreateStyledDataGridView();
            _subjectsGrid.Location = new Point(0, 440);
            _subjectsGrid.Size = new Size(960, 250);
            AppDataGridViewFactory.AddTextBoxColumn(_subjectsGrid, "SubjectCode", "Code", 100);
            AppDataGridViewFactory.AddTextBoxColumn(_subjectsGrid, "SubjectTitle", "Title", 250);
            AppDataGridViewFactory.AddTextBoxColumn(_subjectsGrid, "Units", "Units", 60);
            AppDataGridViewFactory.AddTextBoxColumn(_subjectsGrid, "Schedule", "Schedule", 150);
            AppDataGridViewFactory.AddTextBoxColumn(_subjectsGrid, "Room", "Room", 80);
            AppDataGridViewFactory.AddTextBoxColumn(_subjectsGrid, "InstructorName", "Instructor", 200);
            AppDataGridViewFactory.AddButtonColumn(_subjectsGrid, "ViewSubject", "Details", "View", 80);

            _enrollmentsGrid.CellClick += OnEnrollmentsGridCellClick;
            _subjectsGrid.CellClick += OnSubjectsGridCellClick;

            mainPanel.Controls.Add(headerLabel);
            mainPanel.Controls.Add(_summaryPanel);
            mainPanel.Controls.Add(enrollmentsLabel);
            mainPanel.Controls.Add(_enrollmentsGrid);
            mainPanel.Controls.Add(subjectsLabel);
            mainPanel.Controls.Add(_subjectsGrid);

            this.Controls.Add(mainPanel);
        }

        private void LoadEnrollmentData()
        {
            try
            {
                var summary = _enrollmentService.GetEnrollmentSummary(_sessionId);
                BuildSummaryPanel(summary);

                var history = _enrollmentService.GetEnrollmentHistory(_sessionId);
                _enrollmentsGrid.DataSource = history;

                if (summary.CurrentEnrollmentId != null)
                {
                    var details = _enrollmentService.GetEnrollmentDetails(
                        _sessionId,
                        summary.CurrentEnrollmentId);
                    _subjectsGrid.DataSource = details.Subjects;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading enrollment data: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void BuildSummaryPanel(StudentEnrollmentSummaryView summary)
        {
            _summaryPanel.Controls.Clear();

            var labels = new[]
            {
                ("Program", summary.ProgramName),
                ("Year Level", summary.YearLevel.ToString()),
                ("Section", summary.Section),
                ("Academic Year", summary.AcademicYear),
                ("Semester", summary.Semester),
                ("Status", summary.EnrollmentStatus),
                ("Total Units", summary.TotalUnits.ToString()),
                ("Outstanding Balance", $"₱{summary.OutstandingBalance:N2}")
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

                x += 230;
                if (x > 700)
                {
                    x = 20;
                    y += 50;
                }
            }
        }

        private void OnEnrollmentsGridCellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            if (_enrollmentsGrid.Columns[e.ColumnIndex].Name == "ViewDetails")
            {
                var enrollmentId = _enrollmentsGrid.Rows[e.RowIndex].Cells["EnrollmentId"].Value?.ToString();
                if (!string.IsNullOrEmpty(enrollmentId))
                {
                    var dialog = new EnrollmentDetailsDialog(
                        _enrollmentService,
                        _sessionId,
                        enrollmentId);
                    dialog.ShowDialog();
                }
            }
        }

        private void OnSubjectsGridCellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            if (_subjectsGrid.Columns[e.ColumnIndex].Name == "ViewSubject")
            {
                var subjectId = _subjectsGrid.Rows[e.RowIndex].Cells["SubjectId"].Value?.ToString();
                if (!string.IsNullOrEmpty(subjectId))
                {
                    var dialog = new SubjectDetailsDialog(subjectId);
                    dialog.ShowDialog();
                }
            }
        }
    }
}
