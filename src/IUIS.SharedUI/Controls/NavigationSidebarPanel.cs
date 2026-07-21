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
        private readonly Label _portalLabel;
        private readonly FlowLayoutPanel _groupsPanel;
        private readonly Button _signOutButton;
        private readonly Dictionary<string, Button> _buttons = new Dictionary<string, Button>(StringComparer.OrdinalIgnoreCase);
        private string _selectedKey;

        public NavigationSidebarPanel()
        {
            Width = UiMetrics.SidebarExpandedWidth;
            Dock = DockStyle.Left;
            BackColor = UiTheme.InstitutionalPrimary;
            Padding = new Padding(0, UiMetrics.ContentPadding, 0, UiMetrics.ContentPadding);
            AccessibleName = "Primary navigation";
            AccessibleRole = AccessibleRole.Grouping;

            _portalLabel = new Label();
            _portalLabel.Text = "IUIS Portal";
            _portalLabel.Font = UiTheme.SectionHeadingFont;
            _portalLabel.ForeColor = Color.White;
            _portalLabel.AutoSize = false;
            _portalLabel.Height = 48;
            _portalLabel.Dock = DockStyle.Top;
            _portalLabel.Padding = new Padding(20, 8, 20, 8);

            _groupsPanel = new FlowLayoutPanel();
            _groupsPanel.Dock = DockStyle.Fill;
            _groupsPanel.FlowDirection = FlowDirection.TopDown;
            _groupsPanel.WrapContents = false;
            _groupsPanel.AutoScroll = true;
            _groupsPanel.Padding = new Padding(8);
            _groupsPanel.Width = UiMetrics.SidebarExpandedWidth;

            _signOutButton = CreateNavButton("Sign Out", "sign.out");
            _signOutButton.Dock = DockStyle.Bottom;
            _signOutButton.Click += delegate { OnSignOutRequested(); };

            Controls.Add(_groupsPanel);
            Controls.Add(_signOutButton);
            Controls.Add(_portalLabel);
        }

        public event EventHandler<NavigationEntryModel> NavigationRequested;
        public event EventHandler SignOutRequested;

        public void LoadNavigation(string portalLabel, IEnumerable<NavigationGroupModel> groups)
        {
            _portalLabel.Text = portalLabel ?? "IUIS Portal";
            _groupsPanel.Controls.Clear();
            _buttons.Clear();

            foreach (var group in groups ?? Enumerable.Empty<NavigationGroupModel>())
            {
                if (string.IsNullOrWhiteSpace(group.GroupKey))
                    continue;

                var heading = new Label();
                heading.Text = group.GroupKey.ToUpperInvariant();
                heading.Font = UiTheme.CaptionFont;
                heading.ForeColor = Color.FromArgb(184, 202, 213);
                heading.AutoSize = true;
                heading.AccessibleRole = AccessibleRole.StaticText;
                heading.Margin = new Padding(12, 16, 12, 4);
                heading.Width = UiMetrics.SidebarExpandedWidth - 32;
                _groupsPanel.Controls.Add(heading);

                foreach (var entry in group.Entries)
                {
                    var button = CreateNavButton(entry.DisplayText, entry.Key);
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
                var selected = string.Equals(pair.Key, key, StringComparison.OrdinalIgnoreCase);
                pair.Value.BackColor = selected ? UiTheme.Accent : UiTheme.InstitutionalPrimary;
                pair.Value.Font = selected ? UiTheme.NavigationSelectedFont : UiTheme.BodyFont;
                pair.Value.AccessibleDescription = selected ? "Current page" : "Open " + pair.Value.Text;
            }
        }

        private void NavigationButtonClick(object sender, EventArgs e)
        {
            var button = sender as Button;
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

        private static Button CreateNavButton(string text, string key)
        {
            var button = new Button();
            button.Text = text;
            button.Name = key;
            button.Width = UiMetrics.SidebarExpandedWidth - 32;
            button.Height = UiMetrics.StandardButtonHeight;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.TextAlign = ContentAlignment.MiddleLeft;
            button.Padding = new Padding(12, 0, 0, 0);
            button.ForeColor = Color.White;
            button.BackColor = UiTheme.InstitutionalPrimary;
            button.Font = UiTheme.BodyFont;
            button.Cursor = Cursors.Hand;
            button.Margin = new Padding(4, 2, 4, 2);
            button.AccessibleName = text;
            button.AccessibleRole = AccessibleRole.PushButton;
            UiTheme.ApplyButtonInteractionStyle(button, UiTheme.InstitutionalDark, UiTheme.Accent);
            return button;
        }
    }
}
