using System;
using System.Drawing;
using System.Windows.Forms;
using IUIS.SharedUI.Forms;

namespace IUIS.UserApp.Forms.Employee.Library
{
    public partial class BookCreateEditDialog : AppDialogBase
    {
        private readonly string _sessionId;
        private readonly string _bookId;
        
        private TextBox _isbnTextBox;
        private TextBox _titleTextBox;
        private TextBox _authorTextBox;
        private TextBox _publisherTextBox;
        private NumericUpDown _publicationYearNumeric;
        private TextBox _genreTextBox;
        private TextBox _languageTextBox;
        private TextBox _shelfNumberTextBox;
        private NumericUpDown _totalCopiesNumeric;
        private ComboBox _conditionComboBox;
        private ComboBox _statusComboBox;
        private TextBox _publicNotesTextBox;
        private TextBox _internalRemarksTextBox;
        private Button _saveButton;
        private Button _cancelButton;

        public BookCreateEditDialog(string sessionId, string bookId = null)
        {
            _sessionId = sessionId ?? throw new ArgumentNullException(nameof(sessionId));
            _bookId = bookId;
            
            InitializeComponent();
            SetupLayout();
            
            if (!string.IsNullOrEmpty(_bookId))
            {
                this.Text = "Edit Book";
                LoadBookData();
            }
            else
            {
                this.Text = "Create Book";
            }
        }

        private void InitializeComponent()
        {
            this.Size = new Size(700, 650);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
        }

        private void SetupLayout()
        {
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                AutoScroll = true
            };

            int y = 0;
            
            AddField(mainPanel, "ISBN:", ref _isbnTextBox, ref y);
            AddField(mainPanel, "Title:", ref _titleTextBox, ref y);
            AddField(mainPanel, "Author:", ref _authorTextBox, ref y);
            AddField(mainPanel, "Publisher:", ref _publisherTextBox, ref y);
            
            var yearLabel = new Label
            {
                Text = "Publication Year:",
                Location = new Point(0, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F)
            };
            
            _publicationYearNumeric = new NumericUpDown
            {
                Location = new Point(120, y - 3),
                Width = 100,
                Minimum = 1900,
                Maximum = 2100,
                Value = 2024
            };
            
            mainPanel.Controls.Add(yearLabel);
            mainPanel.Controls.Add(_publicationYearNumeric);
            y += 35;
            
            AddField(mainPanel, "Genre:", ref _genreTextBox, ref y);
            AddField(mainPanel, "Language:", ref _languageTextBox, ref y);
            AddField(mainPanel, "Shelf Number:", ref _shelfNumberTextBox, ref y);
            
            var totalCopiesLabel = new Label
            {
                Text = "Total Copies:",
                Location = new Point(0, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F)
            };
            
            _totalCopiesNumeric = new NumericUpDown
            {
                Location = new Point(120, y - 3),
                Width = 100,
                Minimum = 0,
                Maximum = 1000,
                Value = 1
            };
            
            mainPanel.Controls.Add(totalCopiesLabel);
            mainPanel.Controls.Add(_totalCopiesNumeric);
            y += 35;
            
            AddComboBoxField(mainPanel, "Condition:", ref _conditionComboBox, new[] { "Good", "Fair", "Poor" }, ref y);
            AddComboBoxField(mainPanel, "Status:", ref _statusComboBox, new[] { "Draft", "Active", "Inactive", "Archived" }, ref y);
            
            var publicNotesLabel = new Label
            {
                Text = "Public Catalog Notes:",
                Location = new Point(0, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F)
            };
            
            _publicNotesTextBox = new TextBox
            {
                Location = new Point(0, y + 25),
                Width = 640,
                Height = 60,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 9F)
            };
            
            mainPanel.Controls.Add(publicNotesLabel);
            mainPanel.Controls.Add(_publicNotesTextBox);
            y += 95;
            
            var internalRemarksLabel = new Label
            {
                Text = "Internal Remarks:",
                Location = new Point(0, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F)
            };
            
            _internalRemarksTextBox = new TextBox
            {
                Location = new Point(0, y + 25),
                Width = 640,
                Height = 60,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 9F)
            };
            
            mainPanel.Controls.Add(internalRemarksLabel);
            mainPanel.Controls.Add(_internalRemarksTextBox);
            y += 95;
            
            var buttonPanel = new Panel
            {
                Location = new Point(0, y),
                Size = new Size(640, 40)
            };
            
            _saveButton = new Button
            {
                Text = "Save",
                Location = new Point(440, 0),
                Size = new Size(90, 35),
                BackColor = Color.FromArgb(79, 70, 229),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F)
            };
            _saveButton.FlatAppearance.BorderSize = 0;
            _saveButton.Click += OnSaveClick;
            
            _cancelButton = new Button
            {
                Text = "Cancel",
                Location = new Point(540, 0),
                Size = new Size(90, 35),
                BackColor = Color.FromArgb(229, 231, 235),
                ForeColor = Color.FromArgb(55, 65, 81),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F)
            };
            _cancelButton.FlatAppearance.BorderSize = 0;
            _cancelButton.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            
            buttonPanel.Controls.Add(_saveButton);
            buttonPanel.Controls.Add(_cancelButton);
            
            mainPanel.Controls.Add(buttonPanel);
            
            this.Controls.Add(mainPanel);
        }

        private void AddField(Panel panel, string labelText, ref TextBox textBox, ref int y)
        {
            var label = new Label
            {
                Text = labelText,
                Location = new Point(0, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F)
            };
            
            textBox = new TextBox
            {
                Location = new Point(120, y - 3),
                Width = 520,
                Height = 28,
                Font = new Font("Segoe UI", 9F)
            };
            
            panel.Controls.Add(label);
            panel.Controls.Add(textBox);
            y += 35;
        }

        private void AddComboBoxField(Panel panel, string labelText, ref ComboBox comboBox, string[] items, ref int y)
        {
            var label = new Label
            {
                Text = labelText,
                Location = new Point(0, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F)
            };
            
            comboBox = new ComboBox
            {
                Location = new Point(120, y - 3),
                Width = 200,
                Height = 28,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F)
            };
            comboBox.Items.AddRange(items);
            comboBox.SelectedIndex = 0;
            
            panel.Controls.Add(label);
            panel.Controls.Add(comboBox);
            y += 35;
        }

        private void LoadBookData()
        {
            // TODO: Load actual book data from service
            _isbnTextBox.Text = "978-0-123456-78-9";
            _titleTextBox.Text = "Introduction to Algorithms";
            _authorTextBox.Text = "Thomas H. Cormen";
        }

        private void OnSaveClick(object sender, EventArgs e)
        {
            // TODO: Implement save logic
            MessageBox.Show("Book saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.DialogResult = DialogResult.OK;
        }
    }
}
