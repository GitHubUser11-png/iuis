using System.Drawing;
using System.Windows.Forms;

using IUIS.SharedUI.Theme;

namespace IUIS.SharedUI.Controls
{
    public enum StatusBannerKind
    {
        Information = 0,
        Success = 1,
        Warning = 2,
        Error = 3,
        Restricted = 4
    }

    public sealed class StatusBannerPanel : Panel
    {
        private readonly Label _messageLabel;

        public StatusBannerPanel()
        {
            Dock = DockStyle.Top;
            Height = 56;
            Padding = new Padding(16, 12, 16, 12);
            Visible = false;

            _messageLabel = new Label();
            _messageLabel.Dock = DockStyle.Fill;
            _messageLabel.Font = UiTheme.BodyFont;
            _messageLabel.TextAlign = ContentAlignment.MiddleLeft;
            Controls.Add(_messageLabel);
        }

        public void ShowMessage(StatusBannerKind kind, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                HideMessage();
                return;
            }

            _messageLabel.Text = message;
            switch (kind)
            {
                case StatusBannerKind.Success:
                    BackColor = Color.FromArgb(230, 246, 237);
                    _messageLabel.ForeColor = UiTheme.Success;
                    break;
                case StatusBannerKind.Warning:
                    BackColor = Color.FromArgb(255, 244, 224);
                    _messageLabel.ForeColor = UiTheme.Warning;
                    break;
                case StatusBannerKind.Error:
                    BackColor = Color.FromArgb(252, 232, 230);
                    _messageLabel.ForeColor = UiTheme.Error;
                    break;
                case StatusBannerKind.Restricted:
                    BackColor = Color.FromArgb(243, 232, 247);
                    _messageLabel.ForeColor = UiTheme.Restricted;
                    break;
                default:
                    BackColor = Color.FromArgb(229, 240, 252);
                    _messageLabel.ForeColor = UiTheme.Information;
                    break;
            }

            Visible = true;
        }

        public void HideMessage()
        {
            Visible = false;
            _messageLabel.Text = string.Empty;
        }
    }
}
