using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using IUIS.Application.Security;
using IUIS.Domain.Identity;
using IUIS.Infrastructure.Presentation;
using IUIS.SharedUI;
using IUIS.SharedUI.Controls;
using IUIS.SharedUI.Theme;
using AppIdentity = IUIS.SharedUI.ApplicationIdentity;

namespace IUIS.AdminApp.Forms.Authentication
{
    internal sealed class AdminLoginForm : Form
    {
        private readonly ApplicationRuntime _runtime;
        private readonly StatusBannerPanel _banner;
        private readonly LabeledFieldPanel _loginIdField;
        private readonly PasswordFieldPanel _passwordField;
        private readonly Button _signInButton;
        private readonly LoadingOverlayPanel _loadingOverlay;

        public AdminLoginForm(ApplicationRuntime runtime)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));

            Text = AppIdentity.ProductName + " — Administrator Sign In";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(UiMetrics.MinimumWindowWidth, UiMetrics.MinimumWindowHeight);
            ClientSize = new Size(UiMetrics.DefaultWindowWidth, UiMetrics.DefaultWindowHeight);
            UiTheme.ApplyBaseFormStyle(this);

            var brandingPanel = new Panel();
            brandingPanel.Dock = DockStyle.Left;
            brandingPanel.Width = (int)(Width * 0.52);
            brandingPanel.BackColor = UiTheme.Restricted;
            brandingPanel.Padding = new Padding(48);

            var brandingTitle = new Label();
            brandingTitle.Text = "Restricted Administrator Access";
            brandingTitle.Font = UiTheme.ApplicationTitleFont;
            brandingTitle.ForeColor = Color.White;
            brandingTitle.AutoSize = false;
            brandingTitle.Location = new Point(48, 48);
            brandingTitle.Size = new Size(420, 48);

            var brandingBody = new Label();
            brandingBody.Text =
                "This application is restricted to authorized administrators.\r\n\r\n" +
                "• Separate from the general user login\r\n" +
                "• Environment-validated access\r\n" +
                "• Full audit trail";
            brandingBody.Font = UiTheme.BodyFont;
            brandingBody.ForeColor = Color.FromArgb(230, 220, 240);
            brandingBody.AutoSize = false;
            brandingBody.Location = new Point(48, 112);
            brandingBody.Size = new Size(420, 120);

            brandingPanel.Controls.Add(brandingTitle);
            brandingPanel.Controls.Add(brandingBody);

            var loginHost = new Panel();
            loginHost.Dock = DockStyle.Fill;
            loginHost.BackColor = UiTheme.Surface;

            var card = new FormCardPanel();
            card.Location = new Point(48, 72);

            var title = new Label();
            title.Text = "Administrator Sign In";
            title.Font = UiTheme.PageTitleFont;
            title.AutoSize = true;
            title.Location = new Point(32, 28);

            var subtitle = new Label();
            subtitle.Text = "Sign in using your administrator-issued credentials.";
            subtitle.Font = UiTheme.BodyFont;
            subtitle.ForeColor = UiTheme.TextSecondary;
            subtitle.AutoSize = false;
            subtitle.Location = new Point(32, 64);
            subtitle.Size = new Size(400, 40);

            _banner = new StatusBannerPanel();
            _banner.Location = new Point(32, 108);
            _banner.Width = 400;

            _loginIdField = new LabeledFieldPanel("Administrator Login ID", true);
            _loginIdField.Location = new Point(32, 168);

            _passwordField = new PasswordFieldPanel("Password", true);
            _passwordField.Location = new Point(32, 260);

            _signInButton = UiTheme.CreatePrimaryButton("Sign In", 160, UiMetrics.StandardButtonHeight);
            _signInButton.Location = new Point(32, 340);
            _signInButton.Click += SignInClick;

            card.Controls.Add(title);
            card.Controls.Add(subtitle);
            card.Controls.Add(_banner);
            card.Controls.Add(_loginIdField);
            card.Controls.Add(_passwordField);
            card.Controls.Add(_signInButton);

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
                _loginIdField.SetErrorText("Login ID is required.");
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                _passwordField.SetErrorText("Password is required.");
                return;
            }

            _signInButton.Enabled = false;
            _loadingOverlay.ShowOverlay("Verifying administrator access…");

            try
            {
                var addresses = _runtime.NetworkContext.GetActiveIpv4Addresses();
                var request = new LoginRequest
                {
                    LoginId = loginId,
                    Password = password,
                    ApplicationKind = SessionApplicationKind.AdministratorApplication,
                    LocalIpAddress = addresses == null ? null : addresses.FirstOrDefault()
                };

                AuthenticationResult = _runtime.AuthenticationPresentation.Authenticate(request);
                if (AuthenticationResult == null)
                {
                    _banner.ShowMessage(StatusBannerKind.Error, "Authentication could not be completed.");
                    return;
                }

                if (AuthenticationResult.Status == AuthenticationStatus.AccountLocked)
                {
                    _banner.ShowMessage(StatusBannerKind.Warning, AuthenticationResult.UserMessage);
                    return;
                }

                if (!AuthenticationResult.IsAuthenticated)
                {
                    _banner.ShowMessage(
                        StatusBannerKind.Error,
                        "The login ID or password is invalid, or the account is unavailable.");
                    _passwordField.InputControl.Clear();
                    return;
                }

                if (AuthenticationResult.Session.PrimaryRole != PrimaryRole.Administrator)
                {
                    _banner.ShowMessage(
                        StatusBannerKind.Error,
                        "Only administrator accounts may sign in to this application.");
                    return;
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            finally
            {
                _signInButton.Enabled = true;
                _loadingOverlay.HideOverlay();
            }
        }
    }
}
