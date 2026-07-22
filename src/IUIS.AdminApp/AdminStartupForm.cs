using System.Drawing;
using System.Windows.Forms;
using IUIS.SharedUI;

namespace IUIS.AdminApp
{
    internal sealed partial class AdminStartupForm : Form
    {
        public AdminStartupForm()
        {
            InitializeComponent();
            
            Text = ApplicationIdentity.ProductName + " - Administrator Application";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(720, 480);
            ClientSize = new Size(960, 640);
        }
    }
}
