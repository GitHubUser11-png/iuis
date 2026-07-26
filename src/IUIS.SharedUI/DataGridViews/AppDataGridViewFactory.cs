using System;
using System.Windows.Forms;
using IUIS.SharedUI.Theme;

namespace IUIS.SharedUI.DataGrids
{
    public static class AppDataGridViewFactory
    {
        public static DataGridView CreateStandardGrid()
        {
            var grid = new DataGridView
            {
                DoubleBuffered = true, // Prevents flickering
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                BorderStyle = BorderStyle.None,
                BackgroundColor = UiTheme.Colors.SurfaceBackground,
                GridColor = UiTheme.Colors.BorderLight,
                Font = UiTheme.Fonts.Body,
                RowTemplate.Height = UiMetrics.Spacing48
            };

            // Apply standard styles
            DataGridViewStyleManager.ApplyBaseStyles(grid);
            
            return grid;
        }

        public static void ConfigureCurrencyColumn(DataGridViewColumn col)
        {
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.DefaultCellStyle.Format = "C2";
            col.SortMode = DataGridViewColumnSortMode.Automatic;
        }

        public static void ConfigureDateColumn(DataGridViewColumn col)
        {
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            col.DefaultCellStyle.Format = "MMM dd, yyyy";
            col.SortMode = DataGridViewColumnSortMode.Automatic;
        }

        public static void ConfigureStatusColumn(DataGridViewColumn col)
        {
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            // Custom painting for badges handled by StyleManager or Cell Template
        }
    }
}
