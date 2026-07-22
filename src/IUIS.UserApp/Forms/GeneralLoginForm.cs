using System;
using System.Drawing;
using System.Windows.Forms;

using IUIS.Application.Security;
using IUIS.Domain.Identity;
using IUIS.Infrastructure.Presentation;
using IUIS.SharedUI;
using IUIS.SharedUI.Controls;
using IUIS.SharedUI.Theme;
using AppIdentity = IUIS.SharedUI.ApplicationIdentity;

namespace IUIS.UserApp.Forms
{
    internal sealed class GeneralLoginForm : Form
    {
        private readonly ApplicationRuntime _runtime;
        private readonly Panel _brandPanel;
        private readonly Panel _contentHost;
        private readonly Panel _card;
        private StatusBannerPanel _banner;
        private LabeledFieldPanel _loginIdField;
        private PasswordFieldPanel _passwordField;
        private CheckBox _rememberIdCheckBox;
        private Button _signInButton;
        private readonly LoadingOverlayPanel _loadingOverlay;

        public GeneralLoginForm(ApplicationRuntime runtime)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));

            Text = AppIdentity.ProductName + " — Sign In";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(880, 660);
            ClientSize = new Size(1180, 760);
            UiTheme.ApplyBaseFormStyle(this);

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38f));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62f));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            _brandPanel = BuildBrandPanel();
            _contentHost = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UiTheme.Surface,
                AutoScroll = true,
                Padding = new Padding(28)
            };
            _card = BuildSignInCard();
            _contentHost.Controls.Add(_card);

            _loadingOverlay = new LoadingOverlayPanel { Dock = DockStyle.Fill };
            _contentHost.Controls.Add(_loadingOverlay);
            _loadingOverlay.BringToFront();

            root.Controls.Add(_brandPanel, 0, 0);
            root.Controls.Add(_contentHost, 1, 0);
            Controls.Add(root);

            Resize += delegate { ApplyResponsiveLayout(root); };
            Shown += delegate
            {
                ApplyResponsiveLayout(root);
                _loginIdField.InputControl.Focus();
            };
            AcceptButton = _signInButton;
        }

        public AuthenticationResult AuthenticationResult { get; private set; }

        private Panel BuildBrandPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UiTheme.InstitutionalPrimary,
                Padding = new Padding(52)
            };

            var stack = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                BackColor = Color.Transparent
            };
            stack.RowStyles.Add(new RowStyle(SizeType.Absolute, 58f));
            stack.RowStyles.Add(new RowStyle(SizeType.Absolute, 96f));
            stack.RowStyles.Add(new RowStyle(SizeType.Absolute, 1f));
            stack.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            var mark = new Label
            {
                Text = "IUIS",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 28f, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft
            };
            var identity = new Label
            {
                Text = AppIdentity.ProductName + "\r\n" + AppIdentity.CampusSubtitle,
                Dock = DockStyle.Fill,
                Font = UiTheme.ApplicationTitleFont,
                ForeColor = Color.White,
                TextAlign = ContentAlignment.TopLeft
            };
            var divider = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(92, 126, 157) };
            var body = new Label
            {
                Text = "One secure place for your university services.\r\n\r\n" +
                       "ACADEMIC RECORDS\r\nView enrollment, subjects, and assessments.\r\n\r\n" +
                       "CAMPUS SERVICES\r\nAccess library, clinic, counseling, and notices.",
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 28, 0, 0),
                Font = UiTheme.BodyFont,
                ForeColor = Color.FromArgb(225, 234, 241),
                TextAlign = ContentAlignment.TopLeft
            };

            stack.Controls.Add(mark, 0, 0);
            stack.Controls.Add(identity, 0, 1);
            stack.Controls.Add(divider, 0, 2);
            stack.Controls.Add(body, 0, 3);
            panel.Controls.Add(stack);
            return panel;
        }

        private Panel BuildSignInCard()
        {
            var card = new Panel
            {
                BackColor = UiTheme.ElevatedSurface,
                Size = new Size(500, 596),
                Padding = new Padding(38, 32, 38, 28),
                BorderStyle = BorderStyle.FixedSingle
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 10,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42f));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48f));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60f));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 96f));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 88f));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42f));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54f));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42f));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 1f));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            var title = new Label
            {
                Text = "Welcome back",
                Dock = DockStyle.Fill,
                Font = UiTheme.PageTitleFont,
                ForeColor = UiTheme.TextPrimary,
                TextAlign = ContentAlignment.MiddleLeft
            };
            var subtitle = new Label
            {
                Text = "Use the ID issued by the university to continue.",
                Dock = DockStyle.Fill,
                Font = UiTheme.BodyFont,
                ForeColor = UiTheme.TextSecondary,
                TextAlign = ContentAlignment.TopLeft
            };

            _banner = new StatusBannerPanel { Dock = DockStyle.Fill, Margin = new Padding(0, 4, 0, 8) };
            _loginIdField = new LabeledFieldPanel("System-issued ID", true) { Dock = DockStyle.Fill, Margin = Padding.Empty };
            _loginIdField.SetHelpText("Example: STU-2026-000001 or EMP-2026-000041");
            _loginIdField.InputControl.AccessibleName = "System-issued ID";
            _loginIdField.InputControl.TabIndex = 0;

            _passwordField = new PasswordFieldPanel("Password", true) { Dock = DockStyle.Fill, Margin = Padding.Empty };
            _passwordField.InputControl.AccessibleName = "Password";
            _passwordField.InputControl.TabIndex = 1;

            _rememberIdCheckBox = new CheckBox
            {
                Text = "Remember my ID on this computer",
                Dock = DockStyle.Fill,
                Font = UiTheme.BodyFont,
                ForeColor = UiTheme.TextPrimary,
                AutoSize = true,
                TabIndex = 2
            };
            _signInButton = UiTheme.CreatePrimaryButton("Sign in", 180, UiMetrics.StandardButtonHeight);
            _signInButton.Dock = DockStyle.Left;
            _signInButton.TabIndex = 3;
            _signInButton.Click += SignInClick;

            var applyLink = UiTheme.CreateTextAction("Apply for an account");
            applyLink.Dock = DockStyle.Left;
            applyLink.AutoSize = true;
            applyLink.TabIndex = 4;
            applyLink.Click += ApplyForAccountClick;

            var divider = new Panel { Dock = DockStyle.Fill, BackColor = UiTheme.BorderNeutral };
            var note = new Label
            {
                Text = "Administrator accounts must use the restricted Admin application.",
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 14, 0, 0),
                Font = UiTheme.CaptionFont,
                ForeColor = UiTheme.TextSecondary,
                TextAlign = ContentAlignment.TopLeft
            };

            layout.Controls.Add(title, 0, 0);
            layout.Controls.Add(subtitle, 0, 1);
            layout.Controls.Add(_banner, 0, 2);
            layout.Controls.Add(_loginIdField, 0, 3);
            layout.Controls.Add(_passwordField, 0, 4);
            layout.Controls.Add(_rememberIdCheckBox, 0, 5);
            layout.Controls.Add(_signInButton, 0, 6);
            layout.Controls.Add(applyLink, 0, 7);
            layout.Controls.Add(divider, 0, 8);
            layout.Controls.Add(note, 0, 9);
            card.Controls.Add(layout);
            return card;
        }

        private void ApplyResponsiveLayout(TableLayoutPanel root)
        {
            var compact = ClientSize.Width < 980;
            root.ColumnStyles[0].Width = compact ? 0f : 38f;
            root.ColumnStyles[1].Width = compact ? 100f : 62f;
            _brandPanel.Visible = !compact;

            var availableWidth = Math.Max(0, _contentHost.ClientSize.Width - _contentHost.Padding.Horizontal);
            var availableHeight = Math.Max(0, _contentHost.ClientSize.Height - _contentHost.Padding.Vertical);
            _card.Width = Math.Min(500, Math.Max(430, availableWidth));
            _card.Left = Math.Max(_contentHost.Padding.Left, (_contentHost.ClientSize.Width - _card.Width) / 2);
            _card.Top = Math.Max(_contentHost.Padding.Top, (_contentHost.ClientSize.Height - _card.Height) / 2);
        }

        private void ApplyForAccountClick(object sender, EventArgs e)
        {
            using (var dialog = new AccountRequestForm())
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                    _banner.ShowMessage(StatusBannerKind.Success, "Your request was prepared. The registrar will review your information.");
            }
        }

        private void SignInClick(object sender, EventArgs e)
        {
            _banner.HideMessage();
            _loginIdField.ClearError();
            _passwordField.ClearError();

            var loginId = (_loginIdField.InputControl.Text ?? string.Empty).Trim();
            var password = _passwordField.InputControl.Text ?? string.Empty;
            if (string.IsNullOrWhiteSpace(loginId))
            {
                _loginIdField.SetErrorText("System-issued ID is required.");
                _loginIdField.InputControl.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                _passwordField.SetErrorText("Password is required.");
                _passwordField.InputControl.Focus();
                return;
            }

            _signInButton.Enabled = false;
            _signInButton.Text = "Verifying…";
            _loadingOverlay.ShowOverlay("Verifying your account…");
            try
            {
                AuthenticationResult = _runtime.AuthenticationPresentation.Authenticate(new LoginRequest
                {
                    LoginId = loginId,
                    Password = password,
                    ApplicationKind = SessionApplicationKind.UserApplication,
                    LocalIpAddress = null
                });

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
                if (AuthenticationResult.Status == AuthenticationStatus.InvalidCredentials || AuthenticationResult.Status == AuthenticationStatus.AccountInactive)
                {
                    _banner.ShowMessage(StatusBannerKind.Error, "The login ID or password is invalid, or the account is unavailable.");
                    _passwordField.InputControl.Clear();
                    _passwordField.InputControl.Focus();
                    return;
                }
                if (AuthenticationResult.Session != null && AuthenticationResult.Session.PrimaryRole == PrimaryRole.Administrator)
                {
                    _banner.ShowMessage(StatusBannerKind.Error, "Administrator accounts must use the restricted Admin application.");
                    return;
                }
                if (AuthenticationResult.IsAuthenticated)
                {
                    _runtime.CurrentUser.SetSession(AuthenticationResult.Session, AuthenticationResult.Access);
                    DialogResult = DialogResult.OK;
                    Close();
                    return;
                }
                _banner.ShowMessage(StatusBannerKind.Error, AuthenticationResult.UserMessage);
            }
            finally
            {
                _signInButton.Enabled = true;
                _signInButton.Text = "Sign in";
                _loadingOverlay.HideOverlay();
            }
        }
    }
}
