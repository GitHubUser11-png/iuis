using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace IUIS.SharedUI.Theme
{
    /// <summary>
    /// Central GDI+ helpers for rounded, anti-aliased custom painting.
    /// Used by every custom-painted control in the design system.
    /// </summary>
    public static class UiPainting
    {
        /// <summary>
        /// Builds a rounded-rectangle path. Caller is responsible for disposing the path.
        /// </summary>
        public static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            var path = new GraphicsPath();
            if (radius <= 0 || bounds.Width <= 0 || bounds.Height <= 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            int diameter = Math.Min(radius * 2, Math.Min(bounds.Width, bounds.Height));
            var arc = new Rectangle(bounds.X, bounds.Y, diameter, diameter);

            path.AddArc(arc, 180f, 90f);
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270f, 90f);
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0f, 90f);
            arc.X = bounds.Left;
            path.AddArc(arc, 90f, 90f);
            path.CloseFigure();
            return path;
        }

        /// <summary>
        /// Paints a filled rounded surface with an optional 1px border.
        /// Pass Color.Empty for either color to skip that part.
        /// </summary>
        public static void PaintRoundedSurface(Graphics graphics, Rectangle bounds, int radius, Color fill, Color border)
        {
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            var previousMode = graphics.SmoothingMode;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Deflate so the 1px border stroke stays fully inside the bounds.
            var rect = new Rectangle(bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
            using (var path = RoundedRect(rect, radius))
            {
                if (!fill.IsEmpty)
                {
                    using (var brush = new SolidBrush(fill))
                    {
                        graphics.FillPath(brush, path);
                    }
                }

                if (!border.IsEmpty)
                {
                    using (var pen = new Pen(border, 1f))
                    {
                        graphics.DrawPath(pen, path);
                    }
                }
            }

            graphics.SmoothingMode = previousMode;
        }

        /// <summary>
        /// Paints a keyboard focus ring just inside the given bounds.
        /// </summary>
        public static void PaintFocusRing(Graphics graphics, Rectangle bounds, int radius, Color color)
        {
            if (bounds.Width <= 6 || bounds.Height <= 6)
            {
                return;
            }

            var previousMode = graphics.SmoothingMode;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new Rectangle(bounds.X + 2, bounds.Y + 2, bounds.Width - 5, bounds.Height - 5);
            using (var path = RoundedRect(rect, Math.Max(radius - 2, 2)))
            using (var pen = new Pen(Color.FromArgb(160, color), 1.6f))
            {
                pen.DashStyle = DashStyle.Solid;
                graphics.DrawPath(pen, path);
            }

            graphics.SmoothingMode = previousMode;
        }

        /// <summary>
        /// Clips a control to a rounded region. Use for controls that cannot be
        /// fully owner-drawn (e.g. hosting panels). Re-apply on resize.
        /// </summary>
        public static void ApplyRoundedRegion(Control control, int radius)
        {
            if (control == null || control.Width <= 0 || control.Height <= 0)
            {
                return;
            }

            var bounds = new Rectangle(0, 0, control.Width, control.Height);
            using (var path = RoundedRect(bounds, radius))
            {
                var oldRegion = control.Region;
                control.Region = new Region(path);
                if (oldRegion != null)
                {
                    oldRegion.Dispose();
                }
            }
        }

        /// <summary>
        /// Draws a subtle 1px "shadow" line under a card to fake elevation
        /// without layered windows.
        /// </summary>
        public static void PaintCardShadowLine(Graphics graphics, Rectangle cardBounds, int radius, Color shadowColor)
        {
            if (cardBounds.Width <= 0 || cardBounds.Height <= 0)
            {
                return;
            }

            var previousMode = graphics.SmoothingMode;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (var pen = new Pen(Color.FromArgb(46, shadowColor), 1f))
            {
                int inset = Math.Max(radius, 4);
                graphics.DrawLine(
                    pen,
                    cardBounds.Left + inset,
                    cardBounds.Bottom - 1,
                    cardBounds.Right - inset,
                    cardBounds.Bottom - 1);
            }

            graphics.SmoothingMode = previousMode;
        }
    }
}
