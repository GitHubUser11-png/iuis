using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using IUIS.SharedUI.Navigation;
using IUIS.SharedUI.Theme;

namespace IUIS.SharedUI.Controls
{
    public sealed class ApplicationShellPanel : Panel
    {
        private readonly NavigationSidebarPanel _sidebar;
        private readonly ApplicationHeaderBarPanel _header;
        private readonly Panel _contentHost;
        private readonly Label _statusBar;
        private readonly Dictionary<string, Control> _pages = new Dictionary<string, Control>(StringComparer.OrdinalIgnoreCase);
        private string _currentPageKey;

        public ApplicationShellPanel()
        {
            Dock = DockStyle.Fill;
            BackColor = UiTheme.Surface;

            _sidebar = new NavigationSidebarPanel();
            _sidebar.NavigationRequested += SidebarNavigationRequested;

            _header = new ApplicationHeaderBarPanel();

            _contentHost = new Panel();
            _contentHost.Dock = DockStyle.Fill;
            _contentHost.BackColor = UiTheme.Surface;
            _contentHost.Padding = new Padding(0);

            _statusBar = new Label();
            _statusBar.Dock = DockStyle.Bottom;
            _statusBar.Height = UiMetrics.StatusBarHeight;
            _statusBar.BackColor = UiTheme.ElevatedSurface;
            _statusBar.ForeColor = UiTheme.TextSecondary;
            _statusBar.Font = UiTheme.CaptionFont;
            _statusBar.TextAlign = ContentAlignment.MiddleLeft;
            _statusBar.Padding = new Padding(UiMetrics.ContentPadding, 0, 0, 0);

            var mainColumn = new Panel();
            mainColumn.Dock = DockStyle.Fill;
            mainColumn.Controls.Add(_contentHost);
            mainColumn.Controls.Add(_header);
            mainColumn.Controls.Add(_statusBar);

            Controls.Add(mainColumn);
            Controls.Add(_sidebar);
        }

        public event EventHandler<NavigationEntryModel> PageChanged;
        public event EventHandler SignOutRequested;

        public NavigationSidebarPanel Sidebar
        {
            get { return _sidebar; }
        }

        public void InitializeShell(
            string portalLabel,
            IEnumerable<NavigationGroupModel> groups,
            string userDisplay,
            string statusText)
        {
            _sidebar.LoadNavigation(portalLabel, groups);
            _sidebar.SignOutRequested += delegate
            {
                var handler = SignOutRequested;
                if (handler != null)
                    handler(this, EventArgs.Empty);
            };
            _header.SetUserDisplay(userDisplay);
            _statusBar.Text = statusText ?? string.Empty;
        }

        public void RegisterPage(string pageKey, Control pageControl, string title)
        {
            if (string.IsNullOrWhiteSpace(pageKey) || pageControl == null)
                return;

            pageControl.Dock = DockStyle.Fill;
            pageControl.Visible = false;
            _pages[pageKey] = pageControl;
            _contentHost.Controls.Add(pageControl);
        }

        public void ShowPage(NavigationEntryModel entry)
        {
            if (entry == null)
                return;

            _currentPageKey = entry.PageKey;
            foreach (var pair in _pages)
            {
                pair.Value.Visible = string.Equals(pair.Key, entry.PageKey, StringComparison.OrdinalIgnoreCase);
            }

            _header.SetPage(entry.DisplayText, "Integrated University Information System");
            _sidebar.SelectEntry(entry.Key);

            var handler = PageChanged;
            if (handler != null)
                handler(this, entry);
        }

        public void ShowPageByKey(string pageKey, string title)
        {
            ShowPage(new NavigationEntryModel
            {
                Key = pageKey,
                DisplayText = title,
                PageKey = pageKey
            });
        }

        public void UpdateStatusBar(string statusText)
        {
            _statusBar.Text = statusText ?? string.Empty;
        }

        private void SidebarNavigationRequested(object sender, NavigationEntryModel entry)
        {
            ShowPage(entry);
        }
    }
}
