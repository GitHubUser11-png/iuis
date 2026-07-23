using System;
using System.Windows.Forms;
using IUIS.SharedUI.Theme;

namespace IUIS.SharedUI.Forms
{
    public class AppFormBase : Form
    {
        protected LoadingOverlayPanel? _loadingOverlay;

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
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            SetupLoadingOverlay();
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

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Escape && this is Form form && form.DialogResult == DialogResult.None)
            {
                // Allow Escape to close only if not already processing
                if (_loadingOverlay == null || !_loadingOverlay.Visible)
                {
                    form.DialogResult = DialogResult.Cancel;
                    form.Close();
                }
            }
        }
    }
}
