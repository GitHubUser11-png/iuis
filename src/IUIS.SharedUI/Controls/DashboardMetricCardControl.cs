using System;
using System.Drawing;
using System.Windows.Forms;

namespace IUIS.SharedUI.Controls
{
    public partial class DashboardMetricCardControl : UserControl
    {
        private Label _titleLabel;
        private Label _valueLabel;
        private Label _subtitleLabel;
        private Panel _indicatorPanel;

        public DashboardMetricCardControl()
        {
            InitializeComponent();
            SetupLayout();
        }

        public string Title
        {
            get => _titleLabel.Text;
            set => _titleLabel.Text = value;
        }

        public string Value
        {
            get => _valueLabel.Text;
            set => _valueLabel.Text = value;
        }

        public string Subtitle
        {
            get => _subtitleLabel.Text;
            set => _subtitleLabel.Text = value;
        }

        public Color IndicatorColor
        {
            get => _indicatorPanel.BackColor;
            set => _indicatorPanel.BackColor = value;
        }

        private void InitializeComponent()
        {
            this.Size = new Size(200, 100);
            this.BackColor = Color.White;
            this.BorderStyle = BorderStyle.FixedSingle;
        }

        private void SetupLayout()
        {
            _indicatorPanel = new Panel
            {
                Size = new Size(4, 100),
                Dock = DockStyle.Left,
                BackColor = Color.FromArgb(79, 70, 229)
            };

            _titleLabel = new Label
            {
                Text = "Metric Title",
                Font = new Font("Segoe UI", 8F),
                ForeColor = Color.FromArgb(107, 114, 128),
                Location = new Point(12, 8),
                AutoSize = true
            };

            _valueLabel = new Label
            {
                Text = "0",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                Location = new Point(12, 28),
                AutoSize = true
            };

            _subtitleLabel = new Label
            {
                Text = "Subtitle",
                Font = new Font("Segoe UI", 7F),
                ForeColor = Color.FromArgb(156, 163, 175),
                Location = new Point(12, 58),
                AutoSize = true
            };

            this.Controls.Add(_indicatorPanel);
            this.Controls.Add(_titleLabel);
            this.Controls.Add(_valueLabel);
            this.Controls.Add(_subtitleLabel);
        }

        public void SetWarningState()
        {
            _indicatorPanel.BackColor = Color.FromArgb(245, 158, 11);
            _valueLabel.ForeColor = Color.FromArgb(245, 158, 11);
        }

        public void SetErrorState()
        {
            _indicatorPanel.BackColor = Color.FromArgb(239, 68, 68);
            _valueLabel.ForeColor = Color.FromArgb(239, 68, 68);
        }

        public void SetSuccessState()
        {
            _indicatorPanel.BackColor = Color.FromArgb(34, 197, 94);
            _valueLabel.ForeColor = Color.FromArgb(34, 197, 94);
        }

        public void SetNormalState()
        {
            _indicatorPanel.BackColor = Color.FromArgb(79, 70, 229);
            _valueLabel.ForeColor = Color.FromArgb(17, 24, 39);
        }
    }
}
