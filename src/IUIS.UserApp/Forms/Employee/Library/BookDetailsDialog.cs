using System;
using System.Drawing;
using System.Windows.Forms;
using IUIS.SharedUI.Forms;
using IUIS.SharedUI.Theme;

namespace IUIS.UserApp.Forms.Employee.Library
{
    public partial class BookDetailsDialog : AppDialogBase
    {
        private readonly string _bookId;
        
        private Panel _detailsPanel;
        private Button _closeButton;

        public BookDetailsDialog(string bookId)
        {
            _bookId = bookId ?? throw new ArgumentNullException(nameof(bookId));
            
            InitializeComponent();
            SetupLayout();
            LoadBookDetails();
        }

        private void InitializeComponent()
        {
            this.Text = "Book Details";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            UiTheme.ApplyBaseFormStyle(this);
        }

        private void SetupLayout()
        {
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                AutoScroll = true
            };

            _detailsPanel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(540, 380),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            _closeButton = UiTheme.CreateSecondaryButton("Close", 120, UiMetrics.StandardButtonHeight);
            _closeButton.Location = new Point(440, 400);
            _closeButton.Click += (s, e) => this.DialogResult = DialogResult.OK;

            mainPanel.Controls.Add(_detailsPanel);
            mainPanel.Controls.Add(_closeButton);

            this.Controls.Add(mainPanel);
        }

        private void LoadBookDetails()
        {
            try
            {
                // TODO: Load actual book details from service
                // For now, display placeholder
                _detailsPanel.Controls.Clear();

                var labels = new[]
                {
                    ("Book ID", _bookId),
                    ("ISBN", "978-0-123456-78-9"),
                    ("Title", "Introduction to Algorithms"),
                    ("Author", "Thomas H. Cormen"),
                    ("Publisher", "MIT Press"),
                    ("Publication Year", "2022"),
                    ("Genre", "Computer Science"),
                    ("Language", "English"),
                    ("Shelf Number", "CS-101"),
                    ("Total Copies", "5"),
                    ("Copies Available", "3"),
                    ("Copies Under Maintenance", "1"),
                    ("Copies Lost", "1"),
                    ("Condition", "Good"),
                    ("Status", "Active")
                };

                int x = 20;
                int y = 20;
                foreach (var (label, value) in labels)
                {
                    var labelControl = new Label
                    {
                        Text = label,
                        Font = UiTheme.CaptionFont,
                        ForeColor = UiTheme.TextSecondary,
                        Location = new Point(x, y),
                        AutoSize = true
                    };

                    var valueControl = new Label
                    {
                        Text = value,
                        Font = UiTheme.BodyFont,
                        ForeColor = UiTheme.TextPrimary,
                        Location = new Point(x, y + 18),
                        AutoSize = true
                    };

                    _detailsPanel.Controls.Add(labelControl);
                    _detailsPanel.Controls.Add(valueControl);

                    x += 270;
                    if (x > 250)
                    {
                        x = 20;
                        y += 45;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading book details: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
