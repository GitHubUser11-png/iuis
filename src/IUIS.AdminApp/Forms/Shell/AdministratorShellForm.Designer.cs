namespace IUIS.AdminApp.Forms.Shell
{
    partial class AdministratorShellForm
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
            if (disposing && (_sessionTimer != null))
            {
                _sessionTimer.Dispose();
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
            this._shell = new IUIS.SharedUI.Controls.ApplicationShellPanel();
            this._sessionTimer = new System.Windows.Forms.Timer();
            this.SuspendLayout();
            // 
            // _shell
            // 
            this._shell.Dock = System.Windows.Forms.DockStyle.Fill;
            this._shell.Location = new System.Drawing.Point(0, 0);
            this._shell.Name = "_shell";
            this._shell.Size = new System.Drawing.Size(1280, 720);
            this._shell.TabIndex = 0;
            // 
            // _sessionTimer
            // 
            this._sessionTimer.Interval = 60000;
            // 
            // AdministratorShellForm
            // 
            this.ClientSize = new System.Drawing.Size(1280, 720);
            this.Controls.Add(this._shell);
            this.MinimumSize = new System.Drawing.Size(1024, 768);
            this.Name = "AdministratorShellForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.ResumeLayout(false);

        }

        #endregion

        private IUIS.SharedUI.Controls.ApplicationShellPanel _shell;
        private System.Windows.Forms.Timer _sessionTimer;
    }
}
