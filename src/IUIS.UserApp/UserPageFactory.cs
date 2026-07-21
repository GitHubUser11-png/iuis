using System.Windows.Forms;

using IUIS.SharedUI.Shell;

namespace IUIS.UserApp.Forms
{
    public static class UserPageFactory
    {
        public static UserControl CreatePage(string pageKey, string displayText, string sessionToken)
        {
            // TODO: Implement actual page creation when forms are available
            // For now, return placeholder for all pages
            return ShellPageFactory.CreatePlaceholderPage(pageKey, displayText);
        }
    }
}
