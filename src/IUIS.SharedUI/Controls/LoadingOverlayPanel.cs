using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using IUIS.SharedUI.Theme;

namespace IUIS.SharedUI.Controls
{
    public class LoadingOverlayPanel : Panel
    {
        private bool _isLoading = false;
        private int _rotationAngle = 0;
        private Timer _spinTimer;

        public LoadingOverlayPanel()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.FromArgb(150, Color.Black); // Semi-transparent
            Visible = false;
            BringToFront(); // Ensure Z-order is correct
            
            SetupSpinner();
        }

        private void SetupSpinner()
        {
            _spinTimer = new Timer { Interval = 30 };
            _spinTimer.Tick += (s, e) =>
            {
                _rotationAngle = (_rotationAngle + 10) % 360;
                Invalidate();
            };
        }

        public void ShowOverlay()
        {
            _isLoading = true;
            Visible = true;
            BringToFront(); // Force to top
            Capture = true; // Block mouse clicks to underlying controls
            _spinTimer.Start();
        }

        public void HideOverlay()
        {
            _isLoading = false;
            Visible = false;
            Capture = false;
            _spinTimer.Stop();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (!_isLoading) return;

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TranslateTransform(Width / 2, Height / 2);
            g.RotateTransform(_rotationAngle);

            // Draw Spinner Arc
            using (var pen = new Pen(UiTheme.Primary, 4))
            {
                int size = 60;
                Rectangle rect = new Rectangle(-size/2, -size/2, size, size);
                g.DrawArc(pen, rect, 0, 270);
            }

            g.ResetTransform();
            
            // Draw "Loading..." text
            string text = "Loading...";
            using (var font = new Font("Segoe UI", 12F, FontStyle.Regular))
            using (var brush = new SolidBrush(Color.White))
            {
                SizeF textSize = g.MeasureString(text, font);
                PointF location = new PointF((Width - textSize.Width) / 2, (Height / 2) + 40);
                g.DrawString(text, font, brush, location);
            }
        }
    }
}
