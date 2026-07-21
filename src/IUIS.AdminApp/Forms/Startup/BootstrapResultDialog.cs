using System.Drawing;
using System.Windows.Forms;

using IUIS.SharedUI;
using IUIS.SharedUI.Theme;

namespace IUIS.AdminApp.Forms.Startup
{
    internal sealed class BootstrapResultDialog : Form
    {
        public BootstrapResultDialog(string loginId, string temporaryPassword)
        {
            Text = ApplicationIdentity.ProductName + " — Bootstrap Complete";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(560, 320);
            UiTheme.ApplyBaseFormStyle(this);

            var title = new Label();
            title.Text = "Administrator account created";
            title.Font = UiTheme.PageTitleFont;
            title.AutoSize = true;
            title.Location = new Point(32, 28);

            var message = new Label();
            message.Text =
                "Save these credentials now. The temporary password is shown only once.\r\n\r\n" +
                "Login ID: " + loginId + "\r\n" +
                "Temporary Password: " + temporaryPassword + "\r\n\r\n" +
                "You must change this password at first sign-in.";
            message.Font = UiTheme.BodyFont;
            message.ForeColor = UiTheme.TextSecondary;
            message.AutoSize = false;
            message.Location = new Point(32, 72);
            message.Size = new Size(496, 160);

            var continueButton = UiTheme.CreatePrimaryButton("Continue to Sign In", 180, UiMetrics.StandardButtonHeight);
            continueButton.Location = new Point(348, 252);
            continueButton.Click += delegate
            {
                DialogResult = DialogResult.OK;
                Close();
            };

            Controls.Add(title);
            Controls.Add(message);
            Controls.Add(continueButton);
        }
    }
}
