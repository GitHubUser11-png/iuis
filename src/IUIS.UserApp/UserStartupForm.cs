using System.Drawing;
using System.Windows.Forms;
using IUIS.SharedUI;

namespace IUIS.UserApp
{
    internal sealed class UserStartupForm : Form
    {
        public UserStartupForm()
        {
            Text = ApplicationIdentity.ProductName + " - User Application";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(720, 480);
            ClientSize = new Size(960, 640);

            Label statusLabel = new Label();
            statusLabel.AutoSize = true;
            statusLabel.Font = new Font(Font.FontFamily, 14.0f, FontStyle.Bold);
            statusLabel.Location = new Point(32, 32);
            statusLabel.Text = "User application solution foundation";

            Label detailLabel = new Label();
            detailLabel.AutoSize = true;
            detailLabel.Location = new Point(35, 76);
            detailLabel.Text = "Authentication and module routing are intentionally deferred to later passes.";

            Controls.Add(statusLabel);
            Controls.Add(detailLabel);
        }
    }
}
