using System.Drawing;
using System.Windows.Forms;

namespace IUIS.SharedUI.Theme
{
    public static class UiTheme
    {
        public static readonly Color InstitutionalPrimary = Color.FromArgb(27, 58, 92);
        public static readonly Color InstitutionalDark = Color.FromArgb(15, 36, 56);
        public static readonly Color Surface = Color.FromArgb(244, 246, 248);
        public static readonly Color ElevatedSurface = Color.White;
        public static readonly Color BorderNeutral = Color.FromArgb(208, 214, 221);
        public static readonly Color TextPrimary = Color.FromArgb(26, 32, 44);
        public static readonly Color TextSecondary = Color.FromArgb(90, 100, 112);
        public static readonly Color Success = Color.FromArgb(30, 125, 78);
        public static readonly Color Warning = Color.FromArgb(184, 110, 0);
        public static readonly Color Error = Color.FromArgb(179, 38, 30);
        public static readonly Color Information = Color.FromArgb(26, 95, 180);
        public static readonly Color Restricted = Color.FromArgb(107, 45, 131);

        public static readonly Font ApplicationTitleFont = new Font("Segoe UI", 18f, FontStyle.Bold);
        public static readonly Font PageTitleFont = new Font("Segoe UI", 16f, FontStyle.Bold);
        public static readonly Font SectionHeadingFont = new Font("Segoe UI", 11f, FontStyle.Bold);
        public static readonly Font BodyFont = new Font("Segoe UI", 10f, FontStyle.Regular);
        public static readonly Font FieldLabelFont = new Font("Segoe UI", 9f, FontStyle.Regular);
        public static readonly Font CaptionFont = new Font("Segoe UI", 8.25f, FontStyle.Regular);
        public static readonly Font ButtonFont = new Font("Segoe UI", 9.75f, FontStyle.Regular);

        public static void ApplyBaseFormStyle(Form form)
        {
            form.Font = BodyFont;
            form.BackColor = Surface;
            form.ForeColor = TextPrimary;
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
