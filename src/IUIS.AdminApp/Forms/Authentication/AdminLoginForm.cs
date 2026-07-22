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
        private readonly Panel _brandPanel;
        private readonly Panel _contentHost;
        private readonly Panel _card;
        private StatusBannerPanel _banner;
        private LabeledFieldPanel _loginIdField;
        private PasswordFieldPanel _passwordField;
        private readonly Button _signInButton;
        private readonly LoadingOverlayPanel _loadingOverlay;

        public AdminLoginForm(ApplicationRuntime runtime)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));

            Text = AppIdentity.ProductName + " — Administrator Sign In";
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
                BackColor = UiTheme.InstitutionalDark,
                Padding = new Padding(52)
            };
            var stack = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                BackColor = Color.Transparent
            };
            stack.RowStyles.Add(new RowStyle(SizeType.Absolute, 52f));
            stack.RowStyles.Add(new RowStyle(SizeType.Absolute, 108f));
            stack.RowStyles.Add(new RowStyle(SizeType.Absolute, 1f));
            stack.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            var eyebrow = new Label
            {
                Text = "IUIS  /  SECURE OPERATIONS",
                Dock = DockStyle.Fill,
                Font = UiTheme.FieldLabelFont,
                ForeColor = Color.FromArgb(194, 215, 228),
                TextAlign = ContentAlignment.MiddleLeft
            };
            var title = new Label
            {
                Text = "Administrator\r\naccess portal",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 24f, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.TopLeft
            };
            var divider = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(80, 111, 132) };
            var body = new Label
            {
                Text = "Restricted to authorized university administrators.\r\n\r\n" +
                       "• Environment-validated access\r\n" +
                       "• Role and policy enforcement\r\n" +
                       "• Complete security audit trail\r\n\r\n" +
                       "Do not share credentials or leave an active session unattended.",
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 28, 0, 0),
                Font = UiTheme.BodyFont,
                ForeColor = Color.FromArgb(222, 232, 238),
                TextAlign = ContentAlignment.TopLeft
            };
            stack.Controls.Add(eyebrow, 0, 0);
            stack.Controls.Add(title, 0, 1);
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
                Size = new Size(500, 510),
                Padding = new Padding(38, 32, 38, 28),
                BorderStyle = BorderStyle.FixedSingle
            };
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 8
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42f));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48f));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60f));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 92f));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 90f));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56f));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 1f));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            var title = new Label
            {
                Text = "Administrator sign in",
                Dock = DockStyle.Fill,
                Font = UiTheme.PageTitleFont,
                ForeColor = UiTheme.TextPrimary,
                TextAlign = ContentAlignment.MiddleLeft
            };
            var subtitle = new Label
            {
                Text = "Use your administrator-issued credentials.",
                Dock = DockStyle.Fill,
                Font = UiTheme.BodyFont,
                ForeColor = UiTheme.TextSecondary
            };
            _banner = new StatusBannerPanel { Dock = DockStyle.Fill, Margin = new Padding(0, 4, 0, 8) };
            _loginIdField = new LabeledFieldPanel("Administrator login ID", true) { Dock = DockStyle.Fill };
            _loginIdField.InputControl.AccessibleName = "Administrator login ID";
            _passwordField = new PasswordFieldPanel("Password", true) { Dock = DockStyle.Fill };
            _passwordField.InputControl.AccessibleName = "Administrator password";
            _signInButton = UiTheme.CreatePrimaryButton("Sign in securely", 190, UiMetrics.StandardButtonHeight);
            _signInButton.Dock = DockStyle.Left;
            _signInButton.Click += SignInClick;
            var divider = new Panel { Dock = DockStyle.Fill, BackColor = UiTheme.BorderNeutral };
            var note = new Label
            {
                Text = "Access attempts are recorded. General users must sign in through the User application.",
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 14, 0, 0),
                Font = UiTheme.CaptionFont,
                ForeColor = UiTheme.TextSecondary
            };

            layout.Controls.Add(title, 0, 0);
            layout.Controls.Add(subtitle, 0, 1);
            layout.Controls.Add(_banner, 0, 2);
            layout.Controls.Add(_loginIdField, 0, 3);
            layout.Controls.Add(_passwordField, 0, 4);
            layout.Controls.Add(_signInButton, 0, 5);
            layout.Controls.Add(divider, 0, 6);
            layout.Controls.Add(note, 0, 7);
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
            _card.Width = Math.Min(500, Math.Max(430, availableWidth));
            _card.Left = Math.Max(_contentHost.Padding.Left, (_contentHost.ClientSize.Width - _card.Width) / 2);
            _card.Top = Math.Max(_contentHost.Padding.Top, (_contentHost.ClientSize.Height - _card.Height) / 2);
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
                _loginIdField.SetErrorText("Administrator login ID is required.");
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
            _loadingOverlay.ShowOverlay("Verifying administrator access…");
            try
            {
                var addresses = _runtime.NetworkContext.GetActiveIpv4Addresses();
                AuthenticationResult = _runtime.AuthenticationPresentation.Authenticate(new LoginRequest
                {
                    LoginId = loginId,
                    Password = password,
                    ApplicationKind = SessionApplicationKind.AdministratorApplication,
                    LocalIpAddress = addresses == null ? null : addresses.FirstOrDefault()
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
                if (!AuthenticationResult.IsAuthenticated)
                {
                    _banner.ShowMessage(StatusBannerKind.Error, "The login ID or password is invalid, or the account is unavailable.");
                    _passwordField.InputControl.Clear();
                    _passwordField.InputControl.Focus();
                    return;
                }
                if (AuthenticationResult.Session.PrimaryRole != PrimaryRole.Administrator)
                {
                    _banner.ShowMessage(StatusBannerKind.Error, "Only administrator accounts may sign in to this application.");
                    return;
                }
                DialogResult = DialogResult.OK;
                Close();
            }
            finally
            {
                _signInButton.Enabled = true;
                _signInButton.Text = "Sign in securely";
                _loadingOverlay.HideOverlay();
            }
        }
    }
}
