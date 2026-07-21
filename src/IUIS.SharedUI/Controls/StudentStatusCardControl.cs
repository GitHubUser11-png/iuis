using System;
using System.Drawing;
using System.Windows.Forms;

using IUIS.SharedUI.Theme;

namespace IUIS.SharedUI.Controls
{
    /// <summary>
    /// Rounded student summary card with a status accent strip and
    /// table-based layout (DPI safe).
    /// </summary>
    public partial class StudentStatusCardControl : UserControl
    {
        private Label _studentNameLabel;
        private Label _studentIdLabel;
        private Label _programLabel;
        private Label _yearLevelLabel;
        private Label _statusLabel;
        private TableLayoutPanel _layout;
        private Color _statusColor;

        public StudentStatusCardControl()
        {
            _statusColor = UiTheme.Success;
            InitializeComponent();
            SetupLayout();
        }

        public string StudentName
        {
            get { return _studentNameLabel.Text; }
            set { _studentNameLabel.Text = value; }
        }

        public string StudentId
        {
            get { return _studentIdLabel.Text; }
            set { _studentIdLabel.Text = value; }
        }

        public string Program
        {
            get { return _programLabel.Text; }
            set { _programLabel.Text = value; }
        }

        public string YearLevel
        {
            get { return _yearLevelLabel.Text; }
            set { _yearLevelLabel.Text = value; }
        }

        public string Status
        {
            get { return _statusLabel.Text; }
            set { _statusLabel.Text = value; }
        }

        private void InitializeComponent()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);
            Size = new Size(350, 132);
            BackColor = UiTheme.Surface;
            BorderStyle = BorderStyle.None;
        }

        private void SetupLayout()
        {
            _layout = new TableLayoutPanel();
            _layout.Dock = DockStyle.Fill;
            _layout.BackColor = Color.Transparent;
            _layout.ColumnCount = 1;
            _layout.RowCount = 5;
            _layout.Padding = new Padding(20, 12, 14, 10);
            _layout.Margin = new Padding(0);
            _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            for (int i = 0; i < 5; i++)
            {
                _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }

            _studentNameLabel = new Label
            {
                Text = "Student Name",
                Font = UiTheme.SectionHeadingFont,
                ForeColor = UiTheme.TextPrimary,
                AutoSize = true,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 0, 0)
            };

            _studentIdLabel = new Label
            {
                Text = "STU-2026-000001",
                Font = UiTheme.CaptionFont,
                ForeColor = UiTheme.TextSecondary,
                AutoSize = true,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 1, 0, 6)
            };

            _programLabel = new Label
            {
                Text = "Program: BS Computer Science",
                Font = UiTheme.FieldLabelFont,
                ForeColor = UiTheme.TextPrimary,
                AutoSize = true,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 1, 0, 1)
            };

            _yearLevelLabel = new Label
            {
                Text = "Year Level: 3rd Year",
                Font = UiTheme.FieldLabelFont,
                ForeColor = UiTheme.TextPrimary,
                AutoSize = true,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 1, 0, 6)
            };

            _statusLabel = new Label
            {
                Text = "Active",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = _statusColor,
                AutoSize = true,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 0, 0)
            };

            _layout.Controls.Add(_studentNameLabel, 0, 0);
            _layout.Controls.Add(_studentIdLabel, 0, 1);
            _layout.Controls.Add(_programLabel, 0, 2);
            _layout.Controls.Add(_yearLevelLabel, 0, 3);
            _layout.Controls.Add(_statusLabel, 0, 4);

            Controls.Add(_layout);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);

            var bounds = ClientRectangle;
            var cardRect = new Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height - 1);

            UiPainting.PaintRoundedSurface(
                e.Graphics,
                cardRect,
                UiMetrics.CardCornerRadius,
                UiTheme.ElevatedSurface,
                UiTheme.BorderNeutral);

            UiPainting.PaintCardShadowLine(
                e.Graphics,
                bounds,
                UiMetrics.CardCornerRadius,
                UiTheme.InstitutionalDark);

            var accentRect = new Rectangle(bounds.X + 8, bounds.Y + 12, UiMetrics.AccentBarWidth + 1, bounds.Height - 26);
            using (var accentBrush = new SolidBrush(_statusColor))
            {
                e.Graphics.FillRectangle(accentBrush, accentRect);
            }
        }

        public void SetStatus(string status, Color statusColor)
        {
            _statusLabel.Text = status;
            _statusLabel.ForeColor = statusColor;
            _statusColor = statusColor;
            Invalidate();
        }

        public void SetActive()
        {
            SetStatus("Active", UiTheme.Success);
        }

        public void SetInactive()
        {
            SetStatus("Inactive", UiTheme.TextDisabled);
        }

        public void SetProbation()
        {
            SetStatus("On Probation", UiTheme.Warning);
        }

        public void SetSuspended()
        {
            SetStatus("Suspended", UiTheme.Error);
        }
    }
}
