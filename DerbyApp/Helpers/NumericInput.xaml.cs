using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DerbyApp.Helpers
{
    /// <summary>
    /// Interaction logic for NumericInput.xaml
    /// </summary>
    public partial class NumericInput : Window
    {
        private int _input;
        private String _prompt;

        public int Input
        {
            get => _input;
            set => _input = value;
        }

        public String Prompt
        {
            get => _prompt;
            set => _prompt = value;
        }

        public NumericInput(String prompt, int input)
        {
            InitializeComponent();
            _prompt = prompt;
            _input = input;
            promptBlock.DataContext = this;
            inputBox.DataContext = this;
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = IsNumber();
            e.Handled = regex.IsMatch(e.Text);
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        [GeneratedRegex("[^0-9]+")]
        private static partial Regex IsNumber();
    }
}
