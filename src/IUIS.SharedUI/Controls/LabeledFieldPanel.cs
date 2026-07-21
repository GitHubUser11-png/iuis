using System.Drawing;
using System.Windows.Forms;

using IUIS.SharedUI.Theme;

namespace IUIS.SharedUI.Controls
{
    public sealed class LabeledFieldPanel : Panel
    {
        private readonly Label _label;
        private readonly Label _helpLabel;
        private readonly Label _errorLabel;

        public LabeledFieldPanel(string labelText, bool required)
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
            InputControl.Width = Width;
            InputControl.Location = new Point(0, 22);

            _helpLabel = new Label();
            _helpLabel.Font = UiTheme.CaptionFont;
            _helpLabel.ForeColor = UiTheme.TextSecondary;
            _helpLabel.AutoSize = true;
            _helpLabel.Location = new Point(0, 68);
            _helpLabel.Visible = false;

            _errorLabel = new Label();
            _errorLabel.Font = UiTheme.CaptionFont;
            _errorLabel.ForeColor = UiTheme.Error;
            _errorLabel.AutoSize = true;
            _errorLabel.Location = new Point(0, 68);
            _errorLabel.Visible = false;

            Controls.Add(_label);
            Controls.Add(InputControl);
            Controls.Add(_helpLabel);
            Controls.Add(_errorLabel);
        }

        public TextBox InputControl { get; private set; }

        public void SetHelpText(string helpText)
        {
            _helpLabel.Text = helpText ?? string.Empty;
            _helpLabel.Visible = !string.IsNullOrWhiteSpace(helpText);
            RepositionMessages();
        }

        public void SetErrorText(string errorText)
        {
            _errorLabel.Text = errorText ?? string.Empty;
            _errorLabel.Visible = !string.IsNullOrWhiteSpace(errorText);
            InputControl.BackColor = _errorLabel.Visible
                ? Color.FromArgb(255, 245, 245)
                : Color.White;
            RepositionMessages();
        }

        public void ClearError()
        {
            SetErrorText(null);
        }

        private void RepositionMessages()
        {
            var y = 68;
            if (_helpLabel.Visible)
            {
                _helpLabel.Location = new Point(0, y);
                y += _helpLabel.Height + 2;
            }

            if (_errorLabel.Visible)
                _errorLabel.Location = new Point(0, y);
        }
    }
}
