using System;
using System.Windows.Forms;

namespace IUIS.UserApp
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new UserStartupForm());
        }
    }
}
