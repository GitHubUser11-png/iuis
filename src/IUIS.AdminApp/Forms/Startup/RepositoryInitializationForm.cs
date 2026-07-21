using System;
using System.Drawing;
using System.Windows.Forms;

using IUIS.Infrastructure.Bootstrap;
using IUIS.Infrastructure.Presentation;
using IUIS.SharedUI;
using IUIS.SharedUI.Controls;
using IUIS.SharedUI.Theme;

namespace IUIS.AdminApp.Forms.Startup
{
    internal sealed class RepositoryInitializationForm : Form
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

        public RepositoryInitializationForm(ApplicationRuntime runtime)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));

            Text = ApplicationIdentity.ProductName + " — Repository Initialization";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(720, 680);
            MinimumSize = new Size(720, 680);
            UiTheme.ApplyBaseFormStyle(this);

            var title = new Label();
            title.Text = "Initialize production repository";
            title.Font = UiTheme.PageTitleFont;
            title.AutoSize = true;
            title.Location = new Point(32, 24);

            var subtitle = new Label();
            subtitle.Text =
                "This will create all 49 authoritative JSON repository files and the first administrator account.";
            subtitle.Font = UiTheme.BodyFont;
            subtitle.ForeColor = UiTheme.TextSecondary;
            subtitle.AutoSize = false;
            subtitle.Location = new Point(32, 56);
            subtitle.Size = new Size(640, 48);

            _banner = new StatusBannerPanel();
            _banner.Location = new Point(32, 108);
            _banner.Width = 640;

            _loginIdField = new LabeledFieldPanel("Administrator Login ID", true);
            _loginIdField.Location = new Point(32, 152);

            _passwordField = new LabeledFieldPanel("Temporary Password", true);
            _passwordField.SetHelpText("Minimum 12 characters.");
            _passwordField.Location = new Point(32, 232);

            _givenNameField = new LabeledFieldPanel("Given Name", true);
            _givenNameField.Location = new Point(32, 312);

            _familyNameField = new LabeledFieldPanel("Family Name", true);
            _familyNameField.Location = new Point(360, 312);

            _emailField = new LabeledFieldPanel("Email Address", true);
            _emailField.Location = new Point(32, 392);

            _birthDateField = new LabeledFieldPanel("Birth Date (YYYY-MM-DD)", true);
            _birthDateField.Location = new Point(360, 392);

            _departmentField = new LabeledFieldPanel("Department ID", true);
            _departmentField.Location = new Point(32, 472);

            _positionField = new LabeledFieldPanel("Position Title", true);
            _positionField.Location = new Point(360, 472);

            _submitButton = UiTheme.CreatePrimaryButton("Initialize Repository", 200, UiMetrics.StandardButtonHeight);
            _submitButton.Location = new Point(32, 560);
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

                BootstrapResult = _runtime.Bootstrapper.Initialize(request);
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
