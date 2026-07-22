namespace IUIS.AdminApp.Forms.Startup
{
    partial class RepositoryInitializationForm
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
            this.subtitle = new System.Windows.Forms.Label();
            this._banner = new IUIS.SharedUI.Controls.StatusBannerPanel();
            this._loginIdField = new IUIS.SharedUI.Controls.LabeledFieldPanel();
            this._passwordField = new IUIS.SharedUI.Controls.LabeledFieldPanel();
            this._givenNameField = new IUIS.SharedUI.Controls.LabeledFieldPanel();
            this._familyNameField = new IUIS.SharedUI.Controls.LabeledFieldPanel();
            this._emailField = new IUIS.SharedUI.Controls.LabeledFieldPanel();
            this._birthDateField = new IUIS.SharedUI.Controls.LabeledFieldPanel();
            this._departmentField = new IUIS.SharedUI.Controls.LabeledFieldPanel();
            this._positionField = new IUIS.SharedUI.Controls.LabeledFieldPanel();
            this._submitButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // title
            // 
            this.title.AutoSize = true;
            this.title.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.title.Location = new System.Drawing.Point(32, 24);
            this.title.Name = "title";
            this.title.Size = new System.Drawing.Size(279, 32);
            this.title.TabIndex = 0;
            this.title.Text = "Initialize production repository";
            // 
            // subtitle
            // 
            this.subtitle.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.subtitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.subtitle.Location = new System.Drawing.Point(32, 56);
            this.subtitle.Name = "subtitle";
            this.subtitle.Size = new System.Drawing.Size(640, 48);
            this.subtitle.TabIndex = 1;
            this.subtitle.Text = "This will create all 49 authoritative JSON repository files and the first administrator account.";
            // 
            // _banner
            // 
            this._banner.Location = new System.Drawing.Point(32, 108);
            this._banner.Name = "_banner";
            this._banner.Size = new System.Drawing.Size(640, 40);
            this._banner.TabIndex = 2;
            // 
            // _loginIdField
            // 
            this._loginIdField.Location = new System.Drawing.Point(32, 152);
            this._loginIdField.Name = "_loginIdField";
            this._loginIdField.Size = new System.Drawing.Size(320, 80);
            this._loginIdField.TabIndex = 3;
            // 
            // _passwordField
            // 
            this._passwordField.Location = new System.Drawing.Point(32, 232);
            this._passwordField.Name = "_passwordField";
            this._passwordField.Size = new System.Drawing.Size(320, 80);
            this._passwordField.TabIndex = 4;
            // 
            // _givenNameField
            // 
            this._givenNameField.Location = new System.Drawing.Point(32, 312);
            this._givenNameField.Name = "_givenNameField";
            this._givenNameField.Size = new System.Drawing.Size(320, 80);
            this._givenNameField.TabIndex = 5;
            // 
            // _familyNameField
            // 
            this._familyNameField.Location = new System.Drawing.Point(360, 312);
            this._familyNameField.Name = "_familyNameField";
            this._familyNameField.Size = new System.Drawing.Size(320, 80);
            this._familyNameField.TabIndex = 6;
            // 
            // _emailField
            // 
            this._emailField.Location = new System.Drawing.Point(32, 392);
            this._emailField.Name = "_emailField";
            this._emailField.Size = new System.Drawing.Size(320, 80);
            this._emailField.TabIndex = 7;
            // 
            // _birthDateField
            // 
            this._birthDateField.Location = new System.Drawing.Point(360, 392);
            this._birthDateField.Name = "_birthDateField";
            this._birthDateField.Size = new System.Drawing.Size(320, 80);
            this._birthDateField.TabIndex = 8;
            // 
            // _departmentField
            // 
            this._departmentField.Location = new System.Drawing.Point(32, 472);
            this._departmentField.Name = "_departmentField";
            this._departmentField.Size = new System.Drawing.Size(320, 80);
            this._departmentField.TabIndex = 9;
            // 
            // _positionField
            // 
            this._positionField.Location = new System.Drawing.Point(360, 472);
            this._positionField.Name = "_positionField";
            this._positionField.Size = new System.Drawing.Size(320, 80);
            this._positionField.TabIndex = 10;
            // 
            // _submitButton
            // 
            this._submitButton.Location = new System.Drawing.Point(32, 560);
            this._submitButton.Name = "_submitButton";
            this._submitButton.Size = new System.Drawing.Size(200, 36);
            this._submitButton.TabIndex = 11;
            this._submitButton.Text = "Initialize Repository";
            this._submitButton.UseVisualStyleBackColor = true;
            // 
            // RepositoryInitializationForm
            // 
            this.ClientSize = new System.Drawing.Size(720, 680);
            this.Controls.Add(this._submitButton);
            this.Controls.Add(this._positionField);
            this.Controls.Add(this._departmentField);
            this.Controls.Add(this._birthDateField);
            this.Controls.Add(this._emailField);
            this.Controls.Add(this._familyNameField);
            this.Controls.Add(this._givenNameField);
            this.Controls.Add(this._passwordField);
            this.Controls.Add(this._loginIdField);
            this.Controls.Add(this._banner);
            this.Controls.Add(this.subtitle);
            this.Controls.Add(this.title);
            this.MinimumSize = new System.Drawing.Size(720, 680);
            this.Name = "RepositoryInitializationForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label title;
        private System.Windows.Forms.Label subtitle;
        private IUIS.SharedUI.Controls.StatusBannerPanel _banner;
        private IUIS.SharedUI.Controls.LabeledFieldPanel _loginIdField;
        private IUIS.SharedUI.Controls.LabeledFieldPanel _passwordField;
        private IUIS.SharedUI.Controls.LabeledFieldPanel _givenNameField;
        private IUIS.SharedUI.Controls.LabeledFieldPanel _familyNameField;
        private IUIS.SharedUI.Controls.LabeledFieldPanel _emailField;
        private IUIS.SharedUI.Controls.LabeledFieldPanel _birthDateField;
        private IUIS.SharedUI.Controls.LabeledFieldPanel _departmentField;
        private IUIS.SharedUI.Controls.LabeledFieldPanel _positionField;
        private System.Windows.Forms.Button _submitButton;
    }
}
