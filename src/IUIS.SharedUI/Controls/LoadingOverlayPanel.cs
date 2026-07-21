using System.Drawing;
using System.Windows.Forms;

using IUIS.SharedUI.Theme;

namespace IUIS.SharedUI.Controls
{
    public sealed class LoadingOverlayPanel : Panel
    {
        private readonly Label _messageLabel;
        private readonly ProgressBar _progressBar;

        public LoadingOverlayPanel()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.FromArgb(180, 244, 246, 248);
            Visible = false;

            var center = new Panel();
            center.Width = 360;
            center.Height = 96;
            center.BackColor = UiTheme.ElevatedSurface;
            center.Anchor = AnchorStyles.None;

            _messageLabel = new Label();
            _messageLabel.Text = "Working…";
            _messageLabel.Font = UiTheme.BodyFont;
            _messageLabel.ForeColor = UiTheme.TextPrimary;
            _messageLabel.AutoSize = false;
            _messageLabel.TextAlign = ContentAlignment.MiddleCenter;
            _messageLabel.Dock = DockStyle.Top;
            _messageLabel.Height = 40;

            _progressBar = new ProgressBar();
            _progressBar.Style = ProgressBarStyle.Marquee;
            _progressBar.MarqueeAnimationSpeed = 30;
            _progressBar.Dock = DockStyle.Bottom;
            _progressBar.Height = 18;

            center.Controls.Add(_messageLabel);
            center.Controls.Add(_progressBar);
            Controls.Add(center);

            Resize += delegate
            {
                center.Left = (Width - center.Width) / 2;
                center.Top = (Height - center.Height) / 2;
            };
        }

        public void ShowOverlay(string message)
        {
            _messageLabel.Text = string.IsNullOrWhiteSpace(message) ? "Working…" : message;
            Visible = true;
            BringToFront();
        }

        public void HideOverlay()
        {
            Visible = false;
        }
    }
}
