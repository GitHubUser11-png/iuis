using System;
using System.Drawing;
using System.Windows.Forms;
using IUIS.SharedUI.Controls;

namespace IUIS.SharedUI.Theme
{
    public static class UiTheme
    {
        // ---- Brand ramp (institutional navy) ----
        public static readonly Color InstitutionalPrimary = Color.FromArgb(27, 58, 92);
        public static readonly Color InstitutionalDark = Color.FromArgb(15, 36, 56);
        public static readonly Color InstitutionalDeeper = Color.FromArgb(10, 28, 46);
        public static readonly Color InstitutionalHover = Color.FromArgb(38, 74, 114);
        public static readonly Color InstitutionalAccent = Color.FromArgb(198, 146, 20);
        public static readonly Color PrimarySoft = Color.FromArgb(227, 236, 245);

        // ---- Neutrals ----
        public static readonly Color Surface = Color.FromArgb(244, 246, 248);
        public static readonly Color SurfaceSunken = Color.FromArgb(236, 239, 243);
        public static readonly Color ElevatedSurface = Color.White;
        public static readonly Color BorderNeutral = Color.FromArgb(208, 214, 221);
        public static readonly Color BorderFocus = Color.FromArgb(46, 89, 132);
        public static readonly Color TextPrimary = Color.FromArgb(26, 32, 44);
        public static readonly Color TextSecondary = Color.FromArgb(90, 100, 112);
        public static readonly Color TextDisabled = Color.FromArgb(150, 158, 168);
        public static readonly Color TextOnPrimary = Color.White;
        public static readonly Color TextOnDeepNavy = Color.FromArgb(196, 210, 224);

        // ---- Semantic ----
        public static readonly Color Success = Color.FromArgb(30, 125, 78);
        public static readonly Color Warning = Color.FromArgb(184, 110, 0);
        public static readonly Color Error = Color.FromArgb(179, 38, 30);
        public static readonly Color Information = Color.FromArgb(26, 95, 180);
        public static readonly Color Restricted = Color.FromArgb(107, 45, 131);

        // ---- Semantic soft tints (banner / badge backgrounds) ----
        public static readonly Color SuccessSoft = Color.FromArgb(223, 240, 231);
        public static readonly Color WarningSoft = Color.FromArgb(250, 238, 217);
        public static readonly Color ErrorSoft = Color.FromArgb(249, 226, 224);
        public static readonly Color InfoSoft = Color.FromArgb(222, 235, 248);
        public static readonly Color RestrictedSoft = Color.FromArgb(238, 226, 244);

        // ---- Typography ----
        public static readonly Font ApplicationTitleFont = new Font("Segoe UI", 18f, FontStyle.Bold);
        public static readonly Font PageTitleFont = new Font("Segoe UI", 16f, FontStyle.Bold);
        public static readonly Font SectionHeadingFont = new Font("Segoe UI", 11f, FontStyle.Bold);
        public static readonly Font BodyFont = new Font("Segoe UI", 10f, FontStyle.Regular);
        public static readonly Font BodyBoldFont = new Font("Segoe UI", 10f, FontStyle.Bold);
        public static readonly Font FieldLabelFont = new Font("Segoe UI", 9f, FontStyle.Regular);
        public static readonly Font CaptionFont = new Font("Segoe UI", 8.25f, FontStyle.Regular);
        public static readonly Font ButtonFont = new Font("Segoe UI", 9.75f, FontStyle.Regular);
        public static readonly Font SmallButtonFont = new Font("Segoe UI", 9f, FontStyle.Regular);
        public static readonly Font MetricValueFont = new Font("Segoe UI", 20f, FontStyle.Bold);

        // ---- Color state helpers ----

        /// <summary>Blends the color toward white by the given factor (0..1).</summary>
        public static Color Lighten(Color color, float factor)
        {
            factor = Clamp01(factor);
            int r = color.R + (int)((255 - color.R) * factor);
            int g = color.G + (int)((255 - color.G) * factor);
            int b = color.B + (int)((255 - color.B) * factor);
            return Color.FromArgb(color.A, r, g, b);
        }

        /// <summary>Blends the color toward black by the given factor (0..1).</summary>
        public static Color Darken(Color color, float factor)
        {
            factor = Clamp01(factor);
            int r = (int)(color.R * (1f - factor));
            int g = (int)(color.G * (1f - factor));
            int b = (int)(color.B * (1f - factor));
            return Color.FromArgb(color.A, r, g, b);
        }

        private static float Clamp01(float value)
        {
            if (value < 0f) return 0f;
            if (value > 1f) return 1f;
            return value;
        }

        public static void ApplyBaseFormStyle(Form form)
        {
            form.Font = BodyFont;
            form.BackColor = Surface;
            form.ForeColor = TextPrimary;
        }

        // ---- Button factories (all return owner-painted RoundedButton) ----

        public static Button CreatePrimaryButton(string text, int width, int height)
        {
            return CreateRoundedButton(text, width, height, RoundedButtonVariant.Primary);
        }

        public static Button CreateSecondaryButton(string text, int width, int height)
        {
            return CreateRoundedButton(text, width, height, RoundedButtonVariant.Secondary);
        }

        public static Button CreateDangerButton(string text, int width, int height)
        {
            return CreateRoundedButton(text, width, height, RoundedButtonVariant.Danger);
        }

        public static Button CreateGhostButton(string text, int width, int height)
        {
            return CreateRoundedButton(text, width, height, RoundedButtonVariant.Ghost);
        }

        private static Button CreateRoundedButton(string text, int width, int height, RoundedButtonVariant variant)
        {
            var button = new RoundedButton();
            button.Text = text;
            button.Width = width;
            button.Height = height;
            button.Variant = variant;
            button.CornerRadius = UiMetrics.CornerRadius;
            return button;
        }

        public static LinkLabel CreateTextAction(string text)
        {
            var link = new LinkLabel();
            link.Text = text;
            link.AutoSize = true;
            link.LinkColor = InstitutionalPrimary;
            link.ActiveLinkColor = InstitutionalHover;
            link.VisitedLinkColor = InstitutionalPrimary;
            link.LinkBehavior = LinkBehavior.HoverUnderline;
            link.Font = BodyFont;
            link.Cursor = Cursors.Hand;
            return link;
        }
    }
}
