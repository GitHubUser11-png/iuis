using System.Drawing;
using System.Windows.Forms;

using IUIS.SharedUI.Theme;

namespace IUIS.SharedUI.Controls
{
    public sealed class PasswordFieldPanel : Panel
    {
        private readonly Label _label;
        private readonly Label _errorLabel;
        private readonly Label _helpLabel;
        private readonly Button _toggleButton;
        private bool _visibleText;

        public PasswordFieldPanel(string labelText, bool required)
        {
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Width = UiMetrics.FormCardWidth - 64;
            Padding = new Padding(0, 0, 0, 8);

            _label = new Label();
            _label.Text = required ? labelText + " *" : labelText;
            _label.Font = UiTheme.FieldLabelFont;
            _label.ForeColor = UiTheme.TextPrimary;
            _label.AutoSize = true;
            _label.Location = new Point(0, 0);

            InputControl = new TextBox();
            InputControl.Font = UiTheme.BodyFont;
            InputControl.Height = UiMetrics.StandardFieldHeight;
            InputControl.Width = Width - 72;
            InputControl.Location = new Point(0, 22);
            InputControl.UseSystemPasswordChar = true;

            _toggleButton = new Button();
            _toggleButton.Text = "Show";
            _toggleButton.Width = 64;
            _toggleButton.Height = UiMetrics.StandardFieldHeight;
            _toggleButton.Location = new Point(InputControl.Right + 8, 22);
            _toggleButton.FlatStyle = FlatStyle.Flat;
            _toggleButton.Font = UiTheme.CaptionFont;
            _toggleButton.Click += ToggleVisibility;

            _helpLabel = new Label();
            _helpLabel.Font = UiTheme.CaptionFont;
            _helpLabel.ForeColor = UiTheme.TextSecondary;
            _helpLabel.AutoSize = true;
            _helpLabel.Location = new Point(0, 50);
            _helpLabel.Visible = false;

            _errorLabel = new Label();
            _errorLabel.Font = UiTheme.CaptionFont;
            _errorLabel.ForeColor = UiTheme.Error;
            _errorLabel.AutoSize = true;
            _errorLabel.Location = new Point(0, 68);
            _errorLabel.Visible = false;

            Controls.Add(_label);
            Controls.Add(InputControl);
            Controls.Add(_toggleButton);
            Controls.Add(_helpLabel);
            Controls.Add(_errorLabel);
        }

        public TextBox InputControl { get; private set; }

        public void SetErrorText(string errorText)
        {
            _errorLabel.Text = errorText ?? string.Empty;
            _errorLabel.Visible = !string.IsNullOrWhiteSpace(errorText);
            InputControl.BackColor = _errorLabel.Visible
                ? Color.FromArgb(255, 245, 245)
                : Color.White;
        }

        public void ClearError()
        {
            SetErrorText(null);
        }

        public void SetHelpText(string helpText)
        {
            _helpLabel.Text = helpText ?? string.Empty;
            _helpLabel.Visible = !string.IsNullOrWhiteSpace(helpText);
        }

        private void ToggleVisibility(object sender, System.EventArgs e)
        {
            _visibleText = !_visibleText;
            InputControl.UseSystemPasswordChar = !_visibleText;
            _toggleButton.Text = _visibleText ? "Hide" : "Show";
        }
    }
}
