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
    internal sealed partial class RepositoryInitializationForm : Form
    {
        private readonly ApplicationRuntime _runtime;

        public RepositoryInitializationForm(ApplicationRuntime runtime)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            InitializeComponent();

            Text = AppIdentity.ProductName + " — Repository Initialization";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(720, 680);
            MinimumSize = new Size(720, 680);
            UiTheme.ApplyBaseFormStyle(this);

            title.Font = UiTheme.PageTitleFont;
            subtitle.Font = UiTheme.BodyFont;
            subtitle.ForeColor = UiTheme.TextSecondary;

            _passwordField.SetHelpText("Minimum 12 characters.");

            _submitButton = UiTheme.CreatePrimaryButton("Initialize Repository", 200, UiMetrics.StandardButtonHeight);
            _submitButton.Click += SubmitClick;

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
