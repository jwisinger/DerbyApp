using System;
using System.Windows;

namespace DerbyApp.Windows
{
    /// <summary>
    /// Interaction logic for InputBox.xaml
    /// </summary>
    public partial class InputBox : Window
    {
        private String _input;
        private String _prompt;

        public String Input
        {
            get => _input;
            set => _input = value;
        }

        public String Prompt
        {
            get => _prompt;
            set => _prompt = value;
        }

        public InputBox(String prompt, String input)
        {
            InitializeComponent();
            _prompt = prompt;
            _input = input;
            promptBlock.DataContext = this;
            inputBox.DataContext = this;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
