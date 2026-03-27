using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace DerbyApp.Windows
{
    /// <summary>
    /// Interaction logic for InputBox.xaml
    /// </summary>
    public partial class InputBox : Window
    {
        private String _input;
        private String _prompt;
        private readonly bool _specialCharactersAllowed;

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

        public InputBox(String prompt, String input, bool specialCharactersAllowed, bool returnAllowed)
        {
            InitializeComponent();
            _prompt = prompt;
            _input = input;
            promptBlock.DataContext = this;
            inputBox.DataContext = this;
            inputBox.AcceptsReturn = returnAllowed;
            _specialCharactersAllowed = specialCharactersAllowed;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void InputBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!_specialCharactersAllowed)
            {
                // Regex that matches any character that is NOT a number (0-9) or a letter (a-z, A-Z)
                Regex regex = MyRegex();
                // If the new character is a special character, handle the event (stop it from being entered)
                e.Handled = regex.IsMatch(e.Text);
            }
        }

        private void InputBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!_specialCharactersAllowed) if (e.Key == Key.Space) e.Handled = true;
        }

        private void InputBox_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (!_specialCharactersAllowed)
            {
                if (e.Command == ApplicationCommands.Paste)
                {
                    e.Handled = true;
                }
            }
        }

        [GeneratedRegex("[^a-zA-Z0-9]+")]
        private static partial Regex MyRegex();
    }
}
