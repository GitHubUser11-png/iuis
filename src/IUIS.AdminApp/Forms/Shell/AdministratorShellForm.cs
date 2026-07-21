using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using IUIS.Application.Navigation;
using IUIS.Application.Security;
using IUIS.Infrastructure.Presentation;
using IUIS.SharedUI;
using IUIS.SharedUI.Controls;
using IUIS.SharedUI.Navigation;
using IUIS.SharedUI.Shell;
using IUIS.SharedUI.Theme;

namespace IUIS.AdminApp.Forms.Shell
{
    internal sealed class AdministratorShellForm : Form
    {
        private readonly ApplicationRuntime _runtime;
        private readonly ApplicationShellPanel _shell;
        private readonly Timer _sessionTimer;

        public AdministratorShellForm(ApplicationRuntime runtime)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));

            Text = ApplicationIdentity.ProductName + " — Administrator Workspace";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(UiMetrics.MinimumWindowWidth, UiMetrics.MinimumWindowHeight);
            ClientSize = new Size(UiMetrics.DefaultWindowWidth, UiMetrics.DefaultWindowHeight);
            UiTheme.ApplyBaseFormStyle(this);

            _shell = new ApplicationShellPanel();
            _shell.Dock = DockStyle.Fill;
            _shell.SignOutRequested += ShellSignOutRequested;

            var items = AdministratorNavigationCatalog.GetAll();
            var filtered = NavigationFilter.Filter(items, _runtime.CurrentUser.Access);
            var groups = NavigationGroupBuilder.BuildGroups(filtered);
            var session = _runtime.CurrentUser.Session;
            var greeting = session == null ? "Administrator" : "Welcome, " + session.LoginId;
            var dashboard = ShellPageFactory.CreateDashboard(
                greeting,
                new DashboardCardModel { Title = "Applications", Value = "—", Caption = "Pending account reviews" },
                new DashboardCardModel { Title = "Security", Value = "—", Caption = "Login and access events" },
                new DashboardCardModel { Title = "Repository", Value = "49", Caption = "Authoritative JSON files" },
                new DashboardCardModel { Title = "Audit", Value = "—", Caption = "Recent administrative actions" });

            ShellPageFactory.RegisterModulePages(_shell, filtered, "ADM-DASH-01", dashboard);

            _shell.InitializeShell(
                "Administrator Portal",
                groups,
                BuildUserDisplay(),
                "Restricted session — " + ApplicationIdentity.CampusSubtitle);

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

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _sessionTimer.Stop();
            _sessionTimer.Dispose();
            base.OnFormClosed(e);
        }

        private string BuildUserDisplay()
        {
            var session = _runtime.CurrentUser.Session;
            return session == null ? string.Empty : session.LoginId + "  ·  Administrator";
        }

        private void SessionTimerTick(object sender, EventArgs e)
        {
            UserSession session;
            EffectiveAccessSnapshot access;
            string reason;
            var credential = _runtime.CurrentUser.GetCredential();
            if (!_runtime.SessionPresentation.ValidateSession(credential, out session, out access, out reason))
            {
                MessageBox.Show(
                    this,
                    reason ?? "Your session has ended.",
                    ApplicationIdentity.ProductName,
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
                "Sign out of the Administrator application?",
                ApplicationIdentity.ProductName,
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
                _runtime.SessionPresentation.Revoke(credential, "Administrator signed out.");
            }

            _runtime.CurrentUser.Clear();
            Close();
        }
    }
}
