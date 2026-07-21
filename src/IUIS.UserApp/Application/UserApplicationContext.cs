using System;
using System.Windows.Forms;

using IUIS.Application.Navigation;
using IUIS.Application.Security;
using IUIS.Domain.Identity;
using IUIS.Infrastructure.Presentation;
using IUIS.SharedUI.Application;
using IUIS.SharedUI.Controls;
using IUIS.SharedUI.Forms;
using IUIS.UserApp.Configuration;
using IUIS.UserApp.Forms;
using IUIS.UserApp.Forms.Shell;

namespace IUIS.UserApp.Application
{
    internal sealed class UserApplicationContext : TransitionAwareApplicationContext
    {
        private readonly ApplicationRuntime _runtime;

        public UserApplicationContext()
        {
            _runtime = new ApplicationRuntime(UserAppSettings.ResolveDataRoot());
            if (!EnsureRepositoryReady())
            {
                ExitThread();
                return;
            }

            if (!TryAuthenticate(out var session, out var access))
            {
                ExitThread();
                return;
            }

            _runtime.CurrentUser.SetSession(session, access);
            OpenRoleShell();
        }

        private bool EnsureRepositoryReady()
        {
            while (true)
            {
                string statusMessage;
                if (_runtime.StartupReadiness.IsRepositoryReady(out statusMessage))
                    return true;

                using (var failure = new StartupFailureForm(statusMessage, false))
                {
                    failure.ShowDialog();
                    if (!failure.RetryRequested)
                        return false;
                }
            }
        }

        private bool TryAuthenticate(out UserSession session, out EffectiveAccessSnapshot access)
        {
            session = null;
            access = null;

            while (true)
            {
                using (var login = new GeneralLoginForm(_runtime))
                {
                    if (login.ShowDialog() != DialogResult.OK
                        || login.AuthenticationResult == null
                        || !login.AuthenticationResult.IsAuthenticated)
                        return false;

                    if (login.AuthenticationResult.Session.PrimaryRole == PrimaryRole.Administrator)
                    {
                        MessageBox.Show(
                            "Administrator accounts must use the restricted Admin application.",
                            "Sign In",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        continue;
                    }

                    session = login.AuthenticationResult.Session;
                    access = login.AuthenticationResult.Access;

                    if (login.AuthenticationResult.Status == AuthenticationStatus.PasswordChangeRequired
                        || session.Purpose == SessionPurpose.FirstLoginPasswordChange
                        || session.Purpose == SessionPurpose.PasswordResetCompletion)
                    {
                        if (!TryChangePassword(session, out session, out access))
                            continue;
                    }

                    return true;
                }
            }
        }

        private bool TryChangePassword(
            UserSession session,
            out UserSession updatedSession,
            out EffectiveAccessSnapshot updatedAccess)
        {
            updatedSession = session;
            updatedAccess = null;

            using (var passwordForm = new FirstLoginPasswordChangeForm(_runtime.PasswordChange, session))
            {
                if (passwordForm.ShowDialog() != DialogResult.OK
                    || passwordForm.ChangeResult == null
                    || !passwordForm.ChangeResult.Succeeded)
                    return false;

                updatedSession = passwordForm.ChangeResult.Session;
                updatedAccess = passwordForm.ChangeResult.Access;
                return true;
            }
        }

        private void OpenRoleShell()
        {
            var session = _runtime.CurrentUser.Session;
            if (session == null)
            {
                ExitThread();
                return;
            }

            UserShellForm shell = CreateShellForRole(session);
            if (shell == null)
            {
                MessageBox.Show(
                    "This account cannot open the user application.",
                    "Access Denied",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                _runtime.CurrentUser.Clear();
                ExitThread();
                return;
            }

            shell.FormClosed += ShellFormClosed;
            Begin(shell);
        }

        private UserShellForm CreateShellForRole(UserSession session)
        {
            if (session.PrimaryRole == PrimaryRole.Student)
            {
                return new UserShellForm(
                    _runtime,
                    "Student Portal",
                    StudentNavigationCatalog.GetAll(),
                    "STU-DASH-01",
                    "Welcome, " + session.LoginId,
                    new DashboardCardModel { Title = "Enrollment", Value = "—", Caption = "Current term status" },
                    new DashboardCardModel { Title = "Finance", Value = "—", Caption = "Assessment and payments" },
                    new DashboardCardModel { Title = "Services", Value = "—", Caption = "Library, clinic, counseling" },
                    new DashboardCardModel { Title = "Notifications", Value = "—", Caption = "Unread messages" });
            }

            if (session.PrimaryRole == PrimaryRole.EmployeeFaculty)
            {
                return new UserShellForm(
                    _runtime,
                    "Employee Portal",
                    EmployeeNavigationCatalog.GetAll(),
                    "EMP-DASH-01",
                    "Welcome, " + session.LoginId,
                    new DashboardCardModel { Title = "Assignments", Value = "—", Caption = "Teaching and coordination" },
                    new DashboardCardModel { Title = "Queue", Value = "—", Caption = "Pending reviews" },
                    new DashboardCardModel { Title = "Attendance", Value = "—", Caption = "Workforce status" },
                    new DashboardCardModel { Title = "Account", Value = "Active", Caption = "Session secured" });
            }

            return null;
        }

        private void ShellFormClosed(object sender, FormClosedEventArgs e)
        {
            var shell = sender as UserShellForm;
            if (shell != null)
                shell.FormClosed -= ShellFormClosed;

            _runtime.CurrentUser.Clear();

            if (!TryAuthenticate(out var session, out var access))
            {
                ExitThread();
                return;
            }

            _runtime.CurrentUser.SetSession(session, access);

            var nextShell = CreateShellForRole(session);
            if (nextShell == null)
            {
                ExitThread();
                return;
            }

            nextShell.FormClosed += ShellFormClosed;
            TransitionTo(nextShell);
        }
    }
}
