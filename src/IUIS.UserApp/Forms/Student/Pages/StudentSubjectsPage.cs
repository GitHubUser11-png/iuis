using System;
using System.Drawing;
using System.Windows.Forms;
using IUIS.Application.Abstractions.StudentSelfService;
using IUIS.Application.StudentSelfService.Enrollment;
using IUIS.SharedUI.DataGridViews;
using IUIS.UserApp.Forms.Student.Dialogs;

namespace IUIS.UserApp.Forms.Student.Pages
{
    public partial class StudentSubjectsPage : UserControl
    {
        private readonly IStudentEnrollmentService _enrollmentService;
        private readonly string _sessionId;
        
        private DataGridView _subjectsGrid;

        public StudentSubjectsPage(
            IStudentEnrollmentService enrollmentService,
            string sessionId)
        {
            _enrollmentService = enrollmentService ?? throw new ArgumentNullException(nameof(enrollmentService));
            _sessionId = sessionId ?? throw new ArgumentNullException(nameof(sessionId));
            
            InitializeComponent();
            SetupLayout();
            LoadSubjects();
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
                Text = "My Subjects",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                Location = new Point(0, 0),
                AutoSize = true
            };

            _subjectsGrid = AppDataGridViewFactory.CreateStyledDataGridView();
            _subjectsGrid.Location = new Point(0, 40);
            _subjectsGrid.Size = new Size(960, 620);
            AppDataGridViewFactory.AddTextBoxColumn(_subjectsGrid, "SubjectCode", "Code", 100);
            AppDataGridViewFactory.AddTextBoxColumn(_subjectsGrid, "SubjectTitle", "Title", 250);
            AppDataGridViewFactory.AddTextBoxColumn(_subjectsGrid, "Units", "Units", 60);
            AppDataGridViewFactory.AddTextBoxColumn(_subjectsGrid, "Schedule", "Schedule", 150);
            AppDataGridViewFactory.AddTextBoxColumn(_subjectsGrid, "Room", "Room", 80);
            AppDataGridViewFactory.AddTextBoxColumn(_subjectsGrid, "InstructorName", "Instructor", 200);
            AppDataGridViewFactory.AddButtonColumn(_subjectsGrid, "ViewDetails", "Details", "View", 80);

            _subjectsGrid.CellClick += OnSubjectsGridCellClick;

            mainPanel.Controls.Add(headerLabel);
            mainPanel.Controls.Add(_subjectsGrid);

            this.Controls.Add(mainPanel);
        }

        private void LoadSubjects()
        {
            try
            {
                var summary = _enrollmentService.GetEnrollmentSummary(_sessionId);
                
                if (summary.CurrentEnrollmentId != null)
                {
                    var details = _enrollmentService.GetEnrollmentDetails(
                        _sessionId,
                        summary.CurrentEnrollmentId);
                    _subjectsGrid.DataSource = details.Subjects;
                }
                else
                {
                    _subjectsGrid.DataSource = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading subjects: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void OnSubjectsGridCellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            if (_subjectsGrid.Columns[e.ColumnIndex].Name == "ViewDetails")
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
