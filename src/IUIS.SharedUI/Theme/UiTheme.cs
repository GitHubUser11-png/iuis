using System;
using System.Drawing;
using System.Windows.Forms;

using IUIS.SharedUI.Controls;

namespace IUIS.SharedUI.Theme
{
    public static class UiTheme
    {
        public static readonly Color InstitutionalPrimary = Color.FromArgb(28, 64, 97);
        public static readonly Color InstitutionalDark = Color.FromArgb(17, 42, 63);
        public static readonly Color Surface = Color.FromArgb(245, 247, 248);
        public static readonly Color ElevatedSurface = Color.White;
        public static readonly Color SubtleSurface = Color.FromArgb(235, 240, 242);
        public static readonly Color BorderNeutral = Color.FromArgb(205, 214, 219);
        public static readonly Color TextPrimary = Color.FromArgb(27, 39, 48);
        public static readonly Color TextSecondary = Color.FromArgb(84, 99, 109);
        public static readonly Color Accent = Color.FromArgb(20, 121, 111);
        public static readonly Color Selection = Color.FromArgb(218, 232, 239);
        public static readonly Color SurfaceSunken = Color.FromArgb(235, 240, 242);
        public static readonly Color PrimarySoft = Color.FromArgb(225, 235, 241);
        public static readonly Color InstitutionalHover = Color.FromArgb(38, 83, 120);
        public static readonly Color BorderFocus = InstitutionalPrimary;
        public static readonly Color TextDisabled = Color.FromArgb(132, 143, 150);
        public static readonly Color TextOnPrimary = Color.White;
        public static readonly Color Success = Accent;
        public static readonly Color Information = InstitutionalPrimary;
        public static readonly Color Warning = Color.FromArgb(139, 86, 17);
        public static readonly Color Error = Color.FromArgb(174, 46, 46);
        public static readonly Color Restricted = Error;

        public static readonly Font BodyFont = new Font("Segoe UI", 10f, FontStyle.Regular);
        public static readonly Font CaptionFont = new Font("Segoe UI", 8.5f, FontStyle.Regular);
        public static readonly Font FieldLabelFont = new Font("Segoe UI", 9f, FontStyle.Bold);
        public static readonly Font ButtonFont = new Font("Segoe UI", 9.5f, FontStyle.Bold);
        public static readonly Font NavigationSelectedFont = new Font("Segoe UI", 10f, FontStyle.Bold);
        public static readonly Font SectionHeadingFont = new Font("Segoe UI", 11f, FontStyle.Bold);
        public static readonly Font PageTitleFont = new Font("Segoe UI", 20f, FontStyle.Bold);
        public static readonly Font ApplicationTitleFont = new Font("Segoe UI", 15f, FontStyle.Bold);
        public static readonly Font MetricValueFont = new Font("Segoe UI", 22f, FontStyle.Bold);

        public static void ApplyBaseFormStyle(Form form)
        {
            form.Font = BodyFont;
            form.BackColor = Surface;
            form.ForeColor = TextPrimary;
            form.AutoScaleMode = AutoScaleMode.Dpi;
            form.KeyPreview = true;
        }

        public static void ApplyButtonInteractionStyle(Button button, Color hoverColor, Color pressedColor)
        {
            button.UseVisualStyleBackColor = false;
            button.FlatAppearance.MouseOverBackColor = hoverColor;
            button.FlatAppearance.MouseDownBackColor = pressedColor;
            button.Cursor = Cursors.Hand;
        }

        public static Button CreatePrimaryButton(string text, int width, int height)
        {
            var button = new RoundedButton
            {
                Text = text,
                Width = width,
                Height = height,
                BackColor = InstitutionalPrimary,
                ForeColor = Color.White,
                Font = ButtonFont,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderSize = 0;
            ApplyButtonInteractionStyle(button, Accent, InstitutionalDark);
            return button;
        }

        public static Button CreateSecondaryButton(string text, int width, int height)
        {
            var button = new RoundedButton
            {
                Text = text,
                Width = width,
                Height = height,
                Variant = RoundedButtonVariant.Secondary,
                BackColor = ElevatedSurface,
                ForeColor = TextPrimary,
                Font = ButtonFont,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderColor = BorderNeutral;
            button.FlatAppearance.BorderSize = 1;
            ApplyButtonInteractionStyle(button, SubtleSurface, BorderNeutral);
            return button;
        }

        public static Color Lighten(Color color, float amount)
        {
            amount = Math.Max(0f, Math.Min(1f, amount));
            return Color.FromArgb(
                color.A,
                color.R + (int)((255 - color.R) * amount),
                color.G + (int)((255 - color.G) * amount),
                color.B + (int)((255 - color.B) * amount));
        }

        public static Color Darken(Color color, float amount)
        {
            amount = Math.Max(0f, Math.Min(1f, amount));
            return Color.FromArgb(
                color.A,
                (int)(color.R * (1f - amount)),
                (int)(color.G * (1f - amount)),
                (int)(color.B * (1f - amount)));
        }

        public static Button CreateTextAction(string text)
        {
            var button = new Button
            {
                Text = text,
                AutoSize = true,
                Height = 36,
                BackColor = ElevatedSurface,
                ForeColor = InstitutionalPrimary,
                Font = BodyFont,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = Padding.Empty
            };
            button.FlatAppearance.BorderSize = 0;
            ApplyButtonInteractionStyle(button, SubtleSurface, BorderNeutral);
            return button;
        }
    }
}
