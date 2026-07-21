using System;
using System.Drawing;
using System.Windows.Forms;
using IUIS.SharedUI.Controls;
using IUIS.SharedUI.DataGridViews;

namespace IUIS.UserApp.Forms.Employee.Library
{
    public partial class BookInventoryPage : UserControl
    {
        private readonly string _sessionId;
        
        private FilterBarControl _filterBar;
        private DataGridView _booksGrid;
        private Button _createBookButton;
        private Button _refreshButton;

        public BookInventoryPage(string sessionId)
        {
            _sessionId = sessionId ?? throw new ArgumentNullException(nameof(sessionId));
            
            InitializeComponent();
            SetupLayout();
            LoadBooks();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(1200, 800);
            this.BackColor = Color.FromArgb(249, 250, 251);
        }

        private void SetupLayout()
        {
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                BackColor = Color.FromArgb(249, 250, 251)
            };

            var headerPanel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(1160, 40)
            };

            var headerLabel = new Label
            {
                Text = "Book Inventory",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                Location = new Point(0, 5),
                AutoSize = true
            };

            _createBookButton = new Button
            {
                Text = "Create Book",
                Location = new Point(900, 0),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(79, 70, 229),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F)
            };
            _createBookButton.FlatAppearance.BorderSize = 0;
            _createBookButton.Click += OnCreateBookClick;

            _refreshButton = new Button
            {
                Text = "Refresh",
                Location = new Point(1030, 0),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(229, 231, 235),
                ForeColor = Color.FromArgb(55, 65, 81),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F)
            };
            _refreshButton.FlatAppearance.BorderSize = 0;
            _refreshButton.Click += (s, e) => LoadBooks();

            headerPanel.Controls.Add(headerLabel);
            headerPanel.Controls.Add(_createBookButton);
            headerPanel.Controls.Add(_refreshButton);

            _filterBar = new FilterBarControl
            {
                Location = new Point(0, 50),
                Size = new Size(1160, 40)
            };
            _filterBar.SetFilterOptions(new[] { "All", "Available", "Borrowed", "Maintenance", "Lost" });

            _booksGrid = AppDataGridViewFactory.CreateStyledDataGridView();
            _booksGrid.Location = new Point(0, 100);
            _booksGrid.Size = new Size(1160, 650);
            AppDataGridViewFactory.AddTextBoxColumn(_booksGrid, "BookId", "ID", 120);
            AppDataGridViewFactory.AddTextBoxColumn(_booksGrid, "Isbn", "ISBN", 120);
            AppDataGridViewFactory.AddTextBoxColumn(_booksGrid, "Title", "Title", 250);
            AppDataGridViewFactory.AddTextBoxColumn(_booksGrid, "Author", "Author", 150);
            AppDataGridViewFactory.AddTextBoxColumn(_booksGrid, "ShelfNumber", "Shelf", 80);
            AppDataGridViewFactory.AddTextBoxColumn(_booksGrid, "TotalCopies", "Total", 60);
            AppDataGridViewFactory.AddTextBoxColumn(_booksGrid, "CopiesAvailable", "Available", 80);
            AppDataGridViewFactory.AddTextBoxColumn(_booksGrid, " ActiveBorrowedCopies", "Borrowed", 80);
            AppDataGridViewFactory.AddTextBoxColumn(_booksGrid, "CopiesUnderMaintenance", "Maint", 60);
            AppDataGridViewFactory.AddTextBoxColumn(_booksGrid, "CopiesLost", "Lost", 60);
            AppDataGridViewFactory.AddTextBoxColumn(_booksGrid, "ConditionDisplay", "Condition", 100);
            AppDataGridViewFactory.AddTextBoxColumn(_booksGrid, "StatusDisplay", "Status", 100);
            AppDataGridViewFactory.AddButtonColumn(_booksGrid, "View", "View", 60);
            AppDataGridViewFactory.AddButtonColumn(_booksGrid, "Edit", "Edit", 60);
            AppDataGridViewFactory.AddButtonColumn(_booksGrid, "Adjust", "Adjust", 60);

            _filterBar.SearchRequested += (s, e) => LoadBooks();
            _filterBar.ClearRequested += (s, e) => LoadBooks();
            _booksGrid.CellClick += OnBooksGridCellClick;

            mainPanel.Controls.Add(headerPanel);
            mainPanel.Controls.Add(_filterBar);
            mainPanel.Controls.Add(_booksGrid);

            this.Controls.Add(mainPanel);
        }

        private void LoadBooks()
        {
            try
            {
                // TODO: Load actual books from service
                // For now, placeholder
                _booksGrid.DataSource = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading books: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void OnCreateBookClick(object sender, EventArgs e)
        {
            var dialog = new BookCreateEditDialog(_sessionId);
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                LoadBooks();
            }
        }

        private void OnBooksGridCellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            var bookId = _booksGrid.Rows[e.RowIndex].Cells["BookId"].Value?.ToString();
            if (string.IsNullOrEmpty(bookId)) return;

            if (_booksGrid.Columns[e.ColumnIndex].Name == "View")
            {
                var dialog = new BookDetailsDialog(bookId);
                dialog.ShowDialog();
            }
            else if (_booksGrid.Columns[e.ColumnIndex].Name == "Edit")
            {
                var dialog = new BookCreateEditDialog(_sessionId, bookId);
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    LoadBooks();
                }
            }
            else if (_booksGrid.Columns[e.ColumnIndex].Name == "Adjust")
            {
                var dialog = new BookInventoryAdjustDialog(bookId);
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    LoadBooks();
                }
            }
        }
    }
}
