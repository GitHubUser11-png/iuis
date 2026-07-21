using System.Drawing;
using System.Windows.Forms;

namespace IUIS.SharedUI.Theme
{
    public static class UiTheme
    {
        // Semantic palette: institutional navy, warm canvas, neutral ink, teal accent, and red for errors.
        public static readonly Color InstitutionalPrimary = Color.FromArgb(24, 54, 83);
        public static readonly Color InstitutionalDark = Color.FromArgb(14, 35, 55);
        public static readonly Color Surface = Color.FromArgb(246, 247, 245);
        public static readonly Color ElevatedSurface = Color.FromArgb(255, 255, 255);
        public static readonly Color SubtleSurface = Color.FromArgb(236, 240, 239);
        public static readonly Color BorderNeutral = Color.FromArgb(207, 215, 215);
        public static readonly Color TextPrimary = Color.FromArgb(27, 39, 48);
        public static readonly Color TextSecondary = Color.FromArgb(85, 99, 108);
        public static readonly Color Accent = Color.FromArgb(18, 121, 111);
        public static readonly Color AccentSoft = Color.FromArgb(220, 240, 236);
        public static readonly Color Success = Accent;
        public static readonly Color Warning = Color.FromArgb(148, 92, 18);
        public static readonly Color Error = Color.FromArgb(177, 47, 47);
        public static readonly Color Information = Color.FromArgb(35, 93, 145);
        public static readonly Color Restricted = Error;
        public static readonly Color Selection = Color.FromArgb(218, 232, 239);
        public static readonly Color Focus = Accent;

        public static readonly Font ApplicationTitleFont = new Font("Segoe UI", 18f, FontStyle.Bold);
        public static readonly Font PageTitleFont = new Font("Segoe UI", 16f, FontStyle.Bold);
        public static readonly Font SectionHeadingFont = new Font("Segoe UI", 11f, FontStyle.Bold);
        public static readonly Font BodyFont = new Font("Segoe UI", 10f, FontStyle.Regular);
        public static readonly Font FieldLabelFont = new Font("Segoe UI", 9f, FontStyle.Regular);
        public static readonly Font CaptionFont = new Font("Segoe UI", 8.25f, FontStyle.Regular);
        public static readonly Font ButtonFont = new Font("Segoe UI", 9.75f, FontStyle.Regular);
        public static readonly Font NavigationSelectedFont = new Font("Segoe UI", 10f, FontStyle.Bold);

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
            button.TabStop = true;
        }

        public static Button CreatePrimaryButton(string text, int width, int height)
        {
            var button = new Button();
            button.Text = text;
            button.Width = width;
            button.Height = height;
            button.FlatStyle = FlatStyle.Flat;
            button.BackColor = InstitutionalPrimary;
            button.ForeColor = Color.White;
            button.Font = ButtonFont;
            button.FlatAppearance.BorderSize = 0;
            button.Cursor = Cursors.Hand;
            ApplyButtonInteractionStyle(button, Accent, InstitutionalDark);
            return button;
        }

        public static Button CreateSecondaryButton(string text, int width, int height)
        {
            var button = new Button();
            button.Text = text;
            button.Width = width;
            button.Height = height;
            button.FlatStyle = FlatStyle.Flat;
            button.BackColor = ElevatedSurface;
            button.ForeColor = TextPrimary;
            button.Font = ButtonFont;
            button.FlatAppearance.BorderColor = BorderNeutral;
            button.FlatAppearance.BorderSize = 1;
            button.Cursor = Cursors.Hand;
            ApplyButtonInteractionStyle(button, SubtleSurface, BorderNeutral);
            return button;
        }

        public static LinkLabel CreateTextAction(string text)
        {
            var link = new LinkLabel();
            link.Text = text;
            link.AutoSize = true;
            link.LinkColor = Information;
            link.ActiveLinkColor = InstitutionalPrimary;
            link.VisitedLinkColor = Information;
            link.Font = BodyFont;
            link.Cursor = Cursors.Hand;
            return link;
        }
    }
}
