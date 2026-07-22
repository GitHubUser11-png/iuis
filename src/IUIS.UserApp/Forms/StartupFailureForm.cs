using System.Drawing;
using System.Windows.Forms;

using IUIS.SharedUI.Theme;

namespace IUIS.UserApp.Forms
{
    internal sealed partial class StartupFailureForm : Form
    {
        public StartupFailureForm(string failureMessage, bool showRecoveryTools)
        {
            InitializeComponent();
            
            Text = "System could not start safely";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(520, 280);
            UiTheme.ApplyBaseFormStyle(this);

            title.Font = UiTheme.PageTitleFont;
            message.Font = UiTheme.BodyFont;
            message.ForeColor = UiTheme.TextSecondary;
            reference.Font = UiTheme.CaptionFont;
            reference.ForeColor = UiTheme.TextSecondary;

            message.Text = string.IsNullOrWhiteSpace(failureMessage)
                ? "Required application data is unavailable.\r\nNo records were changed."
                : failureMessage + "\r\nNo records were changed.";

            retryButton = UiTheme.CreatePrimaryButton("Retry", 120, UiMetrics.StandardButtonHeight);
            retryButton.Click += delegate
            {
                RetryRequested = true;
                DialogResult = DialogResult.Retry;
                Close();
            };

            exitButton = UiTheme.CreateSecondaryButton("Exit", 120, UiMetrics.StandardButtonHeight);
            exitButton.Click += delegate
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };

            recoveryButton = UiTheme.CreateSecondaryButton("Open Recovery Tools", 180, UiMetrics.StandardButtonHeight);
            recoveryButton.Visible = showRecoveryTools;
        }

        public bool RetryRequested { get; private set; }
    }
}
