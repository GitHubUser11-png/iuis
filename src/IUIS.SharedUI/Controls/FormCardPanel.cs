using System;
using System.Drawing;
using System.Windows.Forms;

using IUIS.SharedUI.Theme;

namespace IUIS.SharedUI.Controls
{
    /// <summary>
    /// Rounded elevated card used to host forms and content sections.
    /// </summary>
    public sealed class FormCardPanel : Panel
    {
        public FormCardPanel()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);
            BackColor = UiTheme.ElevatedSurface;
            Padding = new Padding(32, 28, 32, 28);
            Width = UiMetrics.FormCardWidth;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            UiPainting.ApplyRoundedRegion(this, UiMetrics.CardCornerRadius);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            UiPainting.PaintRoundedSurface(
                e.Graphics,
                ClientRectangle,
                UiMetrics.CardCornerRadius,
                Color.Empty,
                UiTheme.BorderNeutral);
        }
    }
}
