using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using IUIS.Application.Navigation;
using IUIS.Application.Security;
using IUIS.Domain.Identity;
using IUIS.Infrastructure.Presentation;
using IUIS.SharedUI;
using IUIS.SharedUI.Controls;
using IUIS.SharedUI.Navigation;
using IUIS.SharedUI.Shell;
using IUIS.SharedUI.Theme;
using IUIS.UserApp.Forms;
using AppIdentity = IUIS.SharedUI.ApplicationIdentity;
using AppUserSession = IUIS.Application.Security.UserSession;

namespace IUIS.UserApp.Forms.Shell
{
    internal sealed class UserShellForm : Form
    {
        private readonly ApplicationRuntime _runtime;
        private readonly ApplicationShellPanel _shell;
        private readonly Timer _sessionTimer;
        private readonly string _dashboardPageKey;
        private readonly IReadOnlyList<NavigationItemDefinition> _navigationItems;

        public UserShellForm(
            ApplicationRuntime runtime,
            string portalLabel,
            IReadOnlyList<NavigationItemDefinition> navigationItems,
            string dashboardPageKey,
            string greeting,
            params DashboardCardModel[] cards)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            _navigationItems = navigationItems ?? throw new ArgumentNullException(nameof(navigationItems));
            _dashboardPageKey = dashboardPageKey ?? "STU-DASH-01";

            Text = AppIdentity.ProductName + " — " + portalLabel;
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(UiMetrics.MinimumWindowWidth, UiMetrics.MinimumWindowHeight);
            ClientSize = new Size(UiMetrics.DefaultWindowWidth, UiMetrics.DefaultWindowHeight);
            UiTheme.ApplyBaseFormStyle(this);

            _shell = new ApplicationShellPanel();
            _shell.Dock = DockStyle.Fill;
            _shell.SignOutRequested += ShellSignOutRequested;

            var filtered = NavigationFilter.Filter(_navigationItems, _runtime.CurrentUser.Access);
            var groups = NavigationGroupBuilder.BuildGroups(filtered);
            var dashboard = ShellPageFactory.CreateDashboard(greeting, cards);
            
            // Instantiate page factory with composition root and session token for service injection
            var sessionToken = _runtime.CurrentUser.Session?.SessionToken ?? string.Empty;
            var pageFactory = new UserPageFactory(_runtime.Composition, sessionToken);
            ShellPageFactory.RegisterModulePages(_shell, filtered, _dashboardPageKey, dashboard, pageFactory.CreatePage);

            _shell.InitializeShell(
                portalLabel,
                groups,
                BuildUserDisplay(),
                "Session active — " + AppIdentity.CampusSubtitle);

            Controls.Add(_shell);

            var firstEntry = filtered.FirstOrDefault(item => item.AlwaysVisible)
                ?? filtered.FirstOrDefault();
            if (firstEntry != null)
                _shell.ShowPageByKey(firstEntry.PageKey, firstEntry.DisplayText);

            _sessionTimer = new Timer();
            _sessionTimer.Interval = 60000;
            _sessionTimer.Tick += SessionTimerTick;
            _sessionTimer.Start();
        }

        public event EventHandler SignOutCompleted;

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _sessionTimer.Stop();
            _sessionTimer.Dispose();
            base.OnFormClosed(e);
        }

        private string BuildUserDisplay()
        {
            var session = _runtime.CurrentUser.Session;
            if (session == null)
                return string.Empty;

            return session.LoginId + "  ·  " + session.PrimaryRole;
        }

        private void SessionTimerTick(object sender, EventArgs e)
        {
            AppUserSession session;
            EffectiveAccessSnapshot access;
            string reason;
            var credential = _runtime.CurrentUser.GetCredential();
            if (!_runtime.SessionPresentation.ValidateSession(credential, out session, out access, out reason))
            {
                MessageBox.Show(
                    this,
                    reason ?? "Your session has ended.",
                    AppIdentity.ProductName,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                PerformSignOut(false);
                return;
            }

            _runtime.CurrentUser.SetSession(session, access);
            _runtime.SessionPresentation.Touch(credential);
        }

        private void ShellSignOutRequested(object sender, EventArgs e)
        {
            var confirm = MessageBox.Show(
                this,
                "Sign out of the Integrated University Information System?",
                AppIdentity.ProductName,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            if (confirm == DialogResult.Yes)
                PerformSignOut(true);
        }

        private void PerformSignOut(bool revokeSession)
        {
            if (revokeSession)
            {
                var credential = _runtime.CurrentUser.GetCredential();
                _runtime.SessionPresentation.Revoke(credential, "User signed out.");
            }

            _runtime.CurrentUser.Clear();
            var handler = SignOutCompleted;
            if (handler != null)
                handler(this, EventArgs.Empty);
            Close();
        }
    }
}
