using System;
using System.Windows.Forms;

namespace IUIS.SharedUI.Application
{
    public class TransitionAwareApplicationContext : ApplicationContext
    {
        public TransitionAwareApplicationContext()
        {
        }

        public TransitionAwareApplicationContext(Form initialForm)
        {
            Begin(initialForm);
        }

        public void Begin(Form initialForm)
        {
            if (initialForm == null)
                throw new ArgumentNullException(nameof(initialForm));

            MainForm = initialForm;
            initialForm.FormClosed += MainFormClosed;
            initialForm.Show();
        }

        public void TransitionTo(Form nextForm)
        {
            if (nextForm == null)
                throw new ArgumentNullException(nameof(nextForm));

            var previous = MainForm;
            MainForm = nextForm;
            nextForm.FormClosed += MainFormClosed;
            nextForm.Show();

            if (previous != null)
            {
                previous.FormClosed -= MainFormClosed;
                previous.Close();
            }
        }

        public void EndApplication()
        {
            ExitThread();
        }

        private void MainFormClosed(object sender, FormClosedEventArgs e)
        {
            if (MainForm == sender)
                ExitThread();
        }
    }
}
