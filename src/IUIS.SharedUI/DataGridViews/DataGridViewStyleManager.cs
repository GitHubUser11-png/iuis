using System.Drawing;
using System.Windows.Forms;
using IUIS.SharedUI.Theme;

namespace IUIS.SharedUI.DataGrids
{
    public static class DataGridViewStyleManager
    {
        public static void ApplyBaseStyles(DataGridView grid)
        {
            // Header Styles
            grid.ColumnHeadersDefaultCellStyle.Font = UiTheme.Fonts.HeaderSmall;
            grid.ColumnHeadersDefaultCellStyle.BackColor = UiTheme.Colors.SurfaceCard;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = UiTheme.Colors.TextPrimary;
            grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(0, UiMetrics.Spacing12, 0, UiMetrics.Spacing12);
            grid.ColumnHeadersHeight = UiMetrics.Spacing56;
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            grid.ColumnHeaderDefaultCellStyle.SelectionBackColor = UiTheme.Colors.SurfaceCard;
            grid.ColumnHeaderDefaultCellStyle.SelectionForeColor = UiTheme.Colors.TextPrimary;

            // Row Styles
            grid.DefaultCellStyle.BackColor = UiTheme.Colors.SurfaceBackground;
            grid.DefaultCellStyle.ForeColor = UiTheme.Colors.TextPrimary;
            grid.DefaultCellStyle.SelectionBackColor = UiTheme.Colors.PrimaryLight;
            grid.DefaultCellStyle.SelectionForeColor = UiTheme.Colors.TextPrimary;
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 252); // Slight variation
            grid.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            
            // Borders
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.AdvancedColumnHeadersBorderStyle.All = DataGridViewAdvancedCellBorderStyle.Single;
        }

        public static void ApplyCurrencyStyle(DataGridViewCell cell)
        {
            cell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            cell.Style.Format = "C2";
        }

        public static void ApplyDateStyle(DataGridViewCell cell)
        {
            cell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            cell.Style.Format = "MMM dd, yyyy";
        }
    }
}
