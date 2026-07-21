using System;
using System.Drawing;
using System.Windows.Forms;

namespace IUIS.SharedUI.Controls
{
    public partial class DetailFieldControl : UserControl
    {
        private Label _labelLabel;
        private Label _valueLabel;
        private Panel _separatorPanel;

        public DetailFieldControl()
        {
            InitializeComponent();
            SetupLayout();
        }

        public string LabelText
        {
            get => _labelLabel.Text;
            set => _labelLabel.Text = value;
        }

        public string ValueText
        {
            get => _valueLabel.Text;
            set => _valueLabel.Text = value;
        }

        public bool ShowSeparator
        {
            get => _separatorPanel.Visible;
            set => _separatorPanel.Visible = value;
        }

        private void InitializeComponent()
        {
            this.Size = new Size(300, 60);
            this.BackColor = Color.Transparent;
        }

        private void SetupLayout()
        {
            _labelLabel = new Label
            {
                Text = "Field Label",
                Font = new Font("Segoe UI", 8F),
                ForeColor = Color.FromArgb(107, 114, 128),
                Location = new Point(0, 0),
                AutoSize = true
            };

            _valueLabel = new Label
            {
                Text = "Field Value",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(17, 24, 39),
                Location = new Point(0, 18),
                AutoSize = true
            };

            _separatorPanel = new Panel
            {
                Size = new Size(300, 1),
                Location = new Point(0, 45),
                BackColor = Color.FromArgb(229, 231, 235),
                Visible = false
            };

            this.Controls.Add(_labelLabel);
            this.Controls.Add(_valueLabel);
            this.Controls.Add(_separatorPanel);
        }

        public void SetReadOnlyMode(bool readOnly)
        {
            _valueLabel.ForeColor = readOnly 
                ? Color.FromArgb(107, 114, 128) 
                : Color.FromArgb(17, 24, 39);
        }
    }
}
