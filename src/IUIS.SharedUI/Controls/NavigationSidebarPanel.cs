using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using IUIS.SharedUI.Navigation;
using IUIS.SharedUI.Theme;

namespace IUIS.SharedUI.Controls
{
    public sealed class NavigationSidebarPanel : Panel
    {
        private readonly Panel _brandPanel;
        private readonly Label _portalLabel;
        private readonly Label _portalSubLabel;
        private readonly FlowLayoutPanel _groupsPanel;
        private readonly Panel _signOutHost;
        private readonly SidebarNavButton _signOutButton;
        private readonly Dictionary<string, SidebarNavButton> _buttons =
            new Dictionary<string, SidebarNavButton>(StringComparer.OrdinalIgnoreCase);
        private string _selectedKey;

        public NavigationSidebarPanel()
        {
            Width = UiMetrics.SidebarExpandedWidth;
            Dock = DockStyle.Left;
            BackColor = UiTheme.InstitutionalDeeper;
            Padding = new Padding(0);
            DoubleBuffered = true;

            // ---- Brand block ----
            _brandPanel = new Panel();
            _brandPanel.Dock = DockStyle.Top;
            _brandPanel.Height = 72;
            _brandPanel.BackColor = UiTheme.InstitutionalDeeper;
            _brandPanel.Padding = new Padding(20, 14, 20, 10);
            _brandPanel.Paint += BrandPanelPaint;

            _portalLabel = new Label();
            _portalLabel.Text = "IUIS Portal";
            _portalLabel.Font = new Font("Segoe UI", 13f, FontStyle.Bold);
            _portalLabel.ForeColor = Color.White;
            _portalLabel.AutoSize = false;
            _portalLabel.Dock = DockStyle.Top;
            _portalLabel.Height = 26;
            _portalLabel.TextAlign = ContentAlignment.MiddleLeft;

            _portalSubLabel = new Label();
            _portalSubLabel.Text = "Integrated University Information System";
            _portalSubLabel.Font = UiTheme.CaptionFont;
            _portalSubLabel.ForeColor = UiTheme.TextOnDeepNavy;
            _portalSubLabel.AutoSize = false;
            _portalSubLabel.Dock = DockStyle.Top;
            _portalSubLabel.Height = 18;
            _portalSubLabel.TextAlign = ContentAlignment.MiddleLeft;

            _brandPanel.Controls.Add(_portalSubLabel);
            _brandPanel.Controls.Add(_portalLabel);

            // ---- Nav groups ----
            _groupsPanel = new FlowLayoutPanel();
            _groupsPanel.Dock = DockStyle.Fill;
            _groupsPanel.FlowDirection = FlowDirection.TopDown;
            _groupsPanel.WrapContents = false;
            _groupsPanel.AutoScroll = true;
            _groupsPanel.Padding = new Padding(8, 10, 8, 8);
            _groupsPanel.BackColor = UiTheme.InstitutionalDeeper;

            // ---- Sign out (pinned bottom, divider above) ----
            _signOutHost = new Panel();
            _signOutHost.Dock = DockStyle.Bottom;
            _signOutHost.Height = UiMetrics.SidebarItemHeight + 18;
            _signOutHost.BackColor = UiTheme.InstitutionalDeeper;
            _signOutHost.Padding = new Padding(8, 9, 8, 9);
            _signOutHost.Paint += SignOutHostPaint;

            _signOutButton = new SidebarNavButton();
            _signOutButton.Text = "Sign Out";
            _signOutButton.Name = "sign.out";
            _signOutButton.Dock = DockStyle.Fill;
            _signOutButton.IsSignOut = true;
            _signOutButton.Click += delegate { OnSignOutRequested(); };

            _signOutHost.Controls.Add(_signOutButton);

            Controls.Add(_groupsPanel);
            Controls.Add(_signOutHost);
            Controls.Add(_brandPanel);
        }

        public event EventHandler<NavigationEntryModel> NavigationRequested;
        public event EventHandler SignOutRequested;

        public void LoadNavigation(
            string portalLabel,
            IEnumerable<NavigationGroupModel> groups)
        {
            _portalLabel.Text = portalLabel ?? "IUIS Portal";
            _groupsPanel.Controls.Clear();
            _buttons.Clear();

            bool firstGroup = true;
            foreach (var group in groups ?? Enumerable.Empty<NavigationGroupModel>())
            {
                if (string.IsNullOrWhiteSpace(group.GroupKey))
                    continue;

                var heading = new Label();
                heading.Text = SpaceOut(group.GroupKey.ToUpperInvariant());
                heading.Font = UiTheme.CaptionFont;
                heading.ForeColor = Color.FromArgb(148, 168, 190);
                heading.AutoSize = false;
                heading.Height = firstGroup ? 22 : 34;
                heading.Width = UiMetrics.SidebarExpandedWidth - 32;
                heading.TextAlign = ContentAlignment.BottomLeft;
                heading.Padding = new Padding(12, 0, 0, 4);
                heading.Margin = new Padding(4, firstGroup ? 4 : 10, 4, 2);
                if (!firstGroup)
                {
                    heading.Paint += GroupHeadingPaint;
                }
                _groupsPanel.Controls.Add(heading);
                firstGroup = false;

                foreach (var entry in group.Entries)
                {
                    var button = new SidebarNavButton();
                    button.Text = entry.DisplayText;
                    button.Name = entry.Key;
                    button.Width = UiMetrics.SidebarExpandedWidth - 24;
                    button.Height = UiMetrics.SidebarItemHeight;
                    button.Margin = new Padding(4, 2, 4, 2);
                    button.Tag = entry;
                    button.Click += NavigationButtonClick;
                    _buttons[entry.Key] = button;
                    _groupsPanel.Controls.Add(button);
                }
            }
        }

