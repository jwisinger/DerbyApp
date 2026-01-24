using System.Windows;

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
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            Password = passwordBox.Password;
            Close();
        }
    }
}
