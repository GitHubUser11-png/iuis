using System;
using System.Drawing;
using System.Windows.Forms;

using IUIS.Application.Abstractions.Security;
using IUIS.Application.Security;
using IUIS.SharedUI.Controls;
using IUIS.SharedUI.Theme;

namespace IUIS.SharedUI.Forms
{
    public sealed class FirstLoginPasswordChangeForm : Form
    {
        private readonly IFirstLoginPasswordChangeService _passwordChange;
        private readonly SessionCredential _credential;
        private readonly StatusBannerPanel _banner;
        private readonly PasswordFieldPanel _newPasswordField;
        private readonly PasswordFieldPanel _confirmPasswordField;
        private readonly Button _submitButton;
        private readonly LoadingOverlayPanel _loadingOverlay;

        public FirstLoginPasswordChangeForm(
            IFirstLoginPasswordChangeService passwordChange,
            UserSession session)
        {
            if (passwordChange == null)
                throw new ArgumentNullException(nameof(passwordChange));
            if (session == null)
                throw new ArgumentNullException(nameof(session));

            _passwordChange = passwordChange;
            _credential = session.ToCredential();

            Text = ApplicationIdentity.ProductName + " — Change Password";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(UiMetrics.MinimumWindowWidth, UiMetrics.MinimumWindowHeight);
            ClientSize = new Size(960, 640);
            UiTheme.ApplyBaseFormStyle(this);

            var card = new FormCardPanel();
            card.Location = new Point(240, 72);

            var title = new Label();
            title.Text = "Change your password";
            title.Font = UiTheme.PageTitleFont;
            title.AutoSize = true;
            title.Location = new Point(32, 28);

            var subtitle = new Label();
            subtitle.Text =
                "Your account requires a new password before you can access university services.";
            subtitle.Font = UiTheme.BodyFont;
            subtitle.ForeColor = UiTheme.TextSecondary;
            subtitle.AutoSize = false;
            subtitle.Location = new Point(32, 64);
            subtitle.Size = new Size(400, 48);

            _banner = new StatusBannerPanel();
            _banner.Location = new Point(32, 120);
            _banner.Width = 400;

            _newPasswordField = new PasswordFieldPanel("New Password", true);
            _newPasswordField.SetHelpText("At least 12 characters with mixed character types.");
            _newPasswordField.Location = new Point(32, 176);

            _confirmPasswordField = new PasswordFieldPanel("Confirm New Password", true);
            _confirmPasswordField.Location = new Point(32, 268);

            _submitButton = UiTheme.CreatePrimaryButton("Update Password", 180, UiMetrics.StandardButtonHeight);
            _submitButton.Location = new Point(32, 360);
            _submitButton.Click += SubmitClick;

            card.Controls.Add(title);
            card.Controls.Add(subtitle);
            card.Controls.Add(_banner);
            card.Controls.Add(_newPasswordField);
            card.Controls.Add(_confirmPasswordField);
            card.Controls.Add(_submitButton);

            _loadingOverlay = new LoadingOverlayPanel();
            Controls.Add(_loadingOverlay);
            Controls.Add(card);

            AcceptButton = _submitButton;
        }

        public PasswordChangeResult ChangeResult { get; private set; }

        private void SubmitClick(object sender, EventArgs e)
        {
            _banner.HideMessage();
            _newPasswordField.ClearError();
            _confirmPasswordField.ClearError();

            var newPassword = _newPasswordField.InputControl.Text ?? string.Empty;
            var confirmPassword = _confirmPasswordField.InputControl.Text ?? string.Empty;

            if (string.IsNullOrEmpty(newPassword))
            {
                _newPasswordField.SetErrorText("A new password is required.");
                return;
            }

            if (newPassword.Length < 12)
            {
                _newPasswordField.SetErrorText("Password must be at least 12 characters.");
                return;
            }

            if (newPassword != confirmPassword)
            {
                _confirmPasswordField.SetErrorText("Passwords do not match.");
                return;
            }

            _submitButton.Enabled = false;
            _loadingOverlay.ShowOverlay("Updating your password…");

            try
            {
                ChangeResult = _passwordChange.ChangePassword(new PasswordChangeRequest
                {
                    Credential = _credential,
                    NewPassword = newPassword,
                    ConfirmPassword = confirmPassword
                });

                if (ChangeResult != null && ChangeResult.Succeeded)
                {
                    DialogResult = DialogResult.OK;
                    Close();
                    return;
                }

                _banner.ShowMessage(
                    StatusBannerKind.Error,
                    ChangeResult == null
                        ? "The password could not be changed."
                        : ChangeResult.UserMessage);
                _newPasswordField.InputControl.Clear();
                _confirmPasswordField.InputControl.Clear();
            }
            finally
            {
                _submitButton.Enabled = true;
                _loadingOverlay.HideOverlay();
            }
        }
    }
}