        public void SelectEntry(string key)
        {
            _selectedKey = key;
            foreach (var pair in _buttons)
            {
                pair.Value.IsSelected = string.Equals(pair.Key, key, StringComparison.OrdinalIgnoreCase);
            }
        }

        private void NavigationButtonClick(object sender, EventArgs e)
        {
            var button = sender as SidebarNavButton;
            var entry = button == null ? null : button.Tag as NavigationEntryModel;
            if (entry == null)
                return;

            SelectEntry(entry.Key);
            var handler = NavigationRequested;
            if (handler != null)
                handler(this, entry);
        }

        private void OnSignOutRequested()
        {
            var handler = SignOutRequested;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private void BrandPanelPaint(object sender, PaintEventArgs e)
        {
            var panel = (Panel)sender;
            // Gold accent under the brand block.
            using (var accentPen = new Pen(UiTheme.InstitutionalAccent, 2f))
            {
                e.Graphics.DrawLine(accentPen, 20, panel.Height - 1, 56, panel.Height - 1);
            }
            using (var dividerPen = new Pen(Color.FromArgb(40, 255, 255, 255), 1f))
            {
                e.Graphics.DrawLine(dividerPen, 56, panel.Height - 1, panel.Width - 20, panel.Height - 1);
            }
        }

        private void SignOutHostPaint(object sender, PaintEventArgs e)
        {
            var panel = (Panel)sender;
            using (var dividerPen = new Pen(Color.FromArgb(40, 255, 255, 255), 1f))
            {
                e.Graphics.DrawLine(dividerPen, 16, 0, panel.Width - 16, 0);
            }
        }

        private void GroupHeadingPaint(object sender, PaintEventArgs e)
        {
            var heading = (Label)sender;
            using (var dividerPen = new Pen(Color.FromArgb(28, 255, 255, 255), 1f))
            {
                e.Graphics.DrawLine(dividerPen, 12, 4, heading.Width - 12, 4);
            }
        }

        private static string SpaceOut(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var chars = new List<char>(text.Length * 2);
            for (int i = 0; i < text.Length; i++)
            {
                chars.Add(text[i]);
                if (i < text.Length - 1)
                    chars.Add(' ');
            }
            return new string(chars.ToArray());
        }

        /// <summary>
        /// Custom-painted sidebar nav item: rounded hover fill, rounded selected
        /// fill with a gold left accent bar, and error-tinted hover for sign out.
        /// </summary>
        private sealed class SidebarNavButton : Button
        {
            private bool _isHovered;
            private bool _isPressed;
            private bool _isSelected;

            public SidebarNavButton()
            {
                SetStyle(
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.UserPaint |
                    ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.ResizeRedraw,
                    true);
                FlatStyle = FlatStyle.Flat;
                FlatAppearance.BorderSize = 0;
                Font = UiTheme.BodyFont;
                Cursor = Cursors.Hand;
                Height = UiMetrics.SidebarItemHeight;
            }

            public bool IsSignOut { get; set; }

            public bool IsSelected
            {
                get { return _isSelected; }
                set
                {
                    _isSelected = value;
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
                _isPressed = true;
                Invalidate();
            }

            protected override void OnMouseUp(MouseEventArgs mevent)
            {
                base.OnMouseUp(mevent);
                _isPressed = false;
                Invalidate();
            }

            protected override void OnPaint(PaintEventArgs pevent)
            {
                var graphics = pevent.Graphics;
                var bounds = ClientRectangle;

                using (var backBrush = new SolidBrush(UiTheme.InstitutionalDeeper))
                {
                    graphics.FillRectangle(backBrush, bounds);
                }

                Color fill = Color.Empty;
                Color text = IsSignOut ? UiTheme.TextOnDeepNavy : Color.FromArgb(214, 226, 238);
                Font font = UiTheme.BodyFont;

                if (_isSelected)
                {
                    fill = UiTheme.InstitutionalPrimary;
                    text = Color.White;
                    font = UiTheme.BodyBoldFont;
                }
                else if (_isPressed)
                {
                    fill = IsSignOut
                        ? Color.FromArgb(88, 38, 34)
                        : UiTheme.Darken(UiTheme.InstitutionalHover, 0.15f);
                    text = Color.White;
                }
                else if (_isHovered)
                {
                    fill = IsSignOut
                        ? Color.FromArgb(74, 34, 32)
                        : UiTheme.InstitutionalHover;
                    text = Color.White;
                }

                if (!fill.IsEmpty)
                {
                    UiPainting.PaintRoundedSurface(graphics, bounds, UiMetrics.CornerRadius, fill, Color.Empty);
                }

                if (_isSelected)
                {
                    // Gold accent bar on the left edge of the selected item.
                    var accentRect = new Rectangle(
                        bounds.X + 4,
                        bounds.Y + 8,
                        UiMetrics.AccentBarWidth,
                        bounds.Height - 16);
                    using (var accentBrush = new SolidBrush(UiTheme.InstitutionalAccent))
                    {
                        graphics.FillRectangle(accentBrush, accentRect);
                    }
                }

                var textRect = new Rectangle(bounds.X + 16, bounds.Y, bounds.Width - 20, bounds.Height);
                TextRenderer.DrawText(
                    graphics,
                    Text,
                    font,
                    textRect,
                    text,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            }
        }
    }
}
