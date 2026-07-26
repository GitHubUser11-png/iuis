using System;
using System.Windows.Forms;
using System.Drawing;
using IUIS.SharedUI.Theme;
using System.Timers;

namespace IUIS.SharedUI.Controls
{
    public partial class SearchBarControl : UserControl
    {
        private TextBox _txtSearch;
        private Button _btnClear;
        private System.Timers.Timer _debounceTimer;

        public event EventHandler<string> SearchChanged;

        public SearchBarControl()
        {
            InitializeComponent();
            SetupDebounce();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            _txtSearch = new TextBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 10F),
                ForeColor = UiTheme.TextPrimary,
                BackColor = UiTheme.Surface,
                Padding = new Padding(10, 8, 10, 8)
            };
            _txtSearch.TextChanged += TxtSearch_TextChanged;

            _btnClear = new Button
            {
                Dock = DockStyle.Right,
                Width = 40,
                FlatStyle = FlatStyle.Flat,
                Text = "✕",
                Font = new Font("Segoe UI", 12F),
                ForeColor = UiTheme.TextSecondary,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };
            _btnClear.FlatAppearance.BorderSize = 0;
            _btnClear.Click += BtnClear_Click;

            this.Controls.Add(_txtSearch);
            this.Controls.Add(_btnClear);
            this.BackColor = UiTheme.Surface;
            this.Padding = new Padding(10, 0, 0, 0);
            
            this.ResumeLayout(false);
        }

        private void SetupDebounce()
        {
            _debounceTimer = new System.Timers.Timer(300); // 300ms debounce
            _debounceTimer.Elapsed += (s, e) =>
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => SearchChanged?.Invoke(this, _txtSearch.Text)));
                }
                else
                {
                    SearchChanged?.Invoke(this, _txtSearch.Text);
                }
                _debounceTimer.Stop();
            };
            _debounceTimer.AutoReset = false;
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            _btnClear.Visible = !string.IsNullOrEmpty(_txtSearch.Text);
            _debounceTimer.Stop();
            _debounceTimer.Start();
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            _txtSearch.Clear();
            _btnClear.Visible = false;
            SearchChanged?.Invoke(this, string.Empty);
        }

        public string SearchText
        {
            get => _txtSearch.Text;
            set => _txtSearch.Text = value;
        }
    }
}
