using System.Collections.Generic;
using System.Windows.Forms;

using IUIS.Application.Navigation;
using IUIS.SharedUI.Controls;

namespace IUIS.SharedUI.Shell
{
    public delegate UserControl PageFactoryDelegate(string pageKey, string displayText, string sessionToken);

    public static class ShellPageFactory
    {
        public static DashboardPagePanel CreateDashboard(
            string greeting,
            params DashboardCardModel[] cards)
        {
            var dashboard = new DashboardPagePanel();
            dashboard.SetGreeting(greeting, cards);
            return dashboard;
        }

        public static void RegisterModulePages(
            ApplicationShellPanel shell,
            IEnumerable<NavigationItemDefinition> items,
            string dashboardPageKey,
            DashboardPagePanel dashboard,
            PageFactoryDelegate pageFactory = null)
        {
            shell.RegisterPage(dashboardPageKey, dashboard, "Dashboard");

            foreach (var item in items)
            {
                if (item.AlwaysVisible && string.Equals(item.PageKey, dashboardPageKey))
                    continue;

                var page = pageFactory != null
                    ? pageFactory(item.PageKey, item.DisplayText, "session-placeholder")
                    : CreatePlaceholderPage(item.PageKey, item.DisplayText);
                shell.RegisterPage(item.PageKey, page, item.DisplayText);
            }
        }

        public static UserControl CreatePlaceholderPage(string pageKey, string displayText)
        {
            var placeholder = new ModulePagePanel();
            placeholder.SetPage(displayText, pageKey);
            return placeholder;
        }
    }
}
