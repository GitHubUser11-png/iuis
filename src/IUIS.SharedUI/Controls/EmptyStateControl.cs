using System;
using System.Drawing;
using System.Windows.Forms;
using IUIS.SharedUI.Theme;

namespace IUIS.SharedUI.Controls
{
    public class EmptyStateControl : UserControl
    {
        private Label _iconLabel;
        private Label _titleLabel;
        private Label _descriptionLabel;
        private Button _actionButton;
        private Panel _contentPanel;

        public string Title
        {
            get => _titleLabel.Text;
            set => _titleLabel.Text = value;
        }

        public string Description
        {
            get => _descriptionLabel.Text;
            set => _descriptionLabel.Text = value;
        }

        public string Icon
        {
            get => _iconLabel.Text;
            set => _iconLabel.Text = value;
        }

        public string ActionButtonText
        {
            get => _actionButton.Text;
            set
            {
                _actionButton.Text = value;
                _actionButton.Visible = !string.IsNullOrEmpty(value);
            }
        }

        public event EventHandler ActionClicked;

        public EmptyStateControl()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(400, 300);
            this.BackColor = Color.Transparent;

            _contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };

            _iconLabel = new Label
            {
                Text = "📭",
                Font = new Font("Segoe UI", 48F),
                ForeColor = UiTheme.TextSecondary,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter
            };

            _titleLabel = new Label
            {
                Text = "No Data Found",
                Font = UiTheme.SectionHeadingFont,
                ForeColor = UiTheme.TextPrimary,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter
            };

            _descriptionLabel = new Label
            {
                Text = "There are no items to display at this time.",
                Font = UiTheme.BodyFont,
                ForeColor = UiTheme.TextSecondary,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter,
                MaximumSize = new Size(350, 0)
            };

            _actionButton = UiTheme.CreatePrimaryButton("Add New", 150, UiMetrics.StandardButtonHeight);
            _actionButton.Visible = false;
            _actionButton.Click += (s, e) => ActionClicked?.Invoke(this, EventArgs.Empty);

            LayoutContent();
            _contentPanel.Controls.Add(_iconLabel);
            _contentPanel.Controls.Add(_titleLabel);
            _contentPanel.Controls.Add(_descriptionLabel);
            _contentPanel.Controls.Add(_actionButton);
            this.Controls.Add(_contentPanel);
        }

        private void LayoutContent()
        {
            var centerX = this.Width / 2;
            var centerY = this.Height / 2;

            _iconLabel.Location = new Point(centerX - _iconLabel.Width / 2, centerY - 80);
            _titleLabel.Location = new Point(centerX - _titleLabel.Width / 2, centerY - 20);
            _descriptionLabel.Location = new Point(centerX - _descriptionLabel.Width / 2, centerY + 10);
            _actionButton.Location = new Point(centerX - _actionButton.Width / 2, centerY + 60);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            LayoutContent();
        }
    }
}
