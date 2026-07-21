using System;
using System.Drawing;
using System.Windows.Forms;

using IUIS.Application.Security;
using IUIS.Domain.Identity;
using IUIS.Infrastructure.Presentation;
using IUIS.SharedUI;
using IUIS.SharedUI.Controls;
using IUIS.SharedUI.Theme;

namespace IUIS.UserApp.Forms
{
    internal sealed class GeneralLoginForm : Form
    {
        private readonly ApplicationRuntime _runtime;
        private readonly StatusBannerPanel _banner;
        private readonly LabeledFieldPanel _loginIdField;
        private readonly PasswordFieldPanel _passwordField;
        private readonly CheckBox _rememberIdCheckBox;
        private readonly Button _signInButton;
        private readonly LoadingOverlayPanel _loadingOverlay;

        public GeneralLoginForm(ApplicationRuntime runtime)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));

            Text = ApplicationIdentity.ProductName + " — Sign In";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(UiMetrics.MinimumWindowWidth, UiMetrics.MinimumWindowHeight);
            ClientSize = new Size(UiMetrics.DefaultWindowWidth, UiMetrics.DefaultWindowHeight);
            UiTheme.ApplyBaseFormStyle(this);

            var brandingPanel = new Panel();
            brandingPanel.Dock = DockStyle.Left;
            brandingPanel.Width = (int)(Width * 0.52);
            brandingPanel.BackColor = UiTheme.InstitutionalPrimary;
            brandingPanel.Padding = new Padding(48, 48, 48, 48);

            var brandingIdentity = new ApplicationIdentityPanel();
            brandingIdentity.Location = new Point(48, 48);
            foreach (Control control in brandingIdentity.Controls)
                control.ForeColor = Color.White;

            var brandingBody = new Label();
            brandingBody.Text =
                "Secure access to academic, administrative, and student service information.\r\n\r\n" +
                "• Shared university records\r\n" +
                "• Role-based access\r\n" +
                "• Synchronized services";
            brandingBody.Font = UiTheme.BodyFont;
            brandingBody.ForeColor = Color.FromArgb(220, 230, 240);
            brandingBody.AutoSize = false;
            brandingBody.Location = new Point(48, 180);
            brandingBody.Size = new Size(420, 160);

            brandingPanel.Controls.Add(brandingIdentity);
            brandingPanel.Controls.Add(brandingBody);

            var loginHost = new Panel();
            loginHost.Dock = DockStyle.Fill;
            loginHost.BackColor = UiTheme.Surface;

            var card = new FormCardPanel();
            card.Location = new Point(48, 72);
            card.Anchor = AnchorStyles.None;

            var title = new Label();
            title.Text = "Welcome back";
            title.Font = UiTheme.PageTitleFont;
            title.AutoSize = true;
            title.Location = new Point(32, 28);

            var subtitle = new Label();
            subtitle.Text = "Sign in using the ID issued by the university system.";
            subtitle.Font = UiTheme.BodyFont;
            subtitle.ForeColor = UiTheme.TextSecondary;
            subtitle.AutoSize = false;
            subtitle.Location = new Point(32, 64);
            subtitle.Size = new Size(400, 40);

            _banner = new StatusBannerPanel();
            _banner.Location = new Point(32, 108);
            _banner.Width = 400;

            _loginIdField = new LabeledFieldPanel("System-Issued ID", true);
            _loginIdField.SetHelpText("e.g. STU-2026-000001 or EMP-2026-000041");
            _loginIdField.Location = new Point(32, 168);

            _passwordField = new PasswordFieldPanel("Password", true);
            _passwordField.Location = new Point(32, 260);

            _rememberIdCheckBox = new CheckBox();
            _rememberIdCheckBox.Text = "Remember my ID";
            _rememberIdCheckBox.Font = UiTheme.BodyFont;
            _rememberIdCheckBox.AutoSize = true;
            _rememberIdCheckBox.Location = new Point(32, 340);

            _signInButton = UiTheme.CreatePrimaryButton("Sign In", 160, UiMetrics.StandardButtonHeight);
            _signInButton.Location = new Point(32, 380);
            _signInButton.Click += SignInClick;

            var applyLink = UiTheme.CreateTextAction("Apply for an account");
            applyLink.Location = new Point(32, 440);

            var adminNote = new Label();
            adminNote.Text =
                "Administrator access is available only through the restricted Admin application.";
            adminNote.Font = UiTheme.CaptionFont;
            adminNote.ForeColor = UiTheme.TextSecondary;
            adminNote.AutoSize = false;
            adminNote.Location = new Point(32, 480);
            adminNote.Size = new Size(400, 40);

            card.Controls.Add(title);
            card.Controls.Add(subtitle);
            card.Controls.Add(_banner);
            card.Controls.Add(_loginIdField);
            card.Controls.Add(_passwordField);
            card.Controls.Add(_rememberIdCheckBox);
            card.Controls.Add(_signInButton);
            card.Controls.Add(applyLink);
            card.Controls.Add(adminNote);

            loginHost.Controls.Add(card);
            loginHost.Resize += delegate
            {
                card.Left = Math.Max(16, (loginHost.Width - card.Width) / 2);
            };

            _loadingOverlay = new LoadingOverlayPanel();
            loginHost.Controls.Add(_loadingOverlay);

            Controls.Add(loginHost);
            Controls.Add(brandingPanel);

            AcceptButton = _signInButton;
        }

        public AuthenticationResult AuthenticationResult { get; private set; }

        private void SignInClick(object sender, EventArgs e)
        {
            _banner.HideMessage();
            _loginIdField.ClearError();
            _passwordField.ClearError();

            var loginId = (_loginIdField.InputControl.Text ?? string.Empty).Trim();
            var password = _passwordField.InputControl.Text ?? string.Empty;
            if (string.IsNullOrWhiteSpace(loginId))
            {
                _loginIdField.SetErrorText("System-Issued ID is required.");
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                _passwordField.SetErrorText("Password is required.");
                return;
            }

            _signInButton.Enabled = false;
            _signInButton.Text = "Verifying…";
            _loadingOverlay.ShowOverlay("Verifying your account…");

            try
            {
                var request = new LoginRequest
                {
                    LoginId = loginId,
                    Password = password,
                    ApplicationKind = SessionApplicationKind.UserApplication,
                    LocalIpAddress = null
                };

                AuthenticationResult = _runtime.AuthenticationPresentation.Authenticate(request);
                if (AuthenticationResult == null)
                {
                    _banner.ShowMessage(StatusBannerKind.Error, "Authentication could not be completed.");
                    return;
                }

                if (AuthenticationResult.Status == AuthenticationStatus.AccountLocked)
                {
                    _banner.ShowMessage(
                        StatusBannerKind.Warning,
                        AuthenticationResult.UserMessage);
                    return;
                }

                if (AuthenticationResult.Status == AuthenticationStatus.InvalidCredentials
                    || AuthenticationResult.Status == AuthenticationStatus.AccountInactive)
                {
                    _banner.ShowMessage(
                        StatusBannerKind.Error,
                        "The login ID or password is invalid, or the account is unavailable.");
                    _passwordField.InputControl.Clear();
                    return;
                }

                if (AuthenticationResult.Session != null
                    && AuthenticationResult.Session.PrimaryRole == PrimaryRole.Administrator)
                {
                    _banner.ShowMessage(
                        StatusBannerKind.Error,
                        "Administrator accounts must use the restricted Admin application.");
                    return;
                }

                if (AuthenticationResult.IsAuthenticated)
                {
                    _runtime.CurrentUser.SetSession(
                        AuthenticationResult.Session,
                        AuthenticationResult.Access);
                    DialogResult = DialogResult.OK;
                    Close();
                    return;
                }

                _banner.ShowMessage(StatusBannerKind.Error, AuthenticationResult.UserMessage);
            }
            finally
            {
                _signInButton.Enabled = true;
                _signInButton.Text = "Sign In";
                _loadingOverlay.HideOverlay();
            }
        }
    }
}
