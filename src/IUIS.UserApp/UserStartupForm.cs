using System.Drawing;
using System.Windows.Forms;
using IUIS.SharedUI;

namespace IUIS.UserApp
{
    internal sealed partial class UserStartupForm : Form
    {
        public UserStartupForm()
        {
            InitializeComponent();
            
            Text = ApplicationIdentity.ProductName + " - User Application";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(720, 480);
            ClientSize = new Size(960, 640);
        }

        private void detailLabel_Click(object sender, System.EventArgs e)
        {

        }
    }
}
