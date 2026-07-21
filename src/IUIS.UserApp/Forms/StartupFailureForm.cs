using System.Drawing;
using System.Windows.Forms;

using IUIS.SharedUI.Theme;

namespace IUIS.UserApp.Forms
{
    internal sealed class StartupFailureForm : Form
    {
        public StartupFailureForm(string failureMessage, bool showRecoveryTools)
        {
            Text = "System could not start safely";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(520, 280);
            UiTheme.ApplyBaseFormStyle(this);

            var title = new Label();
            title.Text = "System could not start safely";
            title.Font = UiTheme.PageTitleFont;
            title.AutoSize = true;
            title.Location = new Point(32, 28);

            var message = new Label();
            message.Text = string.IsNullOrWhiteSpace(failureMessage)
                ? "Required application data is unavailable.\r\nNo records were changed."
                : failureMessage + "\r\nNo records were changed.";
            message.Font = UiTheme.BodyFont;
            message.ForeColor = UiTheme.TextSecondary;
            message.AutoSize = false;
            message.Location = new Point(32, 72);
            message.Size = new Size(456, 72);

            var reference = new Label();
            reference.Text = "Error reference: REP-START-001";
            reference.Font = UiTheme.CaptionFont;
            reference.ForeColor = UiTheme.TextSecondary;
            reference.AutoSize = true;
            reference.Location = new Point(32, 152);

            var retryButton = UiTheme.CreatePrimaryButton("Retry", 120, UiMetrics.StandardButtonHeight);
            retryButton.Location = new Point(248, 200);
            retryButton.Click += delegate
            {
                RetryRequested = true;
                DialogResult = DialogResult.Retry;
                Close();
            };

            var exitButton = UiTheme.CreateSecondaryButton("Exit", 120, UiMetrics.StandardButtonHeight);
            exitButton.Location = new Point(376, 200);
            exitButton.Click += delegate
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };

            Controls.Add(title);
            Controls.Add(message);
            Controls.Add(reference);
            Controls.Add(retryButton);
            Controls.Add(exitButton);

            if (showRecoveryTools)
            {
                var recoveryButton = UiTheme.CreateSecondaryButton("Open Recovery Tools", 180, UiMetrics.StandardButtonHeight);
                recoveryButton.Location = new Point(32, 200);
                Controls.Add(recoveryButton);
            }
        }

        public bool RetryRequested { get; private set; }
    }
}
