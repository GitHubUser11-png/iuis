using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace IUIS.SharedUI.Navigation
{
    public class FormNavigationService
    {
        private readonly Form _ownerForm;
        private readonly Stack<Form> _navigationStack;
        private readonly Dictionary<string, Type> _registeredPages;

        public FormNavigationService(Form ownerForm)
        {
            _ownerForm = ownerForm ?? throw new ArgumentNullException(nameof(ownerForm));
            _navigationStack = new Stack<Form>();
            _registeredPages = new Dictionary<string, Type>();
        }

        public void RegisterPage<T>(string key) where T : Form
        {
            _registeredPages[key] = typeof(T);
        }

        public void NavigateTo(string key, object parameter = null)
        {
            if (!_registeredPages.ContainsKey(key))
                throw new ArgumentException($"Page with key '{key}' is not registered.");

            var pageType = _registeredPages[key];
            var page = (Form)Activator.CreateInstance(pageType);

            if (parameter != null && page is IParameterReceiver receiver)
            {
                receiver.ReceiveParameter(parameter);
            }

            _navigationStack.Push(_ownerForm);
            page.Show();
            _ownerForm.Hide();
        }

        public void GoBack()
        {
            if (_navigationStack.Count == 0)
                return;

            var previousForm = _navigationStack.Pop();
            previousForm.Show();
            _ownerForm.Close();
        }

        public bool CanGoBack => _navigationStack.Count > 0;

        public void ClearHistory()
        {
            _navigationStack.Clear();
        }
    }

    public interface IParameterReceiver
    {
        void ReceiveParameter(object parameter);
    }
}
