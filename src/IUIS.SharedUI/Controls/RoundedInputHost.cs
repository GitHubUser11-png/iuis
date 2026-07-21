using System;
using System.Drawing;
using System.Windows.Forms;

using IUIS.SharedUI.Theme;

namespace IUIS.SharedUI.Controls
{
    /// <summary>
    /// Rounded bordered container for a borderless TextBox/ComboBox.
    /// Paints a navy border on focus and a red border in error state —
    /// the standard WinForms approach since TextBox itself cannot be rounded.
    /// </summary>
    public sealed class RoundedInputHost : Panel
    {
        private readonly Control _input;
        private bool _hasFocus;
        private bool _hasError;

        public RoundedInputHost(Control input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);

            _input = input;
            BackColor = UiTheme.ElevatedSurface;
            Height = UiMetrics.StandardFieldHeight;
            Padding = new Padding(12, 0, 12, 0);

            var textBox = input as TextBox;
            if (textBox != null)
            {
                textBox.BorderStyle = BorderStyle.None;
            }

            var comboBox = input as ComboBox;
            if (comboBox != null)
            {
                comboBox.FlatStyle = FlatStyle.Flat;
            }

            _input.BackColor = UiTheme.ElevatedSurface;
            _input.Font = UiTheme.BodyFont;
            _input.GotFocus += InputGotFocus;
            _input.LostFocus += InputLostFocus;

            Controls.Add(_input);
            CenterInput();
            Resize += delegate { CenterInput(); };
            Click += delegate { _input.Focus(); };
        }

        public Control Input
        {
            get { return _input; }
        }

        public bool HasError
        {
            get { return _hasError; }
            set
            {
                _hasError = value;
                _input.BackColor = value ? Color.FromArgb(255, 245, 245) : UiTheme.ElevatedSurface;
                BackColor = _input.BackColor;
                Invalidate();
            }
        }

        private void InputGotFocus(object sender, EventArgs e)
        {
            _hasFocus = true;
            Invalidate();
        }

        private void InputLostFocus(object sender, EventArgs e)
        {
            _hasFocus = false;
            Invalidate();
        }

        private void CenterInput()
        {
            int innerWidth = Width - Padding.Left - Padding.Right;
            if (innerWidth < 10)
                innerWidth = 10;
            _input.Width = innerWidth;
            _input.Location = new Point(Padding.Left, (Height - _input.Height) / 2);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Color border = _hasError
                ? UiTheme.Error
                : _hasFocus ? UiTheme.BorderFocus : UiTheme.BorderNeutral;

            UiPainting.PaintRoundedSurface(
                e.Graphics,
                ClientRectangle,
                UiMetrics.InputCornerRadius,
                Color.Empty,
                border);

            if (_hasFocus && !_hasError)
            {
                // Slightly stronger inner line for a focus emphasis.
                var inner = new Rectangle(1, 1, Width - 2, Height - 2);
                UiPainting.PaintRoundedSurface(
                    e.Graphics,
                    inner,
                    UiMetrics.InputCornerRadius,
                    Color.Empty,
                    Color.FromArgb(120, UiTheme.BorderFocus));
            }
        }
    }
}
