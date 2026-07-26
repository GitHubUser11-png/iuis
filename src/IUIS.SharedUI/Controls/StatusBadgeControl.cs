using System;
using System.Drawing;
using System.Windows.Forms;
using IUIS.SharedUI.Theme;

namespace IUIS.SharedUI.Controls
{
    public class StatusBadgeControl : Label
    {
        public enum BadgeStatus { Success, Warning, Error, Info }

        private BadgeStatus _status = BadgeStatus.Info;

        public StatusBadgeControl()
        {
            AutoSize = true;
            Padding = new Padding(8, 4, 8, 4);
            Font = new Font("Segoe UI", 8.5F, FontStyle.SemiBold);
            BackColor = UiTheme.InfoLight;
            ForeColor = UiTheme.Info;
            BorderStyle = BorderStyle.None;
            TextAlign = ContentAlignment.MiddleCenter;
        }

        public BadgeStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                UpdateColors();
                Invalidate();
            }
        }

        private void UpdateColors()
        {
            switch (_status)
            {
                case BadgeStatus.Success:
                    BackColor = UiTheme.SuccessLight;
                    ForeColor = UiTheme.Success;
                    break;
                case BadgeStatus.Warning:
                    BackColor = UiTheme.WarningLight;
                    ForeColor = UiTheme.Warning;
                    break;
                case BadgeStatus.Error:
                    BackColor = UiTheme.ErrorLight;
                    ForeColor = UiTheme.Error;
                    break;
                case BadgeStatus.Info:
                default:
                    BackColor = UiTheme.InfoLight;
                    ForeColor = UiTheme.Info;
                    break;
            }
        }
        
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            // Optional: Add rounded corners here if desired
        }
    }
}
