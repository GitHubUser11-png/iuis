using System.Windows.Forms;

namespace IUIS.SharedUI.Forms
{
    public class AppDialogBase : Form
    {
        public AppDialogBase()
        {
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ShowInTaskbar = false;
        }
    }
}
