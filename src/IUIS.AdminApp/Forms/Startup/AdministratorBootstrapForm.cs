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
    internal sealed partial class AdministratorBootstrapForm : Form
    {
        private readonly ApplicationRuntime _runtime;

        public AdministratorBootstrapForm(ApplicationRuntime runtime)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            InitializeComponent();

            Text = AppIdentity.ProductName + " — Administrator Bootstrap";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(720, 640);
            MinimumSize = new Size(720, 640);
            UiTheme.ApplyBaseFormStyle(this);

            title.Font = UiTheme.PageTitleFont;
            subtitle.Font = UiTheme.BodyFont;
            subtitle.ForeColor = UiTheme.TextSecondary;

            _passwordField.SetHelpText("Minimum 12 characters. The administrator must change this at first login.");
            _departmentField.SetHelpText("e.g. ADMIN");
            _positionField.SetHelpText("e.g. System Administrator");

            _submitButton = UiTheme.CreatePrimaryButton("Create Administrator", 200, UiMetrics.StandardButtonHeight);
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
