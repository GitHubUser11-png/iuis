using System.Collections.Generic;

using IUIS.Application.Security;

namespace IUIS.Application.Navigation
{
    public static class AdministratorNavigationCatalog
    {
        public static IReadOnlyList<NavigationItemDefinition> GetAll()
        {
            return new List<NavigationItemDefinition>
            {
                Item("admin.dashboard", "Home", "Dashboard", "ADM-DASH-01", string.Empty, 10, true),
                Item("admin.applications", "Identity", "Account Applications", "ADM-APP-01", PermissionCatalog.Keys.AdminApplicationReview, 100),
                Item("admin.accounts", "Identity", "User Accounts", "ADM-USR-01", PermissionCatalog.Keys.AdminAccountManage, 110),
                Item("admin.permissions", "Identity", "Permission Profiles", "ADM-PERM-01", PermissionCatalog.Keys.AdminSecurityManage, 120),
                Item("admin.login.activity", "Security", "Login Activity", "ADM-LOG-01", PermissionCatalog.Keys.AdminSecurityManage, 200),
                Item("admin.security", "Security", "Security Policy", "ADM-SEC-01", PermissionCatalog.Keys.AdminSecurityManage, 210),
                Item("admin.repository", "Operations", "Repository Health", "ADM-REP-01", PermissionCatalog.Keys.AdminRepositoryManage, 300),
                Item("admin.audit", "Operations", "Audit Logs", "ADM-AUD-01", PermissionCatalog.Keys.AdminAuditView, 310),
                Item("admin.settings", "Operations", "System Settings", "ADM-SET-01", PermissionCatalog.Keys.AdminSettingsManage, 320),
                Item("admin.reports", "Operations", "Reports", "ADM-RPT-01", PermissionCatalog.Keys.AdminReportRun, 330)
            };
        }

        private static NavigationItemDefinition Item(
            string key,
            string group,
            string text,
            string pageKey,
            string permission,
            int order,
            bool alwaysVisible = false)
        {
            return new NavigationItemDefinition
            {
                Key = key,
                GroupKey = group,
                DisplayText = text,
                PageKey = pageKey,
                RequiredPermission = permission,
                DisplayOrder = order,
                AlwaysVisible = alwaysVisible
            };
        }
    }
}
