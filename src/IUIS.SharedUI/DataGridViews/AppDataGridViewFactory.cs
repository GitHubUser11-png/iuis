using System;
using System.Drawing;
using System.Windows.Forms;

namespace IUIS.SharedUI.DataGridViews
{
    /// <summary>
    /// Factory for creating consistently styled DataGridView controls across the application.
    /// Provides methods to add columns with specific formatting and behavior.
    /// </summary>
    public static class AppDataGridViewFactory
    {
        private static readonly Color GridBackColor = Color.White;
        private static readonly Color GridHeaderBackColor = Color.FromArgb(31, 41, 55);
        private static readonly Color GridHeaderForeColor = Color.White;
        private static readonly Color GridAlternatingBackColor = Color.FromArgb(249, 250, 251);
        private static readonly Color GridBorderColor = Color.FromArgb(209, 213, 219);

        /// <summary>
        /// Creates a styled DataGridView with consistent appearance across the application.
        /// </summary>
        public static DataGridView CreateStyledDataGridView()
        {
            var dataGridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = GridBackColor,
                BorderStyle = BorderStyle.Single,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = GridBorderColor,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = GridBackColor,
                    ForeColor = Color.FromArgb(31, 41, 55),
                    Font = new Font("Segoe UI", 9F),
                    Alignment = DataGridViewContentAlignment.MiddleLeft,
                    Padding = new Padding(4, 2, 4, 2)
                },
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = GridAlternatingBackColor,
                    ForeColor = Color.FromArgb(31, 41, 55),
                    Font = new Font("Segoe UI", 9F)
                },
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = GridHeaderBackColor,
                    ForeColor = GridHeaderForeColor,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    Alignment = DataGridViewContentAlignment.MiddleLeft,
                    Padding = new Padding(4, 6, 4, 6),
                    WrapMode = DataGridViewTriState.False
                },
                RowTemplate = new DataGridViewRow
                {
                    Height = 32
                }
            };

            return dataGridView;
        }

        /// <summary>
        /// Adds a text box column to the DataGridView.
        /// </summary>
        public static void AddTextBoxColumn(DataGridView grid, string dataPropertyName, string headerText, int width)
        {
            var column = new DataGridViewTextBoxColumn
            {
                DataPropertyName = dataPropertyName,
                HeaderText = headerText,
                Width = width,
                ReadOnly = true,
                Name = dataPropertyName
            };
            grid.Columns.Add(column);
        }

        /// <summary>
        /// Adds a numeric column to the DataGridView.
        /// </summary>
        public static void AddNumericColumn(DataGridView grid, string dataPropertyName, string headerText, int width, string format = "N2")
        {
            var column = new DataGridViewTextBoxColumn
            {
                DataPropertyName = dataPropertyName,
                HeaderText = headerText,
                Width = width,
                ReadOnly = true,
                Name = dataPropertyName,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = format,
                    Alignment = DataGridViewContentAlignment.MiddleRight
                }
            };
            grid.Columns.Add(column);
        }

        /// <summary>
        /// Adds a currency column to the DataGridView.
        /// </summary>
        public static void AddCurrencyColumn(DataGridView grid, string dataPropertyName, string headerText, int width)
        {
            var column = new DataGridViewTextBoxColumn
            {
                DataPropertyName = dataPropertyName,
                HeaderText = headerText,
                Width = width,
                ReadOnly = true,
                Name = dataPropertyName,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "C2",
                    Alignment = DataGridViewContentAlignment.MiddleRight
                }
            };
            grid.Columns.Add(column);
        }

        /// <summary>
        /// Adds a date column to the DataGridView.
        /// </summary>
        public static void AddDateColumn(DataGridView grid, string dataPropertyName, string headerText, string format, int width)
        {
            var column = new DataGridViewTextBoxColumn
            {
                DataPropertyName = dataPropertyName,
                HeaderText = headerText,
                Width = width,
                ReadOnly = true,
                Name = dataPropertyName,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = format,
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                }
            };
            grid.Columns.Add(column);
        }

        /// <summary>
        /// Adds a boolean checkbox column to the DataGridView.
        /// </summary>
        public static void AddCheckBoxColumn(DataGridView grid, string dataPropertyName, string headerText, int width)
        {
            var column = new DataGridViewCheckBoxColumn
            {
                DataPropertyName = dataPropertyName,
                HeaderText = headerText,
                Width = width,
                ReadOnly = true,
                Name = dataPropertyName,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                }
            };
            grid.Columns.Add(column);
        }

        /// <summary>
        /// Adds a button column to the DataGridView.
        /// </summary>
        public static void AddButtonColumn(DataGridView grid, string columnName, string headerText, string buttonText, int width)
        {
            var column = new DataGridViewButtonColumn
            {
                HeaderText = headerText,
                Width = width,
                Name = columnName,
                Text = buttonText,
                UseColumnTextForButtonValue = true,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(79, 70, 229),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9F),
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                }
            };
            grid.Columns.Add(column);
        }

        /// <summary>
        /// Adds a status/badge column to the DataGridView.
        /// </summary>
        public static void AddStatusColumn(DataGridView grid, string dataPropertyName, string headerText, int width)
        {
            var column = new DataGridViewTextBoxColumn
            {
                DataPropertyName = dataPropertyName,
                HeaderText = headerText,
                Width = width,
                ReadOnly = true,
                Name = dataPropertyName,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold)
                }
            };
            grid.Columns.Add(column);
        }

        /// <summary>
        /// Adds a combobox column to the DataGridView.
        /// </summary>
        public static void AddComboBoxColumn(DataGridView grid, string dataPropertyName, string headerText, int width, string[] items)
        {
            var column = new DataGridViewComboBoxColumn
            {
                DataPropertyName = dataPropertyName,
                HeaderText = headerText,
                Width = width,
                Name = dataPropertyName,
                ReadOnly = false
            };

            foreach (var item in items)
            {
                column.Items.Add(item);
            }

            grid.Columns.Add(column);
        }

        /// <summary>
        /// Applies consistent styling to a DataGridView.
        /// </summary>
        public static void ApplyConsistentStyling(DataGridView grid)
        {
            grid.BackgroundColor = GridBackColor;
            grid.GridColor = GridBorderColor;
            
            grid.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = GridBackColor,
                ForeColor = Color.FromArgb(31, 41, 55),
                Font = new Font("Segoe UI", 9F),
                Padding = new Padding(4, 2, 4, 2)
            };

            grid.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = GridAlternatingBackColor,
                ForeColor = Color.FromArgb(31, 41, 55),
                Font = new Font("Segoe UI", 9F)
            };

            grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = GridHeaderBackColor,
                ForeColor = GridHeaderForeColor,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                Padding = new Padding(4, 6, 4, 6)
            };
        }

        /// <summary>
        /// Enables sorting functionality on grid columns.
        /// </summary>
        public static void EnableSorting(DataGridView grid, DataGridViewCellMouseEventHandler sortClickHandler)
        {
            grid.ColumnHeaderMouseClick += sortClickHandler;
        }

        /// <summary>
        /// Gets or creates a column style for a specific status value.
        /// </summary>
        public static DataGridViewCellStyle GetStatusStyle(string status)
        {
            return status switch
            {
                "Active" => new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(220, 252, 231),
                    ForeColor = Color.FromArgb(22, 101, 52),
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold)
                },
                "Inactive" => new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(243, 244, 246),
                    ForeColor = Color.FromArgb(75, 85, 99),
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold)
                },
                "Pending" => new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(255, 243, 224),
                    ForeColor = Color.FromArgb(120, 53, 15),
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold)
                },
                "Suspended" => new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(254, 226, 226),
                    ForeColor = Color.FromArgb(127, 29, 29),
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold)
                },
                _ => new DataGridViewCellStyle
                {
                    BackColor = GridBackColor,
                    ForeColor = Color.FromArgb(31, 41, 55),
                    Font = new Font("Segoe UI", 9F)
                }
            };
        }

        /// <summary>
        /// Applies row coloring based on status.
        /// </summary>
        public static void ApplyStatusRowColoring(DataGridView grid, string statusColumnName)
        {
            foreach (DataGridViewRow row in grid.Rows)
            {
                if (row.Cells[statusColumnName].Value is string status)
                {
                    var style = GetStatusStyle(status);
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        cell.Style = style;
                    }
                }
            }
        }

        /// <summary>
        /// Creates an empty state message panel for empty DataGridViews.
        /// </summary>
        public static Panel CreateEmptyStatePanel(string message)
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            var label = new Label
            {
                Text = message,
                Font = new Font("Segoe UI", 11F),
                ForeColor = Color.FromArgb(107, 114, 128),
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = false,
                Dock = DockStyle.Fill
            };

            panel.Controls.Add(label);
            return panel;
        }
    }
}
