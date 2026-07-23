using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using IUIS.Application.Models;

namespace IUIS.SharedUI.Controls
{
    public class ValidationSummaryControl : UserControl
    {
        private Panel _containerPanel;
        private Label _titleLabel;
        private FlowLayoutPanel _errorsPanel;

        public ValidationSummaryControl()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(400, 200);
            this.BackColor = Color.Transparent;
            this.Visible = false;
            this.Dock = DockStyle.Top;

            _containerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(254, 242, 242),
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(16)
            };

            _titleLabel = new Label
            {
                Text = "⚠️ Please fix the following errors:",
                Font = Theme.UiTheme.FieldLabelFont,
                ForeColor = Theme.UiTheme.Error,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 8)
            };

            _errorsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = Color.Transparent,
                MinimumSize = new Size(0, 50)
            };

            _containerPanel.Controls.Add(_titleLabel);
            _containerPanel.Controls.Add(_errorsPanel);
            this.Controls.Add(_containerPanel);
            
            // Enable double buffering to prevent flicker
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.UserPaint |
                         ControlStyles.OptimizedDoubleBuffer, true);
        }

        public void SetErrors(IEnumerable<string> errors)
        {
            _errorsPanel.Controls.Clear();

            foreach (var error in errors)
            {
                var errorLabel = new Label
                {
                    Text = $"• {error}",
                    Font = Theme.UiTheme.BodyFont,
                    ForeColor = Theme.UiTheme.Error,
                    AutoSize = true,
                    Margin = new Padding(0, 4, 0, 4),
                    MaximumSize = new Size(this.Width - 40, 0)
                };
                _errorsPanel.Controls.Add(errorLabel);
            }

            UpdateVisibility();
        }

        public void SetErrors(OperationResult result)
        {
            if (result == null || result.IsSuccess)
            {
                ClearErrors();
                return;
            }
            
            SetErrors(result.Errors);
        }

        public void ClearErrors()
        {
            _errorsPanel.Controls.Clear();
            UpdateVisibility();
        }

        private void UpdateVisibility()
        {
            bool hasErrors = _errorsPanel.Controls.Count > 0;
            this.Visible = hasErrors;
            _titleLabel.Visible = hasErrors;
            _containerPanel.Visible = hasErrors;
            
            if (hasErrors)
            {
                this.BringToFront();
            }
        }
        
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            // Update label maximum size on resize for proper word wrapping
            foreach (Control ctrl in _errorsPanel.Controls)
            {
                if (ctrl is Label label)
                {
                    label.MaximumSize = new Size(this.Width - 40, 0);
                }
            }
        }
    }
}
