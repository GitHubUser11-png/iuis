using System.Drawing;
using System.Windows.Forms;

using IUIS.SharedUI.Theme;

namespace IUIS.SharedUI.Controls
{
    public sealed class ApplicationIdentityPanel : Panel
    {
        private readonly Label _titleLabel;
        private readonly Label _subtitleLabel;
        private readonly Label _abbreviationLabel;

        public ApplicationIdentityPanel()
        {
            BackColor = Color.Transparent;
            AutoSize = true;

            _abbreviationLabel = new Label();
            _abbreviationLabel.Text = "IUIS";
            _abbreviationLabel.Font = new Font("Segoe UI", 28f, FontStyle.Bold);
            _abbreviationLabel.ForeColor = UiTheme.InstitutionalPrimary;
            _abbreviationLabel.AutoSize = true;
            _abbreviationLabel.Location = new Point(0, 0);

            _titleLabel = new Label();
            _titleLabel.Text = ApplicationIdentity.ProductName;
            _titleLabel.Font = UiTheme.ApplicationTitleFont;
            _titleLabel.ForeColor = UiTheme.TextPrimary;
            _titleLabel.AutoSize = true;
            _titleLabel.Location = new Point(0, 52);

            _subtitleLabel = new Label();
            _subtitleLabel.Text = ApplicationIdentity.CampusSubtitle;
            _subtitleLabel.Font = UiTheme.BodyFont;
            _subtitleLabel.ForeColor = UiTheme.TextSecondary;
            _subtitleLabel.AutoSize = true;
            _subtitleLabel.Location = new Point(0, 84);

            Controls.Add(_abbreviationLabel);
            Controls.Add(_titleLabel);
            Controls.Add(_subtitleLabel);
        }

        public void SetPortalLabel(string portalLabel)
        {
            if (!string.IsNullOrWhiteSpace(portalLabel))
                _subtitleLabel.Text = portalLabel;
        }
    }
}
