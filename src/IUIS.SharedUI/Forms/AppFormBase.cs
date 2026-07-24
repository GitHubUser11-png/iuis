using System;
using System.ComponentModel;
using System.Windows.Forms;
using IUIS.Application.Models;
using IUIS.SharedUI.Theme;

namespace IUIS.SharedUI.Forms
{
    public class AppFormBase : Form
    {
        protected LoadingOverlayPanel? _loadingOverlay;
        protected ValidationSummaryControl? _validationSummary;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsBusy { get; private set; }

        public AppFormBase()
        {
            InitializeBaseForm();
        }

        private void InitializeBaseForm()
        {
            this.Font = UiTheme.BodyFont;
            this.BackColor = UiTheme.Surface;
            this.ForeColor = UiTheme.TextPrimary;
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.KeyPreview = true;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(800, 600);
            
            // Prevent designer crashes
            if (!DesignMode)
            {
                this.Load += AppFormBase_Load;
            }
        }

        private void AppFormBase_Load(object? sender, EventArgs e)
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
            if (e.KeyCode == Keys.Escape && this is Form form && form.DialogResult == DialogResult.None)
            {
                // Allow Escape to close only if not busy
                if (!IsBusy)
                {
                    form.DialogResult = DialogResult.Cancel;
                    form.Close();
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
