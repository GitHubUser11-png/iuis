using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace IUIS.SharedUI.Controls
{
    public partial class FilterBarControl : UserControl
    {
        private FlowLayoutPanel _flowPanel;
        private TextBox _searchTextBox;
        private ComboBox _filterComboBox;
        private Button _applyButton;
        private Button _clearButton;

        public event EventHandler SearchRequested;
        public event EventHandler FilterChanged;
        public event EventHandler ClearRequested;

        public FilterBarControl()
        {
            InitializeComponent();
            SetupLayout();
        }

        public string SearchText
        {
            get => _searchTextBox.Text;
            set => _searchTextBox.Text = value;
        }

        public string SelectedFilter
        {
            get => _filterComboBox.SelectedItem?.ToString();
            set
            {
                int index = _filterComboBox.Items.IndexOf(value);
                if (index >= 0)
                    _filterComboBox.SelectedIndex = index;
            }
        }

        public void SetFilterOptions(IEnumerable<string> options)
        {
            _filterComboBox.Items.Clear();
            foreach (var option in options)
            {
                _filterComboBox.Items.Add(option);
            }
            if (_filterComboBox.Items.Count > 0)
                _filterComboBox.SelectedIndex = 0;
        }

        private void InitializeComponent()
        {
            this.Size = new Size(600, 40);
            this.BackColor = Color.White;
        }

        private void SetupLayout()
        {
            _flowPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.White,
                Padding = new Padding(5)
            };

            _searchTextBox = new TextBox
            {
                Width = 200,
                Height = 28,
                Text = "Search...",
                Font = new Font("Segoe UI", 9F)
            };
            _searchTextBox.GotFocus += (s, e) =>
            {
                if (_searchTextBox.Text == "Search...")
                    _searchTextBox.Text = string.Empty;
            };
            _searchTextBox.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(_searchTextBox.Text))
                    _searchTextBox.Text = "Search...";
            };

            _filterComboBox = new ComboBox
            {
                Width = 150,
                Height = 28,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F)
            };

            _applyButton = new Button
            {
                Text = "Apply",
                Width = 80,
                Height = 28,
                BackColor = Color.FromArgb(79, 70, 229),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F)
            };
            _applyButton.FlatAppearance.BorderSize = 0;

            _clearButton = new Button
            {
                Text = "Clear",
                Width = 80,
                Height = 28,
                BackColor = Color.FromArgb(229, 231, 235),
                ForeColor = Color.FromArgb(55, 65, 81),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F)
            };
            _clearButton.FlatAppearance.BorderSize = 0;

            _searchTextBox.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                    SearchRequested?.Invoke(this, EventArgs.Empty);
            };

            _filterComboBox.SelectedIndexChanged += (s, e) =>
                FilterChanged?.Invoke(this, EventArgs.Empty);

            _applyButton.Click += (s, e) =>
                SearchRequested?.Invoke(this, EventArgs.Empty);

            _clearButton.Click += (s, e) =>
            {
                _searchTextBox.Clear();
                _filterComboBox.SelectedIndex = -1;
                ClearRequested?.Invoke(this, EventArgs.Empty);
            };

            _flowPanel.Controls.Add(_searchTextBox);
            _flowPanel.Controls.Add(_filterComboBox);
            _flowPanel.Controls.Add(_applyButton);
            _flowPanel.Controls.Add(_clearButton);

            this.Controls.Add(_flowPanel);
        }
    }

}
