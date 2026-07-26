using System;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using IUIS.UserApp.Composition;
using IUIS.UserApp;

namespace IUIS.UserApp
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // Enable Visual Styles
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Build the Dependency Injection Container
            IServiceProvider serviceProvider;
            try
            {
                serviceProvider = UserAppCompositionRoot.BuildServiceProvider();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to initialize application services:\n\n{ex.Message}\n\nPlease ensure all data files are present and valid.",
                    "IUIS Startup Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            // Set up Global Exception Handling
            Application.ThreadException += (sender, e) =>
            {
                MessageBox.Show(
                    $"An unexpected error occurred:\n\n{e.Exception.Message}",
                    "Application Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                // Optional: Log to audit log here
            };

            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                MessageBox.Show(
                    $"A critical fatal error occurred:\n\n{(e.ExceptionObject as Exception)?.Message}",
                    "Fatal Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Stop);
            };

            // Resolve and Run the ApplicationContext
            try
            {
                var context = serviceProvider.GetRequiredService<UserApplicationContext>();
                Application.Run(context);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to start application context:\n\n{ex.Message}",
                    "Critical Startup Failure",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Stop);
            }
        }
    }
}
