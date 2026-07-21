using System;
using System.Drawing;
using System.Windows.Forms;

namespace IUIS.SharedUI.Controls
{
    public partial class StudentStatusCardControl : UserControl
    {
        private Label _studentNameLabel;
        private Label _studentIdLabel;
        private Label _programLabel;
        private Label _yearLevelLabel;
        private Label _statusLabel;
        private Panel _statusIndicator;

        public StudentStatusCardControl()
        {
            InitializeComponent();
            SetupLayout();
        }

        public string StudentName
        {
            get => _studentNameLabel.Text;
            set => _studentNameLabel.Text = value;
        }

        public string StudentId
        {
            get => _studentIdLabel.Text;
            set => _studentIdLabel.Text = value;
        }

        public string Program
        {
            get => _programLabel.Text;
            set => _programLabel.Text = value;
        }

        public string YearLevel
        {
            get => _yearLevelLabel.Text;
            set => _yearLevelLabel.Text = value;
        }

        public string Status
        {
            get => _statusLabel.Text;
            set => _statusLabel.Text = value;
        }

        private void InitializeComponent()
        {
            this.Size = new Size(350, 120);
            this.BackColor = Color.White;
            this.BorderStyle = BorderStyle.FixedSingle;
        }

        private void SetupLayout()
        {
            _statusIndicator = new Panel
            {
                Size = new Size(4, 120),
                Dock = DockStyle.Left,
                BackColor = Color.FromArgb(34, 197, 94)
            };

            _studentNameLabel = new Label
            {
                Text = "Student Name",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                Location = new Point(12, 8),
                AutoSize = true
            };

            _studentIdLabel = new Label
            {
                Text = "STU-2026-000001",
                Font = new Font("Segoe UI", 8F),
                ForeColor = Color.FromArgb(107, 114, 128),
                Location = new Point(12, 28),
                AutoSize = true
            };

            _programLabel = new Label
            {
                Text = "Program: BS Computer Science",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(55, 65, 81),
                Location = new Point(12, 48),
                AutoSize = true
            };

            _yearLevelLabel = new Label
            {
                Text = "Year Level: 3rd Year",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(55, 65, 81),
                Location = new Point(12, 68),
                AutoSize = true
            };

            _statusLabel = new Label
            {
                Text = "Active",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(34, 197, 94),
                Location = new Point(12, 88),
                AutoSize = true
            };

            this.Controls.Add(_statusIndicator);
            this.Controls.Add(_studentNameLabel);
            this.Controls.Add(_studentIdLabel);
            this.Controls.Add(_programLabel);
            this.Controls.Add(_yearLevelLabel);
            this.Controls.Add(_statusLabel);
        }

        public void SetStatus(string status, Color statusColor)
        {
            _statusLabel.Text = status;
            _statusLabel.ForeColor = statusColor;
            _statusIndicator.BackColor = statusColor;
        }

        public void SetActive()
        {
            SetStatus("Active", Color.FromArgb(34, 197, 94));
        }

        public void SetInactive()
        {
            SetStatus("Inactive", Color.FromArgb(156, 163, 175));
        }

        public void SetProbation()
        {
            SetStatus("On Probation", Color.FromArgb(245, 158, 11));
        }

        public void SetSuspended()
        {
            SetStatus("Suspended", Color.FromArgb(239, 68, 68));
        }
    }
}
