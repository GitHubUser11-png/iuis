namespace IUIS.AdminApp.Forms.Authentication
{
    partial class AdminLoginForm
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
            this.root = new System.Windows.Forms.TableLayoutPanel();
            this._brandPanel = new System.Windows.Forms.Panel();
            this._contentHost = new System.Windows.Forms.Panel();
            this._card = new System.Windows.Forms.Panel();
            this._loadingOverlay = new IUIS.SharedUI.Controls.LoadingOverlayPanel();
            this.SuspendLayout();
            // 
            // root
            // 
            this.root.ColumnCount = 2;
            this.root.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 38F));
            this.root.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 62F));
            this.root.Dock = System.Windows.Forms.DockStyle.Fill;
            this.root.Location = new System.Drawing.Point(0, 0);
            this.root.Name = "root";
            this.root.RowCount = 1;
            this.root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.root.Size = new System.Drawing.Size(1180, 760);
            this.root.TabIndex = 0;
            // 
            // _brandPanel
            // 
            this._brandPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._brandPanel.Location = new System.Drawing.Point(0, 0);
            this._brandPanel.Name = "_brandPanel";
            this._brandPanel.Size = new System.Drawing.Size(448, 760);
            this._brandPanel.TabIndex = 0;
            // 
            // _contentHost
            // 
            this._contentHost.AutoScroll = true;
            this._contentHost.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(249)))), ((int)(((byte)(250)))), ((int)(((byte)(251)))));
            this._contentHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this._contentHost.Location = new System.Drawing.Point(448, 0);
            this._contentHost.Name = "_contentHost";
            this._contentHost.Padding = new System.Windows.Forms.Padding(28);
            this._contentHost.Size = new System.Drawing.Size(732, 760);
            this._contentHost.TabIndex = 1;
            // 
            // _card
            // 
            this._card.BackColor = System.Drawing.Color.White;
            this._card.Location = new System.Drawing.Point(116, 82);
            this._card.Name = "_card";
            this._card.Size = new System.Drawing.Size(500, 510);
            this._card.TabIndex = 0;
            // 
            // _loadingOverlay
            // 
            this._loadingOverlay.Dock = System.Windows.Forms.DockStyle.Fill;
            this._loadingOverlay.Location = new System.Drawing.Point(0, 0);
            this._loadingOverlay.Name = "_loadingOverlay";
            this._loadingOverlay.Size = new System.Drawing.Size(676, 704);
            this._loadingOverlay.TabIndex = 1;
            // 
            // AdminLoginForm
            // 
            this.ClientSize = new System.Drawing.Size(1180, 760);
            this.Controls.Add(this.root);
            this.MinimumSize = new System.Drawing.Size(880, 660);
            this.Name = "AdminLoginForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel root;
        private System.Windows.Forms.Panel _brandPanel;
        private System.Windows.Forms.Panel _contentHost;
        private System.Windows.Forms.Panel _card;
        private IUIS.SharedUI.Controls.LoadingOverlayPanel _loadingOverlay;
    }
}
