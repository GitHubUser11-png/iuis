using System;
using System.Drawing;
using System.Windows.Forms;
using IUIS.SharedUI.Theme;

namespace IUIS.SharedUI.Controls
{
    public class SearchBarControl : UserControl
    {
        private TextBox _searchTextBox;
        private Button _searchButton;
        private Panel _containerPanel;

        public string SearchText
        {
            get => _searchTextBox.Text;
            set => _searchTextBox.Text = value;
        }

        public string PlaceholderText { get; set; } = "Search...";

        public event EventHandler SearchClicked;

        public SearchBarControl()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(400, 42);
            this.BackColor = Color.Transparent;

            _containerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            _searchTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = UiTheme.BodyFont,
                BorderStyle = BorderStyle.None,
                Padding = new Padding(8, 0, 0, 0),
                ForeColor = UiTheme.TextPrimary
            };
            _searchTextBox.GotFocus += (s, e) => 
            {
                if (_searchTextBox.Text == PlaceholderText)
                    _searchTextBox.Text = "";
            };
            _searchTextBox.LostFocus += (s, e) => 
            {
                if (string.IsNullOrWhiteSpace(_searchTextBox.Text))
                    _searchTextBox.Text = PlaceholderText;
            };
            _searchTextBox.KeyDown += (s, e) => 
            {
                if (e.KeyCode == Keys.Enter)
                    OnSearchClicked();
            };

            _searchButton = new Button
            {
                Text = "🔍",
                Width = 42,
                Dock = DockStyle.Right,
                FlatStyle = FlatStyle.Flat,
                BackColor = UiTheme.InstitutionalPrimary,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14F),
                Cursor = Cursors.Hand
            };
            _searchButton.FlatAppearance.BorderSize = 0;
            _searchButton.Click += (s, e) => OnSearchClicked();

            _containerPanel.Controls.Add(_searchTextBox);
            _containerPanel.Controls.Add(_searchButton);
            this.Controls.Add(_containerPanel);
        }

        private void OnSearchClicked()
        {
            SearchClicked?.Invoke(this, EventArgs.Empty);
        }

        public void Clear()
        {
            _searchTextBox.Text = PlaceholderText;
        }
    }
}
