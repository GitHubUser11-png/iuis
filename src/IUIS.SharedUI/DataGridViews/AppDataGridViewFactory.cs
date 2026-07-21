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
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                EnableHeadersVisualStyles = false,
                GridColor = Color.FromArgb(232, 236, 240),
                Font = UiTheme.FieldLabelFont,
                ForeColor = UiTheme.TextPrimary,
                BackColor = UiTheme.ElevatedSurface,
                RowTemplate = { Height = UiMetrics.GridRowHeight },
                ColumnHeadersHeight = UiMetrics.GridHeaderHeight,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                AllowUserToResizeRows = false,
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(249, 250, 252)
                }
            };

            dataGridView.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = UiTheme.SurfaceSunken,
                ForeColor = UiTheme.TextSecondary,
                SelectionBackColor = UiTheme.SurfaceSunken,
                SelectionForeColor = UiTheme.TextSecondary,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                Padding = new Padding(12, 6, 12, 6)
            };

            dataGridView.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = UiTheme.ElevatedSurface,
                ForeColor = UiTheme.TextPrimary,
                Padding = new Padding(12, 6, 12, 6),
                SelectionBackColor = UiTheme.PrimarySoft,
                SelectionForeColor = UiTheme.TextPrimary
            };

            AttachRowHoverHighlight(dataGridView);

            return dataGridView;
        }

        /// <summary>
        /// Hosts a grid inside a rounded, bordered card so every table
        /// gets consistent modern framing. Dock the returned panel.
        /// </summary>
        public static Panel WrapInRoundedCard(DataGridView dataGridView)
        {
            var card = new RoundedGridCardPanel();
            dataGridView.Dock = DockStyle.Fill;
            card.Controls.Add(dataGridView);
            return card;
        }

        private static void AttachRowHoverHighlight(DataGridView dataGridView)
        {
            dataGridView.CellMouseEnter += delegate (object sender, DataGridViewCellEventArgs e)
            {
                var grid = (DataGridView)sender;
                if (e.RowIndex < 0 || e.RowIndex >= grid.Rows.Count)
                    return;
                var row = grid.Rows[e.RowIndex];
                if (!row.Selected)
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(238, 243, 249);
                }
            };

            dataGridView.CellMouseLeave += delegate (object sender, DataGridViewCellEventArgs e)
            {
                var grid = (DataGridView)sender;
                if (e.RowIndex < 0 || e.RowIndex >= grid.Rows.Count)
                    return;
                var row = grid.Rows[e.RowIndex];
                row.DefaultCellStyle.BackColor = Color.Empty;
            };
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

            column.FlatStyle = FlatStyle.Flat;
            column.DefaultCellStyle.BackColor = UiTheme.ElevatedSurface;
            column.DefaultCellStyle.ForeColor = UiTheme.InstitutionalPrimary;
            column.DefaultCellStyle.SelectionBackColor = UiTheme.PrimarySoft;
            column.DefaultCellStyle.SelectionForeColor = UiTheme.InstitutionalPrimary;

            dataGridView.Columns.Add(column);
        }

        /// <summary>
        /// Rounded, bordered panel that hosts a DataGridView and clips it
        /// to rounded corners.
        /// </summary>
        private sealed class RoundedGridCardPanel : Panel
        {
            public RoundedGridCardPanel()
            {
                SetStyle(
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.UserPaint |
                    ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.ResizeRedraw,
                    true);
                Dock = DockStyle.Fill;
                BackColor = UiTheme.ElevatedSurface;
                Padding = new Padding(1);
            }

            protected override void OnResize(EventArgs eventargs)
            {
                base.OnResize(eventargs);
                UiPainting.ApplyRoundedRegion(this, UiMetrics.CardCornerRadius);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                UiPainting.PaintRoundedSurface(
                    e.Graphics,
                    ClientRectangle,
                    UiMetrics.CardCornerRadius,
                    Color.Empty,
                    UiTheme.BorderNeutral);
            }
        }
    }
}
