using System;
using System.Drawing;
using System.Net.Mail;
using System.Windows.Forms;

using IUIS.SharedUI.Controls;
using IUIS.SharedUI.Theme;

namespace IUIS.UserApp.Forms
{
    internal sealed partial class AccountRequestForm : Form
    {
        public AccountRequestForm()
        {
            InitializeComponent();

            Text = "Apply for an account";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(560, 590);
            UiTheme.ApplyBaseFormStyle(this);

            var title = new Label
            {
                Text = "Request university access",
                Dock = DockStyle.Fill,
                Font = UiTheme.PageTitleFont,
                ForeColor = UiTheme.TextPrimary
            };
            var subtitle = new Label
            {
                Text = "Submit your details for registrar or HR verification. An account is issued only after approval.",
                Dock = DockStyle.Fill,
                Font = UiTheme.BodyFont,
                ForeColor = UiTheme.TextSecondary
            };
            _nameField = new LabeledFieldPanel("Full name", true) { Dock = DockStyle.Fill };
            _emailField = new LabeledFieldPanel("Email address", true) { Dock = DockStyle.Fill };

            var typePanel = new Panel { Dock = DockStyle.Fill };
            var typeLabel = new Label
            {
                Text = "Account type *",
                Dock = DockStyle.Top,
                Height = 24,
                Font = UiTheme.FieldLabelFont,
                ForeColor = UiTheme.TextPrimary
            };
            _requestType = new ComboBox
            {
                Dock = DockStyle.Top,
                Height = UiMetrics.StandardFieldHeight,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = UiTheme.BodyFont
            };
            _requestType.Items.AddRange(new object[] { "Student", "Faculty", "Staff" });
            typePanel.Controls.Add(_requestType);
            typePanel.Controls.Add(typeLabel);

            var privacy = new Label
            {
                Text = "Your information is used only to verify eligibility and create your university account.",
                Dock = DockStyle.Fill,
                Font = UiTheme.CaptionFont,
                ForeColor = UiTheme.TextSecondary
            };
            var actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false
            };
            var submit = UiTheme.CreatePrimaryButton("Submit request", 150, UiMetrics.StandardButtonHeight);
            submit.Click += SubmitClick;
            var cancel = UiTheme.CreateSecondaryButton("Cancel", 100, UiMetrics.StandardButtonHeight);
            cancel.DialogResult = DialogResult.Cancel;
            actions.Controls.Add(submit);
            actions.Controls.Add(cancel);

            layout.Controls.Add(title, 0, 0);
            layout.Controls.Add(subtitle, 0, 1);
            layout.Controls.Add(_banner, 0, 2);
            layout.Controls.Add(_nameField, 0, 3);
            layout.Controls.Add(_emailField, 0, 4);
            layout.Controls.Add(typePanel, 0, 5);
            layout.Controls.Add(privacy, 0, 6);
            layout.Controls.Add(actions, 0, 7);
            Controls.Add(layout);

            AcceptButton = submit;
            CancelButton = cancel;
        }

        private void SubmitClick(object sender, EventArgs e)
        {
            _banner.HideMessage();
            _nameField.ClearError();
            _emailField.ClearError();

            var name = (_nameField.InputControl.Text ?? string.Empty).Trim();
            var email = (_emailField.InputControl.Text ?? string.Empty).Trim();
            if (name.Length < 3)
            {
                _nameField.SetErrorText("Enter your complete name.");
                _nameField.InputControl.Focus();
                return;
            }
            if (!IsValidEmail(email))
            {
                _emailField.SetErrorText("Enter a valid email address.");
                _emailField.InputControl.Focus();
                return;
            }
            if (_requestType.SelectedIndex < 0)
            {
                _banner.ShowMessage(StatusBannerKind.Error, "Select the account type you need.");
                _requestType.Focus();
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private static bool IsValidEmail(string value)
        {
            try
            {
                return !string.IsNullOrWhiteSpace(value) && new MailAddress(value).Address == value;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
