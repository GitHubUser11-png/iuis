using System;
using System.Drawing;
using System.Windows.Forms;
using IUIS.SharedUI.Theme;

namespace IUIS.SharedUI.Controls
{
    /// <summary>
    /// Visual variants supported by <see cref="RoundedButton"/>.
    /// </summary>
    public enum RoundedButtonVariant
    {
        Primary,
        Secondary,
        Danger,
        Ghost
    }

    /// <summary>
    /// Owner-painted button with rounded corners, hover/pressed/disabled states
    /// and a keyboard focus ring. Pure WinForms + GDI+, no external libraries.
    /// </summary>
    public class RoundedButton : Button
    {
        private bool _isHovered;
        private bool _isPressed;
        private RoundedButtonVariant _variant = RoundedButtonVariant.Primary;
        private int _cornerRadius = 8;

        public RoundedButton()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor,
                true);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Font = UiTheme.ButtonFont;
            Cursor = Cursors.Hand;
            BackColor = Color.Transparent;
        }

        public RoundedButtonVariant Variant
        {
            get { return _variant; }
            set
            {
                _variant = value;
                Invalidate();
            }
        }

        public int CornerRadius
        {
            get { return _cornerRadius; }
            set
            {
                _cornerRadius = value < 0 ? 0 : value;
                Invalidate();
            }
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
            _isPressed = false;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            base.OnMouseDown(mevent);
            if (mevent.Button == MouseButtons.Left)
            {
                _isPressed = true;
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            base.OnMouseUp(mevent);
            _isPressed = false;
            Invalidate();
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            Invalidate();
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            var graphics = pevent.Graphics;
            var bounds = ClientRectangle;

            // Paint parent background behind rounded corners.
            Color behind = Parent != null ? Parent.BackColor : UiTheme.Surface;
            if (behind == Color.Transparent)
            {
                behind = UiTheme.Surface;
            }
            using (var backBrush = new SolidBrush(behind))
            {
                graphics.FillRectangle(backBrush, bounds);
            }

            Color fill;
            Color border;
            Color text;
            ResolveColors(behind, out fill, out border, out text);

            UiPainting.PaintRoundedSurface(graphics, bounds, _cornerRadius, fill, border);

            if (Focused && ShowFocusCues && Enabled)
            {
                Color ringColor = _variant == RoundedButtonVariant.Primary || _variant == RoundedButtonVariant.Danger
                    ? Color.White
                    : UiTheme.BorderFocus;
                UiPainting.PaintFocusRing(graphics, bounds, _cornerRadius, ringColor);
            }

            TextRenderer.DrawText(
                graphics,
                Text,
                Font,
                bounds,
                text,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        private void ResolveColors(Color behind, out Color fill, out Color border, out Color text)
        {
            if (!Enabled)
            {
                switch (_variant)
                {
                    case RoundedButtonVariant.Secondary:
                    case RoundedButtonVariant.Ghost:
                        fill = UiTheme.SurfaceSunken;
                        border = UiTheme.BorderNeutral;
                        break;
                    default:
                        fill = UiTheme.BorderNeutral;
                        border = Color.Empty;
                        break;
                }
                text = UiTheme.TextDisabled;
                return;
            }

            switch (_variant)
            {
                case RoundedButtonVariant.Secondary:
                    fill = _isPressed
                        ? UiTheme.Darken(UiTheme.ElevatedSurface, 0.06f)
                        : _isHovered ? UiTheme.SurfaceSunken : UiTheme.ElevatedSurface;
                    border = _isHovered || _isPressed ? UiTheme.InstitutionalPrimary : UiTheme.BorderNeutral;
                    text = UiTheme.TextPrimary;
                    break;

                case RoundedButtonVariant.Danger:
                    fill = _isPressed
                        ? UiTheme.Darken(UiTheme.Error, 0.12f)
                        : _isHovered ? UiTheme.Lighten(UiTheme.Error, 0.10f) : UiTheme.Error;
                    border = Color.Empty;
                    text = Color.White;
                    break;

                case RoundedButtonVariant.Ghost:
                    fill = _isPressed
                        ? UiTheme.Darken(UiTheme.PrimarySoft, 0.05f)
                        : _isHovered ? UiTheme.PrimarySoft : behind;
                    border = Color.Empty;
                    text = UiTheme.InstitutionalPrimary;
                    break;

                default: // Primary
                    fill = _isPressed
                        ? UiTheme.Darken(UiTheme.InstitutionalPrimary, 0.12f)
                        : _isHovered ? UiTheme.InstitutionalHover : UiTheme.InstitutionalPrimary;
                    border = Color.Empty;
                    text = UiTheme.TextOnPrimary;
                    break;
            }
        }
    }
}
