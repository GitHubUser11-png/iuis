namespace IUIS.UserApp.Forms
{
    partial class StartupFailureForm
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
            this.reference = new System.Windows.Forms.Label();
            this.retryButton = new System.Windows.Forms.Button();
            this.exitButton = new System.Windows.Forms.Button();
            this.recoveryButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // title
            // 
            this.title.AutoSize = true;
            this.title.Location = new System.Drawing.Point(32, 28);
            this.title.Name = "title";
            this.title.Size = new System.Drawing.Size(176, 20);
            this.title.TabIndex = 0;
            this.title.Text = "System could not start safely";
            // 
            // message
            // 
            this.message.Location = new System.Drawing.Point(32, 72);
            this.message.Name = "message";
            this.message.Size = new System.Drawing.Size(456, 72);
            this.message.TabIndex = 1;
            // 
            // reference
            // 
            this.reference.AutoSize = true;
            this.reference.Location = new System.Drawing.Point(32, 152);
            this.reference.Name = "reference";
            this.reference.Size = new System.Drawing.Size(168, 20);
            this.reference.TabIndex = 2;
            this.reference.Text = "Error reference: REP-START-001";
            // 
            // retryButton
            // 
            this.retryButton.Location = new System.Drawing.Point(248, 200);
            this.retryButton.Name = "retryButton";
            this.retryButton.Size = new System.Drawing.Size(120, 36);
            this.retryButton.TabIndex = 3;
            this.retryButton.Text = "Retry";
            this.retryButton.UseVisualStyleBackColor = true;
            // 
            // exitButton
            // 
            this.exitButton.Location = new System.Drawing.Point(376, 200);
            this.exitButton.Name = "exitButton";
            this.exitButton.Size = new System.Drawing.Size(120, 36);
            this.exitButton.TabIndex = 4;
            this.exitButton.Text = "Exit";
            this.exitButton.UseVisualStyleBackColor = true;
            // 
            // recoveryButton
            // 
            this.recoveryButton.Location = new System.Drawing.Point(32, 200);
            this.recoveryButton.Name = "recoveryButton";
            this.recoveryButton.Size = new System.Drawing.Size(180, 36);
            this.recoveryButton.TabIndex = 5;
            this.recoveryButton.Text = "Open Recovery Tools";
            this.recoveryButton.UseVisualStyleBackColor = true;
            this.recoveryButton.Visible = false;
            // 
            // StartupFailureForm
            // 
            this.ClientSize = new System.Drawing.Size(520, 280);
            this.Controls.Add(this.recoveryButton);
            this.Controls.Add(this.exitButton);
            this.Controls.Add(this.retryButton);
            this.Controls.Add(this.reference);
            this.Controls.Add(this.message);
            this.Controls.Add(this.title);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "StartupFailureForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "System could not start safely";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label title;
        private System.Windows.Forms.Label message;
        private System.Windows.Forms.Label reference;
        private System.Windows.Forms.Button retryButton;
        private System.Windows.Forms.Button exitButton;
        private System.Windows.Forms.Button recoveryButton;
    }
}
