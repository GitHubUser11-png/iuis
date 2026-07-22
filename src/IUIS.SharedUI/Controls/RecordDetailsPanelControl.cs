using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using IUIS.SharedUI.Theme;

namespace IUIS.SharedUI.Controls
{
    public class RecordDetailsPanelControl : UserControl
    {
        private Panel _containerPanel;
        private FlowLayoutPanel _fieldsPanel;

        public RecordDetailsPanelControl()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(400, 300);
            this.BackColor = UiTheme.ElevatedSurface;

            _containerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16)
            };

            _fieldsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = Color.Transparent
            };

            _containerPanel.Controls.Add(_fieldsPanel);
            this.Controls.Add(_containerPanel);
        }

        public void AddField(string label, string value)
        {
            var fieldPanel = new Panel
            {
                Width = _fieldsPanel.Width - 20,
                Height = 50,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 0, 8)
            };

            var labelControl = new Label
            {
                Text = label,
                Font = UiTheme.CaptionFont,
                ForeColor = UiTheme.TextSecondary,
                AutoSize = true,
                Location = new Point(0, 0)
            };

            var valueControl = new Label
            {
                Text = value ?? "-",
                Font = UiTheme.BodyFont,
                ForeColor = UiTheme.TextPrimary,
                AutoSize = true,
                Location = new Point(0, 20)
            };

            fieldPanel.Controls.Add(labelControl);
            fieldPanel.Controls.Add(valueControl);
            _fieldsPanel.Controls.Add(fieldPanel);
        }

        public void AddFields(Dictionary<string, string> fields)
        {
            foreach (var field in fields)
            {
                AddField(field.Key, field.Value);
            }
        }

        public void Clear()
        {
            _fieldsPanel.Controls.Clear();
        }
    }
}
