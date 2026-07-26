using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using IUIS.SharedUI.Theme;

namespace IUIS.SharedUI.Controls
{
    public class RoundedInputHost : UserControl
    {
        private TextBox _textBox;
        private bool _isError = false;
        private int _radius = UiMetrics.RadiusSmall;

        public RoundedInputHost()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.OptimizedDoubleBuffer, true);
            BackColor = Color.Transparent;
            Size = new Size(200, 40);
            
            _textBox = new TextBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 10F),
                ForeColor = UiTheme.TextPrimary,
                BackColor = UiTheme.Surface,
                Multiline = false
            };
            // Adjust padding to prevent text overlap with border
            _textBox.Padding = new Padding(12, 8, 12, 8); 
            
            Controls.Add(_textBox);
        }

        [Browsable(true)]
        [Category("Behavior")]
        public string Text
        {
            get => _textBox.Text;
            set => _textBox.Text = value;
        }

        [Browsable(true)]
        [Category("Appearance")]
        public bool IsError
        {
            get => _isError;
            set { _isError = value; Invalidate(); }
        }

        [Browsable(true)]
        [Category("Appearance")]
        public int CornerRadius
        {
            get => _radius;
            set { _radius = value; Invalidate(); }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (var path = GetRoundedRectPath(ClientRectangle, _radius))
            {
                // Determine border color based on state
                Color borderColor = _isError ? UiTheme.Error : UiTheme.Border;
                
                using (var pen = new Pen(borderColor, _isError ? 2 : 1))
                {
                    g.DrawPath(pen, path);
                }
            }
        }

        private GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(rect.Location, size);
            path.AddArc(arc, 180, 90);
            arc.X = rect.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = rect.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = rect.Left;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
