namespace IUIS.UserApp.Forms
{
    partial class StartupCheckForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            if (disposing && (_worker != null))
            {
                _worker.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.identity = new IUIS.SharedUI.Controls.ApplicationIdentityPanel();
            this._statusLabel = new System.Windows.Forms.Label();
            this._progressBar = new System.Windows.Forms.ProgressBar();
            this._versionLabel = new System.Windows.Forms.Label();
            this._worker = new System.ComponentModel.BackgroundWorker();
            this.SuspendLayout();
            // 
            // identity
            // 
            this.identity.Location = new System.Drawing.Point(48, 32);
            this.identity.Name = "identity";
            this.identity.Size = new System.Drawing.Size(200, 100);
            this.identity.TabIndex = 0;
            // 
            // _statusLabel
            // 
            this._statusLabel.Location = new System.Drawing.Point(48, 160);
            this._statusLabel.Name = "_statusLabel";
            this._statusLabel.Size = new System.Drawing.Size(544, 32);
            this._statusLabel.TabIndex = 1;
            this._statusLabel.Text = "Initializing system…";
            // 
            // _progressBar
            // 
            this._progressBar.Location = new System.Drawing.Point(120, 204);
            this._progressBar.Name = "_progressBar";
            this._progressBar.Size = new System.Drawing.Size(400, 18);
            this._progressBar.TabIndex = 2;
            // 
            // _versionLabel
            // 
            this._versionLabel.AutoSize = true;
            this._versionLabel.Location = new System.Drawing.Point(48, 300);
            this._versionLabel.Name = "_versionLabel";
            this._versionLabel.Size = new System.Drawing.Size(45, 20);
            this._versionLabel.TabIndex = 3;
            this._versionLabel.Text = "label3";
            // 
            // StartupCheckForm
            // 
            this.ClientSize = new System.Drawing.Size(640, 360);
            this.Controls.Add(this._versionLabel);
            this.Controls.Add(this._progressBar);
            this.Controls.Add(this._statusLabel);
            this.Controls.Add(this.identity);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "StartupCheckForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private IUIS.SharedUI.Controls.ApplicationIdentityPanel identity;
        private System.Windows.Forms.Label _statusLabel;
        private System.Windows.Forms.ProgressBar _progressBar;
        private System.Windows.Forms.Label _versionLabel;
        private System.ComponentModel.BackgroundWorker _worker;
    }
}
