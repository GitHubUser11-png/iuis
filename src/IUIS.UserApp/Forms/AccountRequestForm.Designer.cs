namespace IUIS.UserApp.Forms
{
    partial class AccountRequestForm
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
            this.layout = new System.Windows.Forms.TableLayoutPanel();
            this._nameField = new IUIS.SharedUI.Controls.LabeledFieldPanel();
            this._emailField = new IUIS.SharedUI.Controls.LabeledFieldPanel();
            this._requestType = new System.Windows.Forms.ComboBox();
            this._banner = new IUIS.SharedUI.Controls.StatusBannerPanel();
            this.SuspendLayout();
            // 
            // layout
            // 
            this.layout.ColumnCount = 1;
            this.layout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layout.Location = new System.Drawing.Point(0, 0);
            this.layout.Name = "layout";
            this.layout.Padding = new System.Windows.Forms.Padding(40, 30, 40, 30);
            this.layout.RowCount = 9;
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 54F));
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 58F));
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 92F));
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 92F));
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 78F));
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 56F));
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layout.Size = new System.Drawing.Size(560, 590);
            this.layout.TabIndex = 0;
            // 
            // _nameField
            // 
            this._nameField.Dock = System.Windows.Forms.DockStyle.Fill;
            this._nameField.Location = new System.Drawing.Point(40, 186);
            this._nameField.Name = "_nameField";
            this._nameField.Size = new System.Drawing.Size(480, 92);
            this._nameField.TabIndex = 1;
            // 
            // _emailField
            // 
            this._emailField.Dock = System.Windows.Forms.DockStyle.Fill;
            this._emailField.Location = new System.Drawing.Point(40, 278);
            this._emailField.Name = "_emailField";
            this._emailField.Size = new System.Drawing.Size(480, 92);
            this._emailField.TabIndex = 2;
            // 
            // _requestType
            // 
            this._requestType.Dock = System.Windows.Forms.DockStyle.Top;
            this._requestType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._requestType.FormattingEnabled = true;
            this._requestType.Location = new System.Drawing.Point(40, 370);
            this._requestType.Name = "_requestType";
            this._requestType.Size = new System.Drawing.Size(480, 28);
            this._requestType.TabIndex = 3;
            // 
            // _banner
            // 
            this._banner.Dock = System.Windows.Forms.DockStyle.Fill;
            this._banner.Location = new System.Drawing.Point(40, 128);
            this._banner.Margin = new System.Windows.Forms.Padding(0, 2, 0, 6);
            this._banner.Name = "_banner";
            this._banner.Size = new System.Drawing.Size(480, 58);
            this._banner.TabIndex = 0;
            // 
            // AccountRequestForm
            // 
            this.ClientSize = new System.Drawing.Size(560, 590);
            this.Controls.Add(this.layout);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AccountRequestForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Apply for an account";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel layout;
        private IUIS.SharedUI.Controls.LabeledFieldPanel _nameField;
        private IUIS.SharedUI.Controls.LabeledFieldPanel _emailField;
        private System.Windows.Forms.ComboBox _requestType;
        private IUIS.SharedUI.Controls.StatusBannerPanel _banner;
    }
}
