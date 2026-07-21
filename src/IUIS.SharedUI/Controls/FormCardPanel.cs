using System.Drawing;
using System.Windows.Forms;

using IUIS.SharedUI.Theme;

namespace IUIS.SharedUI.Controls
{
    public sealed class FormCardPanel : Panel
    {
        public FormCardPanel()
        {
            BackColor = UiTheme.ElevatedSurface;
            Padding = new Padding(32, 28, 32, 28);
            Width = UiMetrics.FormCardWidth;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (var pen = new Pen(UiTheme.BorderNeutral))
            {
                var rect = ClientRectangle;
                rect.Width -= 1;
                rect.Height -= 1;
                e.Graphics.DrawRectangle(pen, rect);
            }
        }
    }
}
