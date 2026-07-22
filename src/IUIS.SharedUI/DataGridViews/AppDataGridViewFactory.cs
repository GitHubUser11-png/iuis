using System;
using System.Windows.Forms;
using IUIS.SharedUI.Theme;

namespace IUIS.SharedUI.DataGridViews
{
    public static class AppDataGridViewFactory
    {
        public static DataGridView CreateStyledDataGridView()
        {
            var grid = new DataGridView
            {
                AutoGenerateColumns = false,
                AllowDrop = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToOrderColumns = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = true,
                ColumnHeadersVisible = true,
                EnableHeadersVisualStyles = false,
                BackgroundColor = UiTheme.Surface,
                GridColor = UiTheme.BorderNeutral,
                BorderStyle = BorderStyle.FixedSingle
            };

            DataGridViewStyleManager.ApplyStandardStyle(grid);

            return grid;
        }

        public static void AddTextBoxColumn(DataGridView grid, string dataPropertyName, string headerText, int width)
        {
            if (grid == null) throw new ArgumentNullException(nameof(grid));

            var column = new DataGridViewTextBoxColumn
            {
                DataPropertyName = dataPropertyName,
                HeaderText = headerText,
                Width = width
            };

            DataGridViewStyleManager.ConfigureColumn(column, headerText, width);
            grid.Columns.Add(column);
        }

        public static void AddDateColumn(DataGridView grid, string dataPropertyName, string headerText, string format, int width)
        {
            if (grid == null) throw new ArgumentNullException(nameof(grid));

            var column = new DataGridViewTextBoxColumn
            {
                DataPropertyName = dataPropertyName,
                HeaderText = headerText,
                Width = width
            };

            if (column.DefaultCellStyle != null)
            {
                column.DefaultCellStyle.Format = format;
            }

            DataGridViewStyleManager.ConfigureColumn(column, headerText, width);
            grid.Columns.Add(column);
        }

        public static void AddButtonColumn(DataGridView grid, string name, string headerText, string text, int width)
        {
            if (grid == null) throw new ArgumentNullException(nameof(grid));

            var column = new DataGridViewButtonColumn
            {
                Name = name,
                HeaderText = headerText,
                Text = text,
                UseColumnTextForButtonValue = true,
                Width = width
            };

            DataGridViewStyleManager.ConfigureColumn(column, headerText, width);
            grid.Columns.Add(column);
        }

        public static void AddCheckBoxColumn(DataGridView grid, string dataPropertyName, string headerText, int width)
        {
            if (grid == null) throw new ArgumentNullException(nameof(grid));

            var column = new DataGridViewCheckBoxColumn
            {
                DataPropertyName = dataPropertyName,
                HeaderText = headerText,
                Width = width
            };

            DataGridViewStyleManager.ConfigureColumn(column, headerText, width);
            grid.Columns.Add(column);
        }

        public static void AddComboBoxColumn(DataGridView grid, string dataPropertyName, string headerText, int width)
        {
            if (grid == null) throw new ArgumentNullException(nameof(grid));

            var column = new DataGridViewComboBoxColumn
            {
                DataPropertyName = dataPropertyName,
                HeaderText = headerText,
                Width = width
            };

            DataGridViewStyleManager.ConfigureColumn(column, headerText, width);
            grid.Columns.Add(column);
        }

        public static void AddCurrencyColumn(DataGridView grid, string dataPropertyName, string headerText, int width)
        {
            if (grid == null) throw new ArgumentNullException(nameof(grid));

            var column = new DataGridViewTextBoxColumn
            {
                DataPropertyName = dataPropertyName,
                HeaderText = headerText,
                Width = width
            };

            if (column.DefaultCellStyle != null)
            {
                column.DefaultCellStyle.Format = "C2";
                column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }

            DataGridViewStyleManager.ConfigureColumn(column, headerText, width);
            DataGridViewStyleManager.SetColumnAlignment(column, DataGridViewContentAlignment.MiddleRight);
            grid.Columns.Add(column);
        }

        public static void AddNumericColumn(DataGridView grid, string dataPropertyName, string headerText, int width, string format = "N2")
        {
            if (grid == null) throw new ArgumentNullException(nameof(grid));

            var column = new DataGridViewTextBoxColumn
            {
                DataPropertyName = dataPropertyName,
                HeaderText = headerText,
                Width = width
            };

            if (column.DefaultCellStyle != null)
            {
                column.DefaultCellStyle.Format = format;
                column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }

            DataGridViewStyleManager.ConfigureColumn(column, headerText, width);
            DataGridViewStyleManager.SetColumnAlignment(column, DataGridViewContentAlignment.MiddleRight);
            grid.Columns.Add(column);
        }

        public static void AddStatusColumn(DataGridView grid, string dataPropertyName, string headerText, int width)
        {
            if (grid == null) throw new ArgumentNullException(nameof(grid));

            var column = new DataGridViewTextBoxColumn
            {
                DataPropertyName = dataPropertyName,
                HeaderText = headerText,
                Width = width
            };

            DataGridViewStyleManager.ConfigureColumn(column, headerText, width);
            grid.Columns.Add(column);
        }
    }
}
