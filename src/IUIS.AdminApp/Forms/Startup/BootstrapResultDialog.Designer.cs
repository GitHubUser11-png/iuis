namespace IUIS.AdminApp.Forms.Startup
{
    partial class BootstrapResultDialog
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
            this.title = new System.Windows.Forms.Label();
            this.message = new System.Windows.Forms.Label();
            this.continueButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // title
            // 
            this.title.AutoSize = true;
            this.title.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.title.Location = new System.Drawing.Point(32, 28);
            this.title.Name = "title";
            this.title.Size = new System.Drawing.Size(252, 32);
            this.title.TabIndex = 0;
            this.title.Text = "Administrator account created";
            // 
            // message
            // 
            this.message.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.message.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.message.Location = new System.Drawing.Point(32, 72);
            this.message.Name = "message";
            this.message.Size = new System.Drawing.Size(496, 160);
            this.message.TabIndex = 1;
            this.message.Text = "Save these credentials now. The temporary password is shown only once.\r\n\r\nLogin ID: \r\n" +
    "Temporary Password: \r\n\r\nYou must change this password at first sign-in.";
            // 
            // continueButton
            // 
            this.continueButton.Location = new System.Drawing.Point(348, 252);
            this.continueButton.Name = "continueButton";
            this.continueButton.Size = new System.Drawing.Size(180, 36);
            this.continueButton.TabIndex = 2;
            this.continueButton.Text = "Continue to Sign In";
            this.continueButton.UseVisualStyleBackColor = true;
            // 
            // BootstrapResultDialog
            // 
            this.ClientSize = new System.Drawing.Size(560, 320);
            this.Controls.Add(this.continueButton);
            this.Controls.Add(this.message);
            this.Controls.Add(this.title);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BootstrapResultDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label title;
        private System.Windows.Forms.Label message;
        private System.Windows.Forms.Button continueButton;
    }
}
