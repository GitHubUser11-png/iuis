using System;
using System.Drawing;
using System.Windows.Forms;
using IUIS.SharedUI.Theme;

namespace IUIS.SharedUI.Controls
{
    public static class DialogLayoutHelper
    {
        public static Panel CreateMainPanel()
        {
            return new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(UiMetrics.OuterPadding),
                AutoScroll = true
            };
        }

        public static Label CreateSectionHeader(string text)
        {
            return new Label
            {
                Text = text,
                Font = UiTheme.SectionHeadingFont,
                ForeColor = UiTheme.TextPrimary,
                AutoSize = true
            };
        }

        public static Label CreateFieldLabel(string text, bool required = false)
        {
            return new Label
            {
                Text = required ? text + " *" : text,
                Font = UiTheme.FieldLabelFont,
                ForeColor = UiTheme.TextPrimary,
                AutoSize = true
            };
        }

        public static TextBox CreateStandardTextBox()
        {
            return new TextBox
            {
                Height = UiMetrics.StandardFieldHeight,
                Font = UiTheme.BodyFont
            };
        }

        public static TextBox CreateReadOnlyTextBox()
        {
            return new TextBox
            {
                Height = UiMetrics.StandardFieldHeight,
                Font = UiTheme.BodyFont,
                ReadOnly = true,
                BackColor = Color.FromArgb(243, 244, 246)
            };
        }

        public static ComboBox CreateStandardComboBox()
        {
            return new ComboBox
            {
                Height = UiMetrics.StandardFieldHeight,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = UiTheme.BodyFont
            };
        }

        public static DateTimePicker CreateStandardDatePicker()
        {
            return new DateTimePicker
            {
                Height = UiMetrics.StandardFieldHeight,
                Font = UiTheme.BodyFont,
                Format = DateTimePickerFormat.Short,
                ShowUpDown = false
            };
        }

        public static TextBox CreateMultilineTextBox(int height = 80)
        {
            return new TextBox
            {
                Height = height,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = UiTheme.BodyFont
            };
        }

        public static Panel CreateButtonPanel(Button primaryButton, Button secondaryButton = null)
        {
            var panel = new Panel
            {
                Height = UiMetrics.StandardButtonHeight + 16,
                Dock = DockStyle.Bottom
            };

            if (secondaryButton != null)
            {
                secondaryButton.Location = new Point(
                    panel.Width - secondaryButton.Width - UiMetrics.ContentPadding,
                    8
                );
                panel.Controls.Add(secondaryButton);
            }

            if (primaryButton != null)
            {
                var x = secondaryButton != null
                    ? panel.Width - secondaryButton.Width - primaryButton.Width - UiMetrics.ContentPadding * 2
                    : panel.Width - primaryButton.Width - UiMetrics.ContentPadding;
                primaryButton.Location = new Point(x, 8);
                panel.Controls.Add(primaryButton);
            }

            return panel;
        }

        public static Panel CreateFieldRow(Control label, Control input, int rowHeight = 70)
        {
            var row = new Panel
            {
                Height = rowHeight,
                Dock = DockStyle.Top
            };

            label.Location = new Point(0, 0);
            input.Location = new Point(0, 22);
            input.Width = row.Width;

            row.Controls.Add(label);
            row.Controls.Add(input);

            return row;
        }

        public static Panel CreateTwoColumnRow(Control leftLabel, Control leftInput, Control rightLabel, Control rightInput, int rowHeight = 70)
        {
            var row = new Panel
            {
                Height = rowHeight,
                Dock = DockStyle.Top
            };

            var columnWidth = (row.Width - UiMetrics.ContentPadding) / 2;

            leftLabel.Location = new Point(0, 0);
            leftInput.Location = new Point(0, 22);
            leftInput.Width = columnWidth;

            rightLabel.Location = new Point(columnWidth + UiMetrics.ContentPadding, 0);
            rightInput.Location = new Point(columnWidth + UiMetrics.ContentPadding, 22);
            rightInput.Width = columnWidth;

            row.Controls.Add(leftLabel);
            row.Controls.Add(leftInput);
            row.Controls.Add(rightLabel);
            row.Controls.Add(rightInput);

            return row;
        }

        public static Panel CreateDetailsPanel(string[] labels, string[] values)
        {
            var panel = new Panel
            {
                BackColor = UiTheme.ElevatedSurface,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(16)
            };

            int y = 16;
            for (int i = 0; i < labels.Length && i < values.Length; i++)
            {
                var label = new Label
                {
                    Text = labels[i],
                    Font = UiTheme.CaptionFont,
                    ForeColor = UiTheme.TextSecondary,
                    Location = new Point(16, y),
                    AutoSize = true
                };

                var value = new Label
                {
                    Text = values[i],
                    Font = UiTheme.BodyFont,
                    ForeColor = UiTheme.TextPrimary,
                    Location = new Point(16, y + 18),
                    AutoSize = true
                };

                panel.Controls.Add(label);
                panel.Controls.Add(value);

                y += 50;
            }

            panel.Height = y + 16;
            return panel;
        }

        public static void AddVerticalSpacing(Panel parent, int pixels = 16)
        {
            var spacer = new Panel
            {
                Height = pixels,
                Dock = DockStyle.Top
            };
            parent.Controls.Add(spacer);
        }
    }
}
