using System;
using System.Windows.Forms;
using IUIS.Application.Context;
using IUIS.Application.Security;
using IUIS.Application.Navigation;
using IUIS.SharedUI.Forms.Auth;
using IUIS.SharedUI.Forms.Shell;
using Microsoft.Extensions.DependencyInjection;

namespace IUIS.AdminApp
{
    public class AdministratorApplicationContext : ApplicationContext
    {
        private readonly IServiceProvider _serviceProvider;

        public AdministratorApplicationContext(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            InitializeApplication();
        }

        private void InitializeApplication()
        {
            try
            {
                // 1. Bootstrap Verification (Check if system is initialized)
                var bootstrapForm = _serviceProvider.GetRequiredService<AdministratorBootstrapForm>();
                Application.Run(bootstrapForm);

                if (!bootstrapForm.IsInitialized)
                {
                    // System not initialized, exit or stay in bootstrap loop
                    ExitThread();
                    return;
                }

                // 2. Main Admin Login Loop
                while (true)
                {
                    using var loginForm = _serviceProvider.GetRequiredService<AdminLoginForm>();
                    Application.Run(loginForm);

                    if (!loginForm.AuthenticationSuccess)
                    {
                        ExitThread();
                        return;
                    }

                    // 3. Verify Admin Role
                    var userContext = _serviceProvider.GetRequiredService<CurrentUserContext>();
                    
                    if (!userContext.IsAdministrator)
                    {
                        MessageBox.Show("This application is for Administrators only.", 
                            "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        continue; // Restart loop
                    }

                    // 4. Run Admin Shell
                    var navCatalog = _serviceProvider.GetRequiredService<AdministratorNavigationCatalog>();
                    var navService = _serviceProvider.GetRequiredService<FormNavigationService>();
                    var shellForm = new AdministratorShellForm(navService, navCatalog, userContext);
                    
                    Application.Run(shellForm);
                    
                    // Loop continues for re-login
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Critical Error: {ex.Message}", "System Failure", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ExitThread();
            }
        }
    }
}
