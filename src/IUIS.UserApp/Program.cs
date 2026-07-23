using System;
using System.Windows.Forms;
using System.Threading;

using Microsoft.Extensions.DependencyInjection;

using IUIS.UserApp.Application;
using IUIS.UserApp.Composition;
using IUIS.Infrastructure.Presentation;
using IUIS.SharedUI.Forms;

namespace IUIS.UserApp
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
            
            // Build DI container
            var serviceProvider = UserAppCompositionRoot.BuildServiceProvider();
            
            // Set up global exception handling
            System.Windows.Forms.Application.ThreadException += (sender, e) =>
            {
                var runtime = serviceProvider.GetService<ApplicationRuntime>();
                runtime?.LogError("UserApp", "UnhandledException", e.Exception.ToString());
                
                MessageBox.Show(
                    $"An unexpected error occurred: {e.Exception.Message}\n\n" +
                    "The application will now close. Please contact support if this issue persists.",
                    "IUIS - Unexpected Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            };
            
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                var runtime = serviceProvider.GetService<ApplicationRuntime>();
                runtime?.LogError("UserApp", "FatalException", e.ExceptionObject?.ToString() ?? "Unknown exception");
            };
            
            try
            {
                // Resolve and run the application context
                var appContext = serviceProvider.GetRequiredService<UserApplicationContext>();
                System.Windows.Forms.Application.Run(appContext);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to start application: {ex.Message}\n\n" +
                    "Please ensure all required files are present and you have appropriate permissions.",
                    "IUIS - Startup Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
