using System.Drawing;
using System.Windows.Forms;

using IUIS.SharedUI.Theme;

namespace IUIS.SharedUI.Controls
{
    public sealed class ModulePagePanel : UserControl
    {
        private readonly Label _titleLabel;
        private readonly Label _pageKeyLabel;
        private readonly Label _descriptionLabel;

        public ModulePagePanel()
        {
            Dock = DockStyle.Fill;
            BackColor = UiTheme.Surface;
            Padding = new Padding(UiMetrics.OuterPadding);

            _titleLabel = new Label();
            _titleLabel.Font = UiTheme.PageTitleFont;
            _titleLabel.ForeColor = UiTheme.TextPrimary;
            _titleLabel.AutoSize = true;
            _titleLabel.Location = new Point(UiMetrics.OuterPadding, UiMetrics.OuterPadding);

            _pageKeyLabel = new Label();
            _pageKeyLabel.Font = UiTheme.CaptionFont;
            _pageKeyLabel.ForeColor = UiTheme.TextSecondary;
            _pageKeyLabel.AutoSize = true;
            _pageKeyLabel.Location = new Point(UiMetrics.OuterPadding, 52);

            _descriptionLabel = new Label();
            _descriptionLabel.Font = UiTheme.BodyFont;
            _descriptionLabel.ForeColor = UiTheme.TextSecondary;
            _descriptionLabel.AutoSize = false;
            _descriptionLabel.Location = new Point(UiMetrics.OuterPadding, 84);
            _descriptionLabel.Size = new Size(720, 120);
            _descriptionLabel.Text =
                "This module workspace is registered in the navigation catalog. " +
                "Detailed CRUD screens will load here as module implementation continues.";

            Controls.Add(_titleLabel);
            Controls.Add(_pageKeyLabel);
            Controls.Add(_descriptionLabel);
        }

        public void SetPage(string title, string pageKey)
        {
            _titleLabel.Text = title ?? string.Empty;
            _pageKeyLabel.Text = string.IsNullOrWhiteSpace(pageKey)
                ? string.Empty
                : "Screen reference: " + pageKey;
        }
    }
}
