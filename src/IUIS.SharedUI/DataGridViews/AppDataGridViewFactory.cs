using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

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
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D,
                EnableHeadersVisualStyles = false,
                GridColor = Color.FromArgb(229, 231, 235),
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(55, 65, 81),
                BackColor = Color.White,
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(249, 250, 251)
                }
            };

            dataGridView.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(243, 244, 246),
                ForeColor = Color.FromArgb(55, 65, 81),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                Padding = new Padding(8, 4, 8, 4)
            };

            dataGridView.DefaultCellStyle = new DataGridViewCellStyle
            {
                Padding = new Padding(8, 4, 8, 4),
                SelectionBackColor = Color.FromArgb(79, 70, 229),
                SelectionForeColor = Color.White
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
