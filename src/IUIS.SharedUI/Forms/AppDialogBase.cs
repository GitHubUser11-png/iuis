using System;
using System.ComponentModel;
using System.Windows.Forms;
using IUIS.SharedUI.Theme;
using IUIS.Application.Models;

namespace IUIS.SharedUI.Forms
{
    public class AppDialogBase : Form
    {
        protected LoadingOverlayPanel? _loadingOverlay;
        protected ValidationSummaryControl? _validationSummary;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsBusy { get; private set; }

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
            this.MinimumSize = new Size(400, 300);
            
            // Prevent designer crashes
            if (!DesignMode)
            {
                this.Load += AppDialogBase_Load;
            }
        }

        private void AppDialogBase_Load(object? sender, EventArgs e)
        {
            if (DesignMode) return;
            SetupLoadingOverlay();
            SetupValidationSummary();
        }

        private void SetupLoadingOverlay()
        {
            _loadingOverlay = new LoadingOverlayPanel
            {
                Dock = DockStyle.Fill,
                Visible = false,
                BackColor = UiTheme.OverlayBackground
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
            _validationSummary.SendToBack();
        }

        protected void ShowBusyOverlay()
        {
            if (_loadingOverlay != null && !DesignMode)
            {
                IsBusy = true;
                _loadingOverlay.Visible = true;
                _loadingOverlay.BringToFront();
                this.Cursor = Cursors.WaitCursor;
                this.Enabled = false;
            }
        }

        protected void HideBusyOverlay()
        {
            if (_loadingOverlay != null && !DesignMode)
            {
                IsBusy = false;
                _loadingOverlay.Visible = false;
                this.Cursor = Cursors.Default;
                this.Enabled = true;
            }
        }

        protected void DisplayValidationResult(OperationResult result)
        {
            if (_validationSummary == null || DesignMode) return;

            if (!result.IsSuccess)
            {
                _validationSummary.ShowErrors(result.Errors);
                _validationSummary.Visible = true;
            }
            else
            {
                _validationSummary.Visible = false;
                _validationSummary.Clear();
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Escape && this.DialogResult == DialogResult.None)
            {
                // Allow Escape to close only if not busy
                if (!IsBusy)
                {
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _loadingOverlay?.Dispose();
                _validationSummary?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
