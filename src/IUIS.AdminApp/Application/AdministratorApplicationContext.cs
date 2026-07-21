using System;
using System.Windows.Forms;

using IUIS.Application.Security;
using IUIS.Domain.Identity;
using IUIS.Infrastructure.Presentation;
using IUIS.SharedUI.Application;
using IUIS.SharedUI.Forms;
using IUIS.AdminApp.Configuration;
using IUIS.AdminApp.Forms.Authentication;
using IUIS.AdminApp.Forms.Shell;
using IUIS.AdminApp.Forms.Startup;

namespace IUIS.AdminApp.Application
{
    internal sealed class AdministratorApplicationContext : TransitionAwareApplicationContext
    {
        private readonly ApplicationRuntime _runtime;

        public AdministratorApplicationContext()
        {
            _runtime = new ApplicationRuntime(AdminAppSettings.ResolveDataRoot());
            if (!EnsureRepositoryAndBootstrap())
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
            OpenAdministratorShell();
        }

        private bool EnsureRepositoryAndBootstrap()
        {
            string statusMessage;
            if (!_runtime.StartupReadiness.IsRepositoryReady(out statusMessage))
            {
                using (var init = new RepositoryInitializationForm(_runtime))
                {
                    if (init.ShowDialog() != DialogResult.OK || init.BootstrapResult == null)
                        return false;

                    using (var result = new BootstrapResultDialog(
                        init.AdministratorLoginId,
                        init.TemporaryPassword))
                    {
                        result.ShowDialog();
                    }
                }

                return true;
            }

            if (_runtime.StartupReadiness.RequiresBootstrap(out statusMessage))
            {
                using (var bootstrap = new AdministratorBootstrapForm(_runtime))
                {
                    if (bootstrap.ShowDialog() != DialogResult.OK || bootstrap.BootstrapResult == null)
                        return false;

                    using (var result = new BootstrapResultDialog(
                        bootstrap.AdministratorLoginId,
                        bootstrap.TemporaryPassword))
                    {
                        result.ShowDialog();
                    }
                }
            }

            return true;
        }

        private bool TryAuthenticate(out UserSession session, out EffectiveAccessSnapshot access)
        {
            session = null;
            access = null;

            while (true)
            {
                using (var login = new AdminLoginForm(_runtime))
                {
                    if (login.ShowDialog() != DialogResult.OK
                        || login.AuthenticationResult == null
                        || !login.AuthenticationResult.IsAuthenticated)
                        return false;

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

        private void OpenAdministratorShell()
        {
            var shell = new AdministratorShellForm(_runtime);
            shell.FormClosed += ShellFormClosed;
            Begin(shell);
        }

        private void ShellFormClosed(object sender, FormClosedEventArgs e)
        {
            var shell = sender as AdministratorShellForm;
            if (shell != null)
                shell.FormClosed -= ShellFormClosed;

            _runtime.CurrentUser.Clear();

            if (!TryAuthenticate(out var session, out var access))
            {
                ExitThread();
                return;
            }

            _runtime.CurrentUser.SetSession(session, access);

            var nextShell = new AdministratorShellForm(_runtime);
            nextShell.FormClosed += ShellFormClosed;
            TransitionTo(nextShell);
        }
    }
}
