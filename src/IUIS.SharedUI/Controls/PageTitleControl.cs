using System;
using System.Drawing;
using System.Windows.Forms;
using IUIS.SharedUI.Theme;

namespace IUIS.SharedUI.Controls
{
    public class PageTitleControl : UserControl
    {
        private Label _titleLabel;
        private Label _subtitleLabel;
        private Panel _actionPanel;

        public string Title
        {
            get => _titleLabel.Text;
            set => _titleLabel.Text = value;
        }

        public string Subtitle
        {
            get => _subtitleLabel.Text;
            set
            {
                _subtitleLabel.Text = value;
                _subtitleLabel.Visible = !string.IsNullOrEmpty(value);
            }
        }

        public PageTitleControl()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(800, 60);
            this.BackColor = Color.Transparent;

            _titleLabel = new Label
            {
                Font = UiTheme.PageTitleFont,
                ForeColor = UiTheme.TextPrimary,
                AutoSize = true,
                Location = new Point(0, 0)
            };

            _subtitleLabel = new Label
            {
                Font = UiTheme.BodyFont,
                ForeColor = UiTheme.TextSecondary,
                AutoSize = true,
                Location = new Point(0, 28),
                Visible = false
            };

            _actionPanel = new Panel
            {
                Dock = DockStyle.Right,
                Width = 200,
                BackColor = Color.Transparent
            };

            this.Controls.Add(_titleLabel);
            this.Controls.Add(_subtitleLabel);
            this.Controls.Add(_actionPanel);
        }

        public void AddAction(Control control)
        {
            control.Dock = DockStyle.Right;
            _actionPanel.Controls.Add(control);
        }

        public void ClearActions()
        {
            _actionPanel.Controls.Clear();
        }
    }
}
