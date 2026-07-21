using System.Drawing;
using System.Windows.Forms;

using IUIS.SharedUI.Theme;

namespace IUIS.SharedUI.Controls
{
    public sealed class DashboardPagePanel : Panel
    {
        private readonly Label _greetingLabel;
        private readonly Label _summaryLabel;
        private readonly FlowLayoutPanel _cardsPanel;

        public DashboardPagePanel()
        {
            Dock = DockStyle.Fill;
            BackColor = UiTheme.Surface;
            Padding = new Padding(UiMetrics.OuterPadding);
            AutoScroll = true;

            _greetingLabel = new Label();
            _greetingLabel.Font = UiTheme.PageTitleFont;
            _greetingLabel.ForeColor = UiTheme.TextPrimary;
            _greetingLabel.AutoSize = true;
            _greetingLabel.Location = new Point(UiMetrics.OuterPadding, UiMetrics.OuterPadding);

            _summaryLabel = new Label();
            _summaryLabel.Font = UiTheme.BodyFont;
            _summaryLabel.ForeColor = UiTheme.TextSecondary;
            _summaryLabel.AutoSize = false;
            _summaryLabel.Location = new Point(UiMetrics.OuterPadding, 52);
            _summaryLabel.Size = new Size(760, 48);
            _summaryLabel.Text =
                "Here is the current status of your university records and services.";

            _cardsPanel = new FlowLayoutPanel();
            _cardsPanel.Location = new Point(UiMetrics.OuterPadding, 112);
            _cardsPanel.Size = new Size(960, 420);
            _cardsPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _cardsPanel.WrapContents = true;
            _cardsPanel.AutoScroll = false;

            Controls.Add(_greetingLabel);
            Controls.Add(_summaryLabel);
            Controls.Add(_cardsPanel);
        }

        public void SetGreeting(string greeting, params DashboardCardModel[] cards)
        {
            _greetingLabel.Text = greeting ?? "Welcome";
            _cardsPanel.Controls.Clear();

            if (cards == null)
                return;

            foreach (var card in cards)
            {
                _cardsPanel.Controls.Add(CreateCard(card));
            }
        }

        private static Panel CreateCard(DashboardCardModel card)
        {
            var panel = new Panel();
            panel.Width = 220;
            panel.Height = 120;
            panel.BackColor = UiTheme.ElevatedSurface;
            panel.Margin = new Padding(0, 0, 16, 16);
            panel.Padding = new Padding(16);

            var title = new Label();
            title.Text = card.Title ?? string.Empty;
            title.Font = UiTheme.CaptionFont;
            title.ForeColor = UiTheme.TextSecondary;
            title.AutoSize = true;
            title.Location = new Point(16, 16);

            var value = new Label();
            value.Text = card.Value ?? "—";
            value.Font = new Font("Segoe UI", 20f, FontStyle.Bold);
            value.ForeColor = UiTheme.InstitutionalPrimary;
            value.AutoSize = true;
            value.Location = new Point(16, 40);

            var caption = new Label();
            caption.Text = card.Caption ?? string.Empty;
            caption.Font = UiTheme.CaptionFont;
            caption.ForeColor = UiTheme.TextSecondary;
            caption.AutoSize = true;
            caption.Location = new Point(16, 84);

            panel.Controls.Add(title);
            panel.Controls.Add(value);
            panel.Controls.Add(caption);
            return panel;
        }
    }

    public sealed class DashboardCardModel
    {
        public string Title { get; set; }
        public string Value { get; set; }
        public string Caption { get; set; }
    }
}
