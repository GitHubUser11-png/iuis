using System;
using System.Drawing;
using System.Windows.Forms;
using IUIS.SharedUI.Theme;

namespace IUIS.SharedUI.Controls
{
    public class StatusBadgeControl : UserControl
    {
        private Label _badgeLabel;
        private Panel _badgePanel;

        public enum StatusType
        {
            Active,
            Inactive,
            Pending,
            Completed,
            Error,
            Warning,
            Success,
            Info
        }

        private StatusType _status = StatusType.Info;

        public StatusType Status
        {
            get => _status;
            set
            {
                _status = value;
                UpdateAppearance();
            }
        }

        public new string Text
        {
            get => _badgeLabel.Text;
            set => _badgeLabel.Text = value;
        }

        public StatusBadgeControl()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(100, 28);
            this.BackColor = Color.Transparent;

            _badgePanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UiTheme.Information
            };

            _badgeLabel = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Status",
                Font = UiTheme.CaptionFont,
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            _badgePanel.Controls.Add(_badgeLabel);
            this.Controls.Add(_badgePanel);
        }

        private void UpdateAppearance()
        {
            switch (_status)
            {
                case StatusType.Active:
                    _badgePanel.BackColor = UiTheme.Success;
                    break;
                case StatusType.Inactive:
                    _badgePanel.BackColor = UiTheme.TextSecondary;
                    break;
                case StatusType.Pending:
                    _badgePanel.BackColor = UiTheme.Warning;
                    break;
                case StatusType.Completed:
                    _badgePanel.BackColor = UiTheme.InstitutionalPrimary;
                    break;
                case StatusType.Error:
                    _badgePanel.BackColor = UiTheme.Error;
                    break;
                case StatusType.Warning:
                    _badgePanel.BackColor = UiTheme.Warning;
                    break;
                case StatusType.Success:
                    _badgePanel.BackColor = UiTheme.Success;
                    break;
                case StatusType.Info:
                default:
                    _badgePanel.BackColor = UiTheme.Information;
                    break;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (var path = new System.Drawing.Drawing2D.GraphicsPath())
            {
                var radius = 14;
                var rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
                path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
                path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
                path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
                path.CloseAllFigures();
                this.Region = new Region(path);
            }
        }
    }
}
