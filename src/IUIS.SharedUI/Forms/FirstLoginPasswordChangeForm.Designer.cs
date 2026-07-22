namespace IUIS.SharedUI.Forms
{
    partial class FirstLoginPasswordChangeForm
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
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.card = new IUIS.SharedUI.Controls.FormCardPanel();
            this.title = new System.Windows.Forms.Label();
            this.subtitle = new System.Windows.Forms.Label();
            this._banner = new IUIS.SharedUI.Controls.StatusBannerPanel();
            this._newPasswordField = new IUIS.SharedUI.Controls.PasswordFieldPanel("New Password", true);
            this._confirmPasswordField = new IUIS.SharedUI.Controls.PasswordFieldPanel("Confirm New Password", true);
            this._submitButton = new System.Windows.Forms.Button();
            this._loadingOverlay = new IUIS.SharedUI.Controls.LoadingOverlayPanel();
            this.SuspendLayout();
            // 
            // card
            // 
            this.card.Location = new System.Drawing.Point(240, 72);
            this.card.Name = "card";
            this.card.Size = new System.Drawing.Size(464, 424);
            this.card.TabIndex = 0;
            // 
            // title
            // 
            this.title.AutoSize = true;
            this.title.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.title.Location = new System.Drawing.Point(32, 28);
            this.title.Name = "title";
            this.title.Size = new System.Drawing.Size(221, 32);
            this.title.TabIndex = 0;
            this.title.Text = "Change your password";
            // 
            // subtitle
            // 
            this.subtitle.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.subtitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.subtitle.Location = new System.Drawing.Point(32, 64);
            this.subtitle.Name = "subtitle";
            this.subtitle.Size = new System.Drawing.Size(400, 48);
            this.subtitle.TabIndex = 1;
            this.subtitle.Text = "Your account requires a new password before you can access university services.";
            // 
            // _banner
            // 
            this._banner.Location = new System.Drawing.Point(32, 120);
            this._banner.Name = "_banner";
            this._banner.Size = new System.Drawing.Size(400, 40);
            this._banner.TabIndex = 2;
            // 
            // _newPasswordField
            // 
            this._newPasswordField.Location = new System.Drawing.Point(32, 176);
            this._newPasswordField.Name = "_newPasswordField";
            this._newPasswordField.Size = new System.Drawing.Size(400, 92);
            this._newPasswordField.TabIndex = 3;
            // 
            // _confirmPasswordField
            // 
            this._confirmPasswordField.Location = new System.Drawing.Point(32, 268);
            this._confirmPasswordField.Name = "_confirmPasswordField";
            this._confirmPasswordField.Size = new System.Drawing.Size(400, 92);
            this._confirmPasswordField.TabIndex = 4;
            // 
            // _submitButton
            // 
            this._submitButton.Location = new System.Drawing.Point(32, 360);
            this._submitButton.Name = "_submitButton";
            this._submitButton.Size = new System.Drawing.Size(180, 36);
            this._submitButton.TabIndex = 5;
            this._submitButton.Text = "Update Password";
            this._submitButton.UseVisualStyleBackColor = true;
            // 
            // _loadingOverlay
            // 
            this._loadingOverlay.Dock = System.Windows.Forms.DockStyle.Fill;
            this._loadingOverlay.Location = new System.Drawing.Point(0, 0);
            this._loadingOverlay.Name = "_loadingOverlay";
            this._loadingOverlay.Size = new System.Drawing.Size(960, 640);
            this._loadingOverlay.TabIndex = 6;
            // 
            // FirstLoginPasswordChangeForm
            // 
            this.ClientSize = new System.Drawing.Size(960, 640);
            this.Controls.Add(this._loadingOverlay);
            this.Controls.Add(this.card);
            this.MinimumSize = new System.Drawing.Size(1024, 768);
            this.Name = "FirstLoginPasswordChangeForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.ResumeLayout(false);

        }

        #endregion

        private IUIS.SharedUI.Controls.FormCardPanel card;
        private System.Windows.Forms.Label title;
        private System.Windows.Forms.Label subtitle;
        private IUIS.SharedUI.Controls.StatusBannerPanel _banner;
        private IUIS.SharedUI.Controls.PasswordFieldPanel _newPasswordField;
        private IUIS.SharedUI.Controls.PasswordFieldPanel _confirmPasswordField;
        private System.Windows.Forms.Button _submitButton;
        private IUIS.SharedUI.Controls.LoadingOverlayPanel _loadingOverlay;
    }
}
