using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using IUIS.SharedUI.Theme;

namespace IUIS.SharedUI.Controls
{
    public class ValidationSummaryControl : UserControl
    {
        private Panel _containerPanel;
        private Label _titleLabel;
        private FlowLayoutPanel _errorsPanel;

        public ValidationSummaryControl()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(400, 200);
            this.BackColor = Color.Transparent;
            this.Visible = false;

            _containerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(254, 242, 242),
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(16)
            };

            _titleLabel = new Label
            {
                Text = "⚠️ Please fix the following errors:",
                Font = UiTheme.FieldLabelFont,
                ForeColor = UiTheme.Error,
                AutoSize = true
            };

            _errorsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = Color.Transparent
            };

            _containerPanel.Controls.Add(_titleLabel);
            _containerPanel.Controls.Add(_errorsPanel);
            this.Controls.Add(_containerPanel);
        }

        public void ShowErrors(IEnumerable<string> errors)
        {
            _errorsPanel.Controls.Clear();

            foreach (var error in errors)
            {
                var errorLabel = new Label
                {
                    Text = $"• {error}",
                    Font = UiTheme.BodyFont,
                    ForeColor = UiTheme.Error,
                    AutoSize = true,
                    Margin = new Padding(0, 4, 0, 4)
                };
                _errorsPanel.Controls.Add(errorLabel);
            }

            this.Visible = errorsPanelHasItems();
            _titleLabel.Visible = errorsPanelHasItems();
        }

        public void Clear()
        {
            _errorsPanel.Controls.Clear();
            this.Visible = false;
        }

        private bool errorsPanelHasItems()
        {
            return _errorsPanel.Controls.Count > 0;
        }
    }
}
