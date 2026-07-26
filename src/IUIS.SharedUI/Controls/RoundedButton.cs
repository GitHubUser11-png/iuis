using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using IUIS.SharedUI.Theme;

namespace IUIS.SharedUI.Controls
{
    public class RoundedButton : Button
    {
        private int _radius = UiMetrics.RadiusMedium;
        private bool _isHovered = false;

        public RoundedButton()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor | 
                     ControlStyles.OptimizedDoubleBuffer | 
                     ControlStyles.AllPaintingInWmPaint | 
                     ControlStyles.UserPaint, true);
            BackColor = Color.Transparent;
            FlatStyle = FlatStyle.Flat;
            Cursor = Cursors.Hand;
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            Size = new Size(120, 40);
        }

        [Browsable(true)]
        [Category("Appearance")]
        public int CornerRadius
        {
            get => _radius;
            set { _radius = value; Invalidate(); }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _isHovered = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isHovered = false;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            base.OnPaint(pevent);
            var g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (var path = GetRoundedRectPath(ClientRectangle, _radius))
            {
                // Draw Background
                Color backColor = _isHovered ? UiTheme.PrimaryDark : UiTheme.Primary;
                using (var brush = new SolidBrush(backColor))
                {
                    g.FillPath(brush, path);
                }

                // Draw Border
                using (var pen = new Pen(UiTheme.Border, 1))
                {
                    g.DrawPath(pen, path);
                }

                // Draw Text
                StringFormat sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                using (var brush = new SolidBrush(ForeColor))
                {
                    g.DrawString(Text, Font, brush, ClientRectangle, sf);
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
