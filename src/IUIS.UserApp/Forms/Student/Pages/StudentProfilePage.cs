using System;
using System.Drawing;
using System.Windows.Forms;
using IUIS.Application.Abstractions.StudentSelfService;
using IUIS.Application.StudentSelfService.Profile;
using IUIS.SharedUI.Controls;
using IUIS.SharedUI.DataGridViews;
using IUIS.UserApp.Forms.Student.Dialogs;

namespace IUIS.UserApp.Forms.Student.Pages
{
    public partial class StudentProfilePage : UserControl
    {
        private readonly IStudentProfileService _profileService;
        private readonly string _sessionId;
        
        private StudentStatusCardControl _statusCard;
        private Panel _profilePanel;
        private Button _requestCorrectionButton;
        private DataGridView _correctionsGrid;

        public StudentProfilePage(
            IStudentProfileService profileService,
            string sessionId)
        {
            _profileService = profileService ?? throw new ArgumentNullException(nameof(profileService));
            _sessionId = sessionId ?? throw new ArgumentNullException(nameof(sessionId));
            
            InitializeComponent();
            SetupLayout();
            LoadProfile();
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
                BackColor = Color.FromArgb(249, 250, 251),
                AutoScroll = true
            };

            var headerLabel = new Label
            {
                Text = "Student Profile",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                Location = new Point(0, 0),
                AutoSize = true
            };

            _statusCard = new StudentStatusCardControl
            {
                Location = new Point(0, 40),
                Size = new Size(350, 120)
            };

            _profilePanel = new Panel
            {
                Location = new Point(0, 180),
                Size = new Size(960, 250),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            _requestCorrectionButton = new Button
            {
                Text = "Request Correction",
                Location = new Point(0, 450),
                Size = new Size(180, 35),
                BackColor = Color.FromArgb(79, 70, 229),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F)
            };
            _requestCorrectionButton.FlatAppearance.BorderSize = 0;
            _requestCorrectionButton.Click += OnRequestCorrectionClick;

            var correctionsLabel = new Label
            {
                Text = "Correction Requests",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                Location = new Point(0, 500),
                AutoSize = true
            };

            _correctionsGrid = AppDataGridViewFactory.CreateStyledDataGridView();
            _correctionsGrid.Location = new Point(0, 530);
            _correctionsGrid.Size = new Size(960, 200);
            AppDataGridViewFactory.AddTextBoxColumn(_correctionsGrid, "RequestedField", "Field", 150);
            AppDataGridViewFactory.AddTextBoxColumn(_correctionsGrid, "CurrentValue", "Current Value", 150);
            AppDataGridViewFactory.AddTextBoxColumn(_correctionsGrid, "RequestedValue", "Requested Value", 150);
            AppDataGridViewFactory.AddDateColumn(_correctionsGrid, "RequestDateUtc", "Date", "MM/dd/yyyy", 120);
            AppDataGridViewFactory.AddTextBoxColumn(_correctionsGrid, "Status", "Status", 120);

            mainPanel.Controls.Add(headerLabel);
            mainPanel.Controls.Add(_statusCard);
            mainPanel.Controls.Add(_profilePanel);
            mainPanel.Controls.Add(_requestCorrectionButton);
            mainPanel.Controls.Add(correctionsLabel);
            mainPanel.Controls.Add(_correctionsGrid);

            this.Controls.Add(mainPanel);
        }

        private void LoadProfile()
        {
            try
            {
                var profile = _profileService.GetProfile(_sessionId);
                
                _statusCard.StudentName = $"{profile.LastName}, {profile.FirstName}";
                _statusCard.Program = profile.ProgramName;
                _statusCard.YearLevel = $"Year Level: {profile.YearLevel}";
                _statusCard.SetActive();

                BuildProfilePanel(profile);

                var corrections = _profileService.GetCorrectionRequests(_sessionId);
                _correctionsGrid.DataSource = corrections;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading profile: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void BuildProfilePanel(StudentProfileView profile)
        {
            _profilePanel.Controls.Clear();

            int y = 20;
            int leftColumn = 20;
            int rightColumn = 480;

            AddProfileField(_profilePanel, "Student ID", profile.StudentId, leftColumn, ref y);
            AddProfileField(_profilePanel, "First Name", profile.FirstName, leftColumn, ref y);
            AddProfileField(_profilePanel, "Middle Name", profile.MiddleName, leftColumn, ref y);
            AddProfileField(_profilePanel, "Last Name", profile.LastName, leftColumn, ref y);
            AddProfileField(_profilePanel, "Suffix", profile.Suffix, leftColumn, ref y);

            y = 20;
            AddProfileField(_profilePanel, "Birth Date", profile.BirthDate.ToString("MM/dd/yyyy"), rightColumn, ref y);
            AddProfileField(_profilePanel, "Sex", profile.Sex, rightColumn, ref y);
            AddProfileField(_profilePanel, "Address", profile.Address, rightColumn, ref y);
            AddProfileField(_profilePanel, "Contact Number", profile.ContactNumber, rightColumn, ref y);
            AddProfileField(_profilePanel, "Email Address", profile.EmailAddress, rightColumn, ref y);
        }

        private void AddProfileField(Panel panel, string label, string value, int x, ref int y)
        {
            var field = new DetailFieldControl
            {
                Location = new Point(x, y),
                LabelText = label,
                ValueText = value,
                ShowSeparator = false
            };
            panel.Controls.Add(field);
            y += 45;
        }

        private void OnRequestCorrectionClick(object sender, EventArgs e)
        {
            var dialog = new StudentProfileCorrectionDialog(
                _profileService,
                _sessionId);
            
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                LoadProfile();
            }
        }
    }
}
