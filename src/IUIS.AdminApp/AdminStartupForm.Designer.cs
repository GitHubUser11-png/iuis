namespace IUIS.AdminApp
{
    partial class AdminStartupForm
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
            this.statusLabel = new System.Windows.Forms.Label();
            this.detailLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // statusLabel
            // 
            this.statusLabel.AutoSize = true;
            this.statusLabel.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.statusLabel.Location = new System.Drawing.Point(32, 32);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(354, 32);
            this.statusLabel.TabIndex = 0;
            this.statusLabel.Text = "Administrator application solution foundation";
            // 
            // detailLabel
            // 
            this.detailLabel.AutoSize = true;
            this.detailLabel.Location = new System.Drawing.Point(35, 76);
            this.detailLabel.Name = "detailLabel";
            this.detailLabel.Size = new System.Drawing.Size(447, 20);
            this.detailLabel.TabIndex = 1;
            this.detailLabel.Text = "Restricted authentication and operations are intentionally deferred to later passes.";
            // 
            // AdminStartupForm
            // 
            this.ClientSize = new System.Drawing.Size(960, 640);
            this.Controls.Add(this.detailLabel);
            this.Controls.Add(this.statusLabel);
            this.MinimumSize = new System.Drawing.Size(720, 480);
            this.Name = "AdminStartupForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "IUIS - Administrator Application";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.Label detailLabel;
    }
}
