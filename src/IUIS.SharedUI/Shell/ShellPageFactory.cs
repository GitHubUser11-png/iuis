using System.Collections.Generic;

using IUIS.Application.Navigation;
using IUIS.SharedUI.Controls;

namespace IUIS.SharedUI.Shell
{
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
            DashboardPagePanel dashboard)
        {
            shell.RegisterPage(dashboardPageKey, dashboard, "Dashboard");

            foreach (var item in items)
            {
                if (item.AlwaysVisible && string.Equals(item.PageKey, dashboardPageKey))
                    continue;

                var page = new ModulePagePanel();
                page.SetPage(item.DisplayText, item.PageKey);
                shell.RegisterPage(item.PageKey, page, item.DisplayText);
            }
        }
    }
}
