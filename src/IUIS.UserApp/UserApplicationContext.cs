using System;
using System.Windows.Forms;
using IUIS.Application.Context;
using IUIS.Application.Security;
using IUIS.Application.Navigation;
using IUIS.SharedUI.Forms.Auth;
using IUIS.SharedUI.Forms.Shell;
using Microsoft.Extensions.DependencyInjection;

namespace IUIS.UserApp
{
    public class UserApplicationContext : ApplicationContext
    {
        private readonly IServiceProvider _serviceProvider;

        public UserApplicationContext(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            InitializeApplication();
        }

        private void InitializeApplication()
        {
            try
            {
                // 1. Show Startup Check
                var startupCheckForm = _serviceProvider.GetRequiredService<StartupCheckForm>();
                Application.Run(startupCheckForm);

                if (!startupCheckForm.IsReady)
                {
                    var failureForm = _serviceProvider.GetRequiredService<StartupFailureForm>();
                    Application.Run(failureForm);
                    ExitThread();
                    return;
                }

                // 2. Main Login Loop
                while (true)
                {
                    using var loginForm = _serviceProvider.GetRequiredService<GeneralLoginForm>();
                    Application.Run(loginForm);

                    if (!loginForm.AuthenticationSuccess)
                    {
                        ExitThread();
                        return;
                    }

                    // 3. Resolve Current User Context & Route
                    var userContext = _serviceProvider.GetRequiredService<CurrentUserContext>();
                    
                    if (userContext.IsAdministrator)
                    {
                        MessageBox.Show("Administrators must use the IUIS Administrator application.", 
                            "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        continue; // Restart loop
                    }

                    Form shellForm;
                    if (userContext.IsStudent)
                    {
                        var navCatalog = _serviceProvider.GetRequiredService<StudentNavigationCatalog>();
                        var navService = _serviceProvider.GetRequiredService<FormNavigationService>();
                        shellForm = new UserShellForm(navService, navCatalog, userContext);
                    }
                    else if (userContext.IsEmployee)
                    {
                        var navCatalog = _serviceProvider.GetRequiredService<EmployeeNavigationCatalog>();
                        var navService = _serviceProvider.GetRequiredService<FormNavigationService>();
                        shellForm = new UserShellForm(navService, navCatalog, userContext);
                    }
                    else
                    {
                        MessageBox.Show("Unknown user role.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        continue;
                    }

                    // 4. Run Shell until Logout
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
