using System;
using System.Drawing;
using System.Windows.Forms;

using IUIS.SharedUI;
using IUIS.SharedUI.Theme;

namespace IUIS.AdminApp.Forms.Startup
{
    internal sealed partial class BootstrapResultDialog : Form
    {
        public BootstrapResultDialog(string loginId, string temporaryPassword)
        {
            InitializeComponent();

            Text = IUIS.SharedUI.ApplicationIdentity.ProductName + " — Bootstrap Complete";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(560, 320);
            UiTheme.ApplyBaseFormStyle(this);

            title.Font = UiTheme.PageTitleFont;
            message.Font = UiTheme.BodyFont;
            message.ForeColor = UiTheme.TextSecondary;

            message.Text =
                "Save these credentials now. The temporary password is shown only once.\r\n\r\n" +
                "Login ID: " + loginId + "\r\n" +
                "Temporary Password: " + temporaryPassword + "\r\n\r\n" +
                "You must change this password at first sign-in.";

            continueButton.Click += continueButton_Click;
        }

        private void continueButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
