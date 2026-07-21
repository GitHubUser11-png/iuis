using System;
using System.Drawing;
using System.Windows.Forms;

using IUIS.SharedUI.Theme;

namespace IUIS.SharedUI.Controls
{
    /// <summary>
    /// Rounded dashboard metric card: accent strip, muted title,
    /// large value, caption subtitle. Layout is table-based (DPI safe).
    /// </summary>
    public partial class DashboardMetricCardControl : UserControl
    {
        private Label _titleLabel;
        private Label _valueLabel;
        private Label _subtitleLabel;
        private TableLayoutPanel _layout;
        private Color _indicatorColor = UiTheme.InstitutionalPrimary;

        public DashboardMetricCardControl()
        {
            InitializeComponent();
            SetupLayout();
        }

        public string Title
        {
            get { return _titleLabel.Text; }
            set { _titleLabel.Text = value; }
        }

        public string Value
        {
            get { return _valueLabel.Text; }
            set { _valueLabel.Text = value; }
        }

        public string Subtitle
        {
            get { return _subtitleLabel.Text; }
            set { _subtitleLabel.Text = value; }
        }

        public Color IndicatorColor
        {
            get { return _indicatorColor; }
            set
            {
                _indicatorColor = value;
                Invalidate();
            }
        }

        private void InitializeComponent()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);
            Size = new Size(220, 108);
            BackColor = UiTheme.Surface;
            BorderStyle = BorderStyle.None;
        }

        private void SetupLayout()
        {
            _layout = new TableLayoutPanel();
            _layout.Dock = DockStyle.Fill;
            _layout.BackColor = Color.Transparent;
            _layout.ColumnCount = 1;
            _layout.RowCount = 3;
            _layout.Padding = new Padding(18, 12, 14, 10);
            _layout.Margin = new Padding(0);
            _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            _titleLabel = new Label
            {
                Text = "Metric Title",
                Font = UiTheme.FieldLabelFont,
                ForeColor = UiTheme.TextSecondary,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 0),
                BackColor = Color.Transparent
            };

            _valueLabel = new Label
            {
                Text = "0",
                Font = UiTheme.MetricValueFont,
                ForeColor = UiTheme.TextPrimary,
                AutoSize = true,
                Margin = new Padding(0, 2, 0, 0),
                BackColor = Color.Transparent
            };

            _subtitleLabel = new Label
            {
                Text = "Subtitle",
                Font = UiTheme.CaptionFont,
                ForeColor = UiTheme.TextDisabled,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 0),
                BackColor = Color.Transparent
            };

            _layout.Controls.Add(_titleLabel, 0, 0);
            _layout.Controls.Add(_valueLabel, 0, 1);
            _layout.Controls.Add(_subtitleLabel, 0, 2);

            Controls.Add(_layout);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);

            var bounds = ClientRectangle;
            var cardRect = new Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height - 1);

            UiPainting.PaintRoundedSurface(
                e.Graphics,
                cardRect,
                UiMetrics.CardCornerRadius,
                UiTheme.ElevatedSurface,
                UiTheme.BorderNeutral);

            UiPainting.PaintCardShadowLine(
                e.Graphics,
                bounds,
                UiMetrics.CardCornerRadius,
                UiTheme.InstitutionalDark);

            // Rounded accent strip on the left.
            var accentRect = new Rectangle(bounds.X + 8, bounds.Y + 12, UiMetrics.AccentBarWidth + 1, bounds.Height - 26);
            using (var accentBrush = new SolidBrush(_indicatorColor))
            {
                e.Graphics.FillRectangle(accentBrush, accentRect);
            }
        }

        public void SetWarningState()
        {
            IndicatorColor = UiTheme.Warning;
            _valueLabel.ForeColor = UiTheme.Warning;
        }

        public void SetErrorState()
        {
            IndicatorColor = UiTheme.Error;
            _valueLabel.ForeColor = UiTheme.Error;
        }

        public void SetSuccessState()
        {
            IndicatorColor = UiTheme.Success;
            _valueLabel.ForeColor = UiTheme.Success;
        }

        public void SetNormalState()
        {
            IndicatorColor = UiTheme.InstitutionalPrimary;
            _valueLabel.ForeColor = UiTheme.TextPrimary;
        }
    }
}
