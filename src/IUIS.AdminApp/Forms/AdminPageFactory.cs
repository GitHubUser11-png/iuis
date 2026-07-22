using System;
using System.Windows.Forms;

using IUIS.Infrastructure.Composition;
using IUIS.SharedUI.Shell;

namespace IUIS.AdminApp.Forms
{
    /// <summary>
    /// Factory for creating administrator portal pages.
    /// Wires page controls to their corresponding services.
    /// </summary>
    public sealed class AdminPageFactory
    {
        private readonly IuisCompositionRoot _composition;
        private readonly string _sessionToken;

        public AdminPageFactory(IuisCompositionRoot composition, string sessionToken)
        {
            _composition = composition ?? throw new ArgumentNullException(nameof(composition));
            _sessionToken = sessionToken ?? throw new ArgumentNullException(nameof(sessionToken));
        }

        /// <summary>
        /// Creates a page control for the given navigation key.
        /// </summary>
        public UserControl CreatePage(string pageKey, string displayText)
        {
            try
            {
                // Admin Dashboard
                if (pageKey == "ADM-DASH-01")
                    return new AdminDashboardPage(_sessionToken);

                // User Management (multiple routes to same page)
                if (pageKey == "ADM-APP-01" || pageKey == "ADM-USR-01")
                    return new UserManagementPage(_sessionToken);

                // System Administration (multiple routes to same page)
                if (pageKey == "ADM-PERM-01" || pageKey == "ADM-LOG-01" || pageKey == "ADM-SEC-01")
                    return new SystemAdministrationPage(_sessionToken);

                // Reports (multiple routes to same page)
                if (pageKey == "ADM-REP-01" || pageKey == "ADM-AUD-01" || pageKey == "ADM-SET-01" || pageKey == "ADM-RPT-01")
                    return new ReportsPage(_sessionToken);

                // Unimplemented pages return graceful placeholder
                return ShellPageFactory.CreatePlaceholderPage(pageKey, displayText);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating page {pageKey}: {ex.Message}");
                return ShellPageFactory.CreatePlaceholderPage(pageKey, displayText + " (Error)");
            }
        }
    }
}
