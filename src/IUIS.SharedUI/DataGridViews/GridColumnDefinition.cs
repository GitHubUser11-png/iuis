// Ensure this class exists and allows defining custom column types
// If the file already exists, just verify it doesn't conflict with the Factory above.
using System.Windows.Forms;

namespace IUIS.SharedUI.DataGrids
{
    public class GridColumnDefinition
    {
        public string HeaderText { get; set; }
        public string DataPropertyName { get; set; }
        public int Width { get; set; }
        public bool Visible { get; set; } = true;
        
        // Add specific types if needed (Badge, Currency, Date)
        public enum ColumnType { Standard, Currency, Date, StatusBadge }
        public ColumnType Type { get; set; } = ColumnType.Standard;

        public DataGridViewColumn CreateColumn()
        {
            var col = new DataGridViewTextBoxColumn
            {
                HeaderText = this.HeaderText,
                DataPropertyName = this.DataPropertyName,
                Width = this.Width,
                Visible = this.Visible,
                SortMode = DataGridViewColumnSortMode.Automatic
            };

            if (Type == ColumnType.Currency)
            {
                AppDataGridViewFactory.ConfigureCurrencyColumn(col);
            }
            else if (Type == ColumnType.Date)
            {
                AppDataGridViewFactory.ConfigureDateColumn(col);
            }
            else if (Type == ColumnType.StatusBadge)
            {
                AppDataGridViewFactory.ConfigureStatusColumn(col);
            }

            return col;
        }
    }
}
