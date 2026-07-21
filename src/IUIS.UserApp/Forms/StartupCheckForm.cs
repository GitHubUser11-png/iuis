using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

using IUIS.Application.Abstractions.Security;
using IUIS.Infrastructure.Presentation;
using IUIS.SharedUI;
using IUIS.SharedUI.Controls;
using IUIS.SharedUI.Theme;
using AppIdentity = IUIS.SharedUI.ApplicationIdentity;

namespace IUIS.UserApp.Forms
{
    internal sealed class StartupCheckForm : Form
    {
        private readonly ApplicationRuntime _runtime;
        private readonly Label _statusLabel;
        private readonly ProgressBar _progressBar;
        private readonly Label _versionLabel;
        private readonly BackgroundWorker _worker;
        private bool _ready;
        private string _failureMessage;

        public StartupCheckForm(ApplicationRuntime runtime)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));

            Text = AppIdentity.ProductName;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterScreen;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(640, 360);
            UiTheme.ApplyBaseFormStyle(this);

            var identity = new ApplicationIdentityPanel();
            identity.Location = new Point(48, 32);

            _statusLabel = new Label();
            _statusLabel.Text = "Initializing system…";
            _statusLabel.Font = UiTheme.BodyFont;
            _statusLabel.ForeColor = UiTheme.TextSecondary;
            _statusLabel.AutoSize = false;
            _statusLabel.TextAlign = ContentAlignment.MiddleCenter;
            _statusLabel.Location = new Point(48, 160);
            _statusLabel.Size = new Size(544, 32);

            _progressBar = new ProgressBar();
            _progressBar.Style = ProgressBarStyle.Marquee;
            _progressBar.MarqueeAnimationSpeed = 25;
            _progressBar.Location = new Point(120, 204);
            _progressBar.Size = new Size(400, 18);

            _versionLabel = new Label();
            _versionLabel.Text = AppIdentity.VersionLabel;
            _versionLabel.Font = UiTheme.CaptionFont;
            _versionLabel.ForeColor = UiTheme.TextSecondary;
            _versionLabel.AutoSize = true;
            _versionLabel.Location = new Point(48, 300);

            Controls.Add(identity);
            Controls.Add(_statusLabel);
            Controls.Add(_progressBar);
            Controls.Add(_versionLabel);

            _worker = new BackgroundWorker();
            _worker.DoWork += WorkerDoWork;
            _worker.RunWorkerCompleted += WorkerCompleted;
            Shown += delegate { _worker.RunWorkerAsync(); };
        }

        public bool StartupReady
        {
            get { return _ready; }
        }

        public string FailureMessage
        {
            get { return _failureMessage; }
        }

        private void WorkerDoWork(object sender, DoWorkEventArgs e)
        {
            var messages = new[]
            {
                "Loading configuration",
                "Checking repository location",
                "Validating required files",
                "Checking incomplete transactions",
                "Loading authentication service"
            };

            for (var index = 0; index < messages.Length; index++)
            {
                _worker.ReportProgress(index);
                Thread.Sleep(180);
            }

            string statusMessage;
            _ready = _runtime.StartupReadiness.IsRepositoryReady(out statusMessage);
            _failureMessage = _ready ? null : statusMessage;
        }

        private void WorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (_ready)
            {
                _statusLabel.Text = "Ready";
                DialogResult = DialogResult.OK;
                Close();
                return;
            }

            using (var failure = new StartupFailureForm(_failureMessage, false))
            {
                failure.ShowDialog(this);
                DialogResult = failure.RetryRequested ? DialogResult.Retry : DialogResult.Cancel;
            }

            Close();
        }
    }
}
