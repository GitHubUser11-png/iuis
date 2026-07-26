using System;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;

using IUIS.SharedUI.Forms;
using IUIS.SharedUI.Application;
using IUIS.UserApp.Forms.Startup;
using IUIS.UserApp.Forms.Auth;
using IUIS.UserApp.Forms.Shell;
using IUIS.Application.Security;
using IUIS.Application.Context;
using IUIS.Application.Navigation;
using IUIS.Infrastructure.Presentation;

namespace IUIS.UserApp
{
    public class UserApplicationContext : ApplicationContext
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ApplicationRuntime _applicationRuntime;
        
        private Form _currentForm;

        public UserApplicationContext(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _applicationRuntime = serviceProvider.GetRequiredService<ApplicationRuntime>();

            // Start the lifecycle
            InitializeLifecycle();
        }

        private void InitializeLifecycle()
        {
            try
            {
                // 1. Startup Health Check
                var startupForm = _serviceProvider.GetRequiredService<StartupCheckForm>();
                ShowForm(startupForm);

                if (!startupForm.IsSystemReady)
                {
                    // System not ready (missing repos, corrupt data)
                    var failureForm = _serviceProvider.GetRequiredService<StartupFailureForm>();
                    // Pass error details from startupForm if available
                    failureForm.ErrorMessage = startupForm.FailureReason ?? "System initialization failed.";
                    ShowForm(failureForm);
                    return;
                }

                // 2. Enter Login Loop
                EnterLoginCycle();
            }
            catch (Exception ex)
            {
                HandleCriticalFailure(ex);
            }
        }

        private void EnterLoginCycle()
        {
            while (true)
            {
                try
                {
                    // 3. Show Login Form
                    // We create a new instance every time to ensure clean state
                    var loginForm = _serviceProvider.GetRequiredService<GeneralLoginForm>();
                    
                    // Attach event handler for successful login
                    loginForm.LoginSuccess += OnLoginSuccess;
                    
                    ShowForm(loginForm);

                    // If the form closed via Logout (not Exit), the loop continues
                    // If the form closed via Exit application, we break
                    if (!_applicationRuntime.ShouldContinueSession)
                    {
                        break;
                    }
                    
                    // Clean up before next iteration
                    loginForm.LoginSuccess -= OnLoginSuccess;
                    loginForm.Dispose();
                }
                catch (Exception ex)
                {
                    HandleCriticalFailure(ex);
                    break;
                }
            }

            // Exit application
            ExitThread();
        }

        private void OnLoginSuccess(ICurrentUserContext userContext)
        {
            try
            {
                // 4. Determine Role and Load Appropriate Shell
                string role = userContext.PrimaryRole; // "Student" or "Employee"

                INavigationCatalog catalog;
                if (role == "Employee")
                {
                    catalog = _serviceProvider.GetRequiredService<EmployeeNavigationCatalog>();
                }
                else
                {
                    // Default to Student
                    catalog = _serviceProvider.GetRequiredService<StudentNavigationCatalog>();
                }

                // 5. Create and Show Shell
                // Inject services directly into the shell
                var shellForm = new UserShellForm(
                    _serviceProvider.GetRequiredService<INavigationService>(),
                    catalog,
                    userContext
                );

                // Wire up logout
                shellForm.LogoutRequested += (s, e) =>
                {
                    _applicationRuntime.ShouldContinueSession = true; // Signal to continue loop
                    shellForm.Close();
                };

                // Wire up exit
                shellForm.ExitRequested += (s, e) =>
                {
                    _applicationRuntime.ShouldContinueSession = false; // Signal to stop loop
                    shellForm.Close();
                };

                ShowForm(shellForm);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to load user dashboard: {ex.Message}", 
                    "Critical Error", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
                
                // Force logout back to login screen
                _applicationRuntime.ShouldContinueSession = true;
                if (_currentForm != null)
                {
                    _currentForm.Close();
                }
            }
        }

        private void ShowForm(Form form)
        {
            if (_currentForm != null)
            {
                _currentForm.Close();
                _currentForm.Dispose();
            }

            _currentForm = form;
            MainForm = form;
            form.Show();
        }

        private void HandleCriticalFailure(Exception ex)
        {
            // Log to file if possible (using Infrastructure logger if available)
            // For now, show a blocking error
            MessageBox.Show(
                $"A critical error occurred: {ex.Message}\n\nThe application will now close.",
                "Unrecoverable Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Stop);
            
            ExitThread();
        }
    }
}
