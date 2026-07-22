using System;
using System.Drawing;
using System.Windows.Forms;
using IUIS.SharedUI.Theme;

namespace IUIS.SharedUI.DataGridViews
{
    public static class DataGridViewStyleManager
    {
        public static void ApplyStandardStyle(DataGridView grid)
        {
            if (grid == null) return;

            grid.BackgroundColor = UiTheme.Surface;
            grid.BorderStyle = BorderStyle.FixedSingle;
            grid.GridColor = UiTheme.BorderNeutral;
            grid.DefaultCellStyle.BackColor = Color.White;
            grid.DefaultCellStyle.ForeColor = UiTheme.TextPrimary;
            grid.DefaultCellStyle.SelectionBackColor = UiTheme.InstitutionalPrimary;
            grid.DefaultCellStyle.SelectionForeColor = Color.White;
            grid.DefaultCellStyle.Font = UiTheme.BodyFont;
            grid.DefaultCellStyle.Padding = new Padding(8);
            grid.AlternatingRowsDefaultCellStyle.BackColor = UiTheme.ElevatedSurface;
            grid.ColumnHeadersDefaultCellStyle.BackColor = UiTheme.Surface;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = UiTheme.TextPrimary;
            grid.ColumnHeadersDefaultCellStyle.Font = UiTheme.FieldLabelFont;
            grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(8);
            grid.EnableHeadersVisualStyles = false;
            grid.RowHeadersDefaultCellStyle.BackColor = UiTheme.Surface;
            grid.RowHeadersDefaultCellStyle.ForeColor = UiTheme.TextSecondary;
            grid.RowHeadersDefaultCellStyle.Padding = new Padding(8);
            grid.RowHeadersWidth = 50;
            grid.RowTemplate.Height = 40;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.ReadOnly = true;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        public static void ApplyCompactStyle(DataGridView grid)
        {
            if (grid == null) return;

            ApplyStandardStyle(grid);
            grid.RowTemplate.Height = 32;
            grid.DefaultCellStyle.Padding = new Padding(4);
            grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(4);
        }

        public static void ApplyStripedStyle(DataGridView grid)
        {
            if (grid == null) return;

            ApplyStandardStyle(grid);
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 250, 251);
        }

        public static void ConfigureColumn(DataGridViewColumn column, string headerText, int width, bool visible = true)
        {
            if (column == null) return;

            column.HeaderText = headerText;
            column.Width = width;
            column.Visible = visible;
            column.HeaderCell.Style.Padding = new Padding(8);
            column.DefaultCellStyle.Padding = new Padding(8);
        }

        public static void SetColumnAlignment(DataGridViewColumn column, DataGridViewContentAlignment alignment)
        {
            if (column == null) return;

            column.HeaderCell.Style.Alignment = alignment;
            column.DefaultCellStyle.Alignment = alignment;
        }

        public static void FreezeColumn(DataGridViewColumn column)
        {
            if (column == null) return;

            column.Frozen = true;
        }

        public static void SetColumnReadOnly(DataGridViewColumn column, bool readOnly)
        {
            if (column == null) return;

            column.ReadOnly = readOnly;
        }
    }
}
