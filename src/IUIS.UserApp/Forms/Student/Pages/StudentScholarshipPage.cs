using System;
using System.Drawing;
using System.Windows.Forms;
using IUIS.Application.Abstractions.StudentSelfService;
using IUIS.Application.StudentSelfService.Scholarship;
using IUIS.SharedUI.DataGridViews;

namespace IUIS.UserApp.Forms.Student.Pages
{
    public partial class StudentScholarshipPage : UserControl
    {
        private readonly IStudentScholarshipService _scholarshipService;
        private readonly string _sessionId;
        
        private TabControl _tabControl;
        private DataGridView _availableProgramsGrid;
        private DataGridView _myApplicationsGrid;

        public StudentScholarshipPage(
            IStudentScholarshipService scholarshipService,
            string sessionId)
        {
            _scholarshipService = scholarshipService ?? throw new ArgumentNullException(nameof(scholarshipService));
            _sessionId = sessionId ?? throw new ArgumentNullException(nameof(sessionId));
            
            InitializeComponent();
            SetupLayout();
            LoadScholarshipData();
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
                Text = "Scholarships",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                Location = new Point(0, 0),
                AutoSize = true
            };

            _tabControl = new TabControl
            {
                Location = new Point(0, 40),
                Size = new Size(960, 620),
                Font = new Font("Segoe UI", 9F)
            };

            var availableTabPage = new TabPage("Available Programs");
            _availableProgramsGrid = AppDataGridViewFactory.CreateStyledDataGridView();
            _availableProgramsGrid.Dock = DockStyle.Fill;
            AppDataGridViewFactory.AddTextBoxColumn(_availableProgramsGrid, "ScholarshipName", "Name", 200);
            AppDataGridViewFactory.AddTextBoxColumn(_availableProgramsGrid, "Provider", "Provider", 150);
            AppDataGridViewFactory.AddTextBoxColumn(_availableProgramsGrid, "CoverageType", "Coverage", 120);
            AppDataGridViewFactory.AddCurrencyColumn(_availableProgramsGrid, "CoverageAmount", "Amount", 120);
            AppDataGridViewFactory.AddDateColumn(_availableProgramsGrid, "ApplicationDeadline", "Deadline", "MM/dd/yyyy", 120);
            AppDataGridViewFactory.AddButtonColumn(_availableProgramsGrid, "Apply", "Apply", 80);

            _availableProgramsGrid.CellClick += OnAvailableProgramsGridCellClick;
            availableTabPage.Controls.Add(_availableProgramsGrid);

            var myApplicationsTabPage = new TabPage("My Applications");
            _myApplicationsGrid = AppDataGridViewFactory.CreateStyledDataGridView();
            _myApplicationsGrid.Dock = DockStyle.Fill;
            AppDataGridViewFactory.AddTextBoxColumn(_myApplicationsGrid, "ScholarshipName", "Name", 200);
            AppDataGridViewFactory.AddTextBoxColumn(_myApplicationsGrid, "Provider", "Provider", 150);
            AppDataGridViewFactory.AddDateColumn(_myApplicationsGrid, "ApplicationDate", "Date", "MM/dd/yyyy", 120);
            AppDataGridViewFactory.AddTextBoxColumn(_myApplicationsGrid, "Status", "Status", 120);
            AppDataGridViewFactory.AddTextBoxColumn(_myApplicationsGrid, "AcademicYear", "Year", 100);
            AppDataGridViewFactory.AddTextBoxColumn(_myApplicationsGrid, "Semester", "Sem", 80);

            myApplicationsTabPage.Controls.Add(_myApplicationsGrid);

            _tabControl.TabPages.Add(availableTabPage);
            _tabControl.TabPages.Add(myApplicationsTabPage);

            mainPanel.Controls.Add(headerLabel);
            mainPanel.Controls.Add(_tabControl);

            this.Controls.Add(mainPanel);
        }

        private void LoadScholarshipData()
        {
            try
            {
                var availablePrograms = _scholarshipService.GetAvailablePrograms(_sessionId);
                _availableProgramsGrid.DataSource = availablePrograms;

                var myApplications = _scholarshipService.GetMyApplications(_sessionId);
                _myApplicationsGrid.DataSource = myApplications;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading scholarship data: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void OnAvailableProgramsGridCellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            if (_availableProgramsGrid.Columns[e.ColumnIndex].Name == "Apply")
            {
                var programId = _availableProgramsGrid.Rows[e.RowIndex].Cells["ProgramId"].Value?.ToString();
                var programName = _availableProgramsGrid.Rows[e.RowIndex].Cells["ScholarshipName"].Value?.ToString();
                
                if (!string.IsNullOrEmpty(programId))
                {
                    var dialog = new ScholarshipProgramDetailsDialog(programId, programName);
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        var applicationDialog = new ScholarshipApplicationDialog(
                            _scholarshipService,
                            _sessionId,
                            programId);
                        
                        if (applicationDialog.ShowDialog() == DialogResult.OK)
                        {
                            LoadScholarshipData();
                        }
                    }
                }
            }
        }
    }
}
