using System;

namespace IUIS.SharedUI.DataGridViews
{
    public sealed class GridColumnDefinition
    {
        public string DataPropertyName { get; set; }
        
        public string HeaderText { get; set; }
        
        public int Width { get; set; }
        
        public bool Visible { get; set; }
        
        public string Format { get; set; }
        
        public bool IsSortable { get; set; }
        
        public bool IsFilterable { get; set; }
        
        public GridColumnType ColumnType { get; set; }

        public GridColumnDefinition()
        {
            DataPropertyName = string.Empty;
            HeaderText = string.Empty;
            Width = 100;
            Visible = true;
            Format = string.Empty;
            IsSortable = true;
            IsFilterable = true;
            ColumnType = GridColumnType.Text;
        }
    }

    public enum GridColumnType
    {
        Text,
        Number,
        Currency,
        Date,
        DateTime,
        Boolean,
        Button
    }
}
