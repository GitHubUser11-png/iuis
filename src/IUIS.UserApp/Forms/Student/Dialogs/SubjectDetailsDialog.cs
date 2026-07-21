using System;
using System.Drawing;
using System.Windows.Forms;
using IUIS.SharedUI.Controls;
using IUIS.SharedUI.Forms;
using IUIS.SharedUI.Theme;

namespace IUIS.UserApp.Forms.Student.Dialogs
{
    public partial class SubjectDetailsDialog : AppDialogBase
    {
        private readonly string _subjectId;
        
        private Panel _detailsPanel;
        private Button _closeButton;

        public SubjectDetailsDialog(string subjectId)
        {
            _subjectId = subjectId ?? throw new ArgumentNullException(nameof(subjectId));
            
            InitializeComponent();
            SetupLayout();
            LoadSubjectDetails();
        }

        private void InitializeComponent()
        {
            this.Text = "Subject Details";
            this.Size = new Size(520, 420);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            UiTheme.ApplyBaseFormStyle(this);
        }

        private void SetupLayout()
        {
            var mainPanel = DialogLayoutHelper.CreateMainPanel();

            var contentPanel = new Panel
            {
                Dock = DockStyle.Fill
            };

            _detailsPanel = DialogLayoutHelper.CreateDetailsPanel(
                new[] { "Subject ID", "Subject Code", "Subject Title", "Units", "Schedule", "Room", "Instructor" },
                new[] { _subjectId, "CS101", "Introduction to Computer Science", "3", "MWF 9:00-10:00 AM", "Room 101", "Dr. Smith" }
            );
            _detailsPanel.Dock = DockStyle.Fill;

            _closeButton = UiTheme.CreateSecondaryButton("Close", 120, UiMetrics.StandardButtonHeight);
            _closeButton.Click += (s, e) => this.DialogResult = DialogResult.OK;

            var buttonPanel = DialogLayoutHelper.CreateButtonPanel(_closeButton);

            contentPanel.Controls.Add(_detailsPanel);
            mainPanel.Controls.Add(contentPanel);
            mainPanel.Controls.Add(buttonPanel);

            this.Controls.Add(mainPanel);
        }

        private void LoadSubjectDetails()
        {
            try
            {
                // TODO: Load actual subject details from service
                // For now, placeholder data is set in SetupLayout
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading subject details: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
