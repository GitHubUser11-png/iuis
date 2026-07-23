using System;
using System.Windows.Forms;
using IUIS.SharedUI.Theme;
using IUIS.Application.Models;

namespace IUIS.SharedUI.Forms
{
    public class AppDialogBase : Form
    {
        protected LoadingOverlayPanel? _loadingOverlay;
        protected ValidationSummaryControl? _validationSummary;

        public AppDialogBase()
        {
            InitializeBaseDialog();
        }

        private void InitializeBaseDialog()
        {
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ShowInTaskbar = false;
            this.Font = UiTheme.BodyFont;
            this.BackColor = UiTheme.Surface;
            this.ForeColor = UiTheme.TextPrimary;
            this.AutoScaleMode = AutoScaleMode.Dpi;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            SetupLoadingOverlay();
            SetupValidationSummary();
        }

        private void SetupLoadingOverlay()
        {
            _loadingOverlay = new LoadingOverlayPanel
            {
                Dock = DockStyle.Fill,
                Visible = false
            };
            Controls.Add(_loadingOverlay);
            _loadingOverlay.BringToFront();
        }

        private void SetupValidationSummary()
        {
            _validationSummary = new ValidationSummaryControl
            {
                Dock = DockStyle.Top,
                Visible = false
            };
            Controls.Add(_validationSummary);
            _validationSummary.BringToFront();
        }

        protected void ShowBusyOverlay()
        {
            if (_loadingOverlay != null)
            {
                _loadingOverlay.Visible = true;
                _loadingOverlay.BringToFront();
                this.Cursor = Cursors.WaitCursor;
            }
        }

        protected void HideBusyOverlay()
        {
            if (_loadingOverlay != null)
            {
                _loadingOverlay.Visible = false;
                this.Cursor = Cursors.Default;
            }
        }

        protected void DisplayValidationErrors(OperationResult result)
        {
            if (_validationSummary != null && !result.IsSuccess)
            {
                _validationSummary.SetErrors(result.Errors);
                _validationSummary.Visible = true;
            }
        }

        protected void ClearValidationErrors()
        {
            if (_validationSummary != null)
            {
                _validationSummary.ClearErrors();
                _validationSummary.Visible = false;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Escape && this.DialogResult == DialogResult.None)
            {
                // Allow Escape to close only if not already processing
                if (_loadingOverlay == null || !_loadingOverlay.Visible)
                {
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                }
            }
        }
    }
}
