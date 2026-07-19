using System.Drawing;
using System.Windows.Forms;
using IUIS.SharedUI;

namespace IUIS.AdminApp
{
    internal sealed class AdminStartupForm : Form
    {
        public AdminStartupForm()
        {
            Text = ApplicationIdentity.ProductName + " - Administrator Application";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(720, 480);
            ClientSize = new Size(960, 640);

            Label statusLabel = new Label();
            statusLabel.AutoSize = true;
            statusLabel.Font = new Font(Font.FontFamily, 14.0f, FontStyle.Bold);
            statusLabel.Location = new Point(32, 32);
            statusLabel.Text = "Administrator application solution foundation";

            Label detailLabel = new Label();
            detailLabel.AutoSize = true;
            detailLabel.Location = new Point(35, 76);
            detailLabel.Text = "Restricted authentication and operations are intentionally deferred to later passes.";

            Controls.Add(statusLabel);
            Controls.Add(detailLabel);
        }
    }
}
