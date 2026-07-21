using System.Drawing;
using System.Windows.Forms;

using IUIS.SharedUI.Theme;

namespace IUIS.SharedUI.Controls
{
    public sealed class ApplicationHeaderBarPanel : Panel
    {
        private readonly Label _pageTitleLabel;
        private readonly Label _contextLabel;
        private readonly Label _userLabel;

        public ApplicationHeaderBarPanel()
        {
            Height = UiMetrics.HeaderHeight;
            Dock = DockStyle.Top;
            BackColor = UiTheme.ElevatedSurface;
            Padding = new Padding(UiMetrics.ContentPadding, 12, UiMetrics.ContentPadding, 12);

            _pageTitleLabel = new Label();
            _pageTitleLabel.Font = UiTheme.PageTitleFont;
            _pageTitleLabel.ForeColor = UiTheme.TextPrimary;
            _pageTitleLabel.AutoSize = true;
            _pageTitleLabel.Location = new Point(UiMetrics.ContentPadding, 8);

            _contextLabel = new Label();
            _contextLabel.Font = UiTheme.CaptionFont;
            _contextLabel.ForeColor = UiTheme.TextSecondary;
            _contextLabel.AutoSize = true;
            _contextLabel.Location = new Point(UiMetrics.ContentPadding, 38);

            _userLabel = new Label();
            _userLabel.Font = UiTheme.BodyFont;
            _userLabel.ForeColor = UiTheme.TextSecondary;
            _userLabel.AutoSize = true;
            _userLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _userLabel.Location = new Point(Width - 240, 18);

            Controls.Add(_pageTitleLabel);
            Controls.Add(_contextLabel);
            Controls.Add(_userLabel);

            Resize += delegate
            {
                _userLabel.Left = Width - _userLabel.Width - UiMetrics.ContentPadding;
            };
        }

        public void SetPage(string pageTitle, string contextText)
        {
            _pageTitleLabel.Text = pageTitle ?? string.Empty;
            _contextLabel.Text = contextText ?? string.Empty;
        }

        public void SetUserDisplay(string userDisplay)
        {
            _userLabel.Text = userDisplay ?? string.Empty;
            _userLabel.Left = Width - _userLabel.Width - UiMetrics.ContentPadding;
        }
    }
}
