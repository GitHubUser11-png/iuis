using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

using IUIS.SharedUI.Theme;

namespace IUIS.SharedUI.DataGridViews
{
    public static class AppDataGridViewFactory
    {
        public static DataGridView CreateStyledDataGridView()
        {
            var dataGridView = new DataGridView
            {
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                BackgroundColor = UiTheme.ElevatedSurface,
                BorderStyle = BorderStyle.FixedSingle,
                EnableHeadersVisualStyles = false,
                GridColor = UiTheme.BorderNeutral,
                Font = UiTheme.BodyFont,
                ForeColor = UiTheme.TextPrimary,
                BackColor = UiTheme.ElevatedSurface,
                RowTemplate = { Height = UiMetrics.StandardRowHeight },
                ColumnHeadersHeight = UiMetrics.StandardRowHeight,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None,
                ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText,
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = UiTheme.Surface
                }
            };

            dataGridView.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = UiTheme.SubtleSurface,
                ForeColor = UiTheme.TextPrimary,
                SelectionBackColor = UiTheme.SubtleSurface,
                SelectionForeColor = UiTheme.TextPrimary,
                Font = UiTheme.SectionHeadingFont,
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                Padding = new Padding(8, 4, 8, 4)
            };

            dataGridView.DefaultCellStyle = new DataGridViewCellStyle
            {
                Padding = new Padding(8, 4, 8, 4),
                SelectionBackColor = UiTheme.Selection,
                SelectionForeColor = UiTheme.TextPrimary,
                NullValue = "—"
            };

            return dataGridView;
        }

        public static void AddTextBoxColumn(
            DataGridView dataGridView,
            string dataPropertyName,
            string headerText,
            int width = 100,
            bool visible = true)
        {
            var column = new DataGridViewTextBoxColumn
            {
                DataPropertyName = dataPropertyName,
                HeaderText = headerText,
                Width = width,
                Visible = visible,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None
            };

            dataGridView.Columns.Add(column);
        }

        public static void AddTextColumn(
            DataGridView dataGridView,
            string dataPropertyName,
            string headerText,
            DataGridViewAutoSizeColumnMode autoSizeMode,
            bool visible = true)
        {
            var column = new DataGridViewTextBoxColumn
            {
                DataPropertyName = dataPropertyName,
                HeaderText = headerText,
                Visible = visible,
                AutoSizeMode = autoSizeMode
            };

            dataGridView.Columns.Add(column);
        }

        public static void AddDateColumn(
            DataGridView dataGridView,
            string dataPropertyName,
            string headerText,
            string format = "MM/dd/yyyy",
            int width = 100)
        {
            var column = new DataGridViewTextBoxColumn
            {
                DataPropertyName = dataPropertyName,
                HeaderText = headerText,
                Width = width,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None
            };

            column.DefaultCellStyle.Format = format;
            dataGridView.Columns.Add(column);
        }

        public static void AddCurrencyColumn(
            DataGridView dataGridView,
            string dataPropertyName,
            string headerText,
            int width = 100)
        {
            var column = new DataGridViewTextBoxColumn
            {
                DataPropertyName = dataPropertyName,
                HeaderText = headerText,
                Width = width,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None
            };

            column.DefaultCellStyle.Format = "C2";
            column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dataGridView.Columns.Add(column);
        }

        public static void AddButtonColumn(
            DataGridView dataGridView,
            string name,
            string headerText,
            string text = "View",
            int width = 80)
        {
            var column = new DataGridViewButtonColumn
            {
                Name = name,
                HeaderText = headerText,
                Text = text,
                UseColumnTextForButtonValue = true,
                Width = width,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None
            };

            dataGridView.Columns.Add(column);
        }
    }
}
