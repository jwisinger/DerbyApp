using System.Windows;
using System.Windows.Input;

namespace DerbyApp.Windows
{
    /// <summary>
    /// Interaction logic for PasswordInputBox.xaml
    /// </summary>
    public partial class PasswordInputBox : Window
    {
        public string Password;

        public PasswordInputBox()
        {
            InitializeComponent();
            FocusManager.SetFocusedElement(this, passwordBox);
            Keyboard.Focus(passwordBox);
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            Password = passwordBox.Password;
            Close();
        }
    }
}
