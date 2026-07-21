using System;
using System.Drawing;
using System.Windows.Forms;

using IUIS.Infrastructure.Bootstrap;
using IUIS.Infrastructure.Presentation;
using IUIS.SharedUI;
using IUIS.SharedUI.Controls;
using IUIS.SharedUI.Theme;
using AppIdentity = IUIS.SharedUI.ApplicationIdentity;

namespace IUIS.AdminApp.Forms.Startup
{
    internal sealed class AdministratorBootstrapForm : Form
    {
        private readonly ApplicationRuntime _runtime;
        private readonly LabeledFieldPanel _loginIdField;
        private readonly LabeledFieldPanel _passwordField;
        private readonly LabeledFieldPanel _givenNameField;
        private readonly LabeledFieldPanel _familyNameField;
        private readonly LabeledFieldPanel _emailField;
        private readonly LabeledFieldPanel _birthDateField;
        private readonly LabeledFieldPanel _departmentField;
        private readonly LabeledFieldPanel _positionField;
        private readonly StatusBannerPanel _banner;
        private readonly Button _submitButton;

        public AdministratorBootstrapForm(ApplicationRuntime runtime)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));

            Text = AppIdentity.ProductName + " — Administrator Bootstrap";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(720, 640);
            MinimumSize = new Size(720, 640);
            UiTheme.ApplyBaseFormStyle(this);

            var title = new Label();
            title.Text = "Create the first administrator account";
            title.Font = UiTheme.PageTitleFont;
            title.AutoSize = true;
            title.Location = new Point(32, 24);

            var subtitle = new Label();
            subtitle.Text =
                "The repository files exist but no administrator account has been created yet.";
            subtitle.Font = UiTheme.BodyFont;
            subtitle.ForeColor = UiTheme.TextSecondary;
            subtitle.AutoSize = false;
            subtitle.Location = new Point(32, 56);
            subtitle.Size = new Size(640, 40);

            _banner = new StatusBannerPanel();
            _banner.Location = new Point(32, 96);
            _banner.Width = 640;

            _loginIdField = new LabeledFieldPanel("Administrator Login ID", true);
            _loginIdField.Location = new Point(32, 140);

            _passwordField = new LabeledFieldPanel("Temporary Password", true);
            _passwordField.SetHelpText("Minimum 12 characters. The administrator must change this at first login.");
            _passwordField.Location = new Point(32, 220);

            _givenNameField = new LabeledFieldPanel("Given Name", true);
            _givenNameField.Location = new Point(32, 300);

            _familyNameField = new LabeledFieldPanel("Family Name", true);
            _familyNameField.Location = new Point(360, 300);

            _emailField = new LabeledFieldPanel("Email Address", true);
            _emailField.Location = new Point(32, 380);

            _birthDateField = new LabeledFieldPanel("Birth Date (YYYY-MM-DD)", true);
            _birthDateField.Location = new Point(360, 380);

            _departmentField = new LabeledFieldPanel("Department ID", true);
            _departmentField.SetHelpText("e.g. ADMIN");
            _departmentField.Location = new Point(32, 460);

            _positionField = new LabeledFieldPanel("Position Title", true);
            _positionField.SetHelpText("e.g. System Administrator");
            _positionField.Location = new Point(360, 460);

            _submitButton = UiTheme.CreatePrimaryButton("Create Administrator", 200, UiMetrics.StandardButtonHeight);
            _submitButton.Location = new Point(32, 548);
            _submitButton.Click += SubmitClick;

            Controls.Add(title);
            Controls.Add(subtitle);
            Controls.Add(_banner);
            Controls.Add(_loginIdField);
            Controls.Add(_passwordField);
            Controls.Add(_givenNameField);
            Controls.Add(_familyNameField);
            Controls.Add(_emailField);
            Controls.Add(_birthDateField);
            Controls.Add(_departmentField);
            Controls.Add(_positionField);
            Controls.Add(_submitButton);

            AcceptButton = _submitButton;
        }

        public ProductionBootstrapResult BootstrapResult { get; private set; }
        public string TemporaryPassword { get; private set; }
        public string AdministratorLoginId { get; private set; }

        private void SubmitClick(object sender, EventArgs e)
        {
            _banner.HideMessage();

            var loginId = (_loginIdField.InputControl.Text ?? string.Empty).Trim();
            var password = _passwordField.InputControl.Text ?? string.Empty;
            var givenName = (_givenNameField.InputControl.Text ?? string.Empty).Trim();
            var familyName = (_familyNameField.InputControl.Text ?? string.Empty).Trim();
            var email = (_emailField.InputControl.Text ?? string.Empty).Trim();
            var birthDate = (_birthDateField.InputControl.Text ?? string.Empty).Trim();
            var department = (_departmentField.InputControl.Text ?? string.Empty).Trim();
            var position = (_positionField.InputControl.Text ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(loginId)
                || string.IsNullOrWhiteSpace(password)
                || string.IsNullOrWhiteSpace(givenName)
                || string.IsNullOrWhiteSpace(familyName)
                || string.IsNullOrWhiteSpace(email)
                || string.IsNullOrWhiteSpace(birthDate)
                || string.IsNullOrWhiteSpace(department)
                || string.IsNullOrWhiteSpace(position))
            {
                _banner.ShowMessage(StatusBannerKind.Error, "Complete all required fields.");
                return;
            }

            try
            {
                var request = new ProductionBootstrapRequest
                {
                    AdministratorLoginId = loginId,
                    AdministratorInitialPassword = password,
                    AdministratorGivenName = givenName,
                    AdministratorFamilyName = familyName,
                    AdministratorEmailAddress = email,
                    AdministratorMobileNumber = "0000000000",
                    AdministratorAddressLine1 = "Campus Administration",
                    AdministratorCityMunicipality = "Malvar",
                    AdministratorProvince = "Batangas",
                    AdministratorCountryCode = "PH",
                    AdministratorBirthDate = birthDate,
                    DepartmentId = department,
                    PositionTitle = position,
                    BootstrapAtUtc = DateTime.UtcNow
                };

                BootstrapResult = _runtime.Bootstrapper.CompleteBootstrap(request);
                TemporaryPassword = password;
                AdministratorLoginId = loginId;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                _banner.ShowMessage(StatusBannerKind.Error, ex.Message);
            }
        }
    }
}
