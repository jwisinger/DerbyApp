using DerbyApp.RaceStats;
using System.Windows;

namespace DerbyApp.Windows
{
    /// <summary>
    /// Interaction logic for NewRace.xaml
    /// </summary>
    public partial class NewRace : Window
    {
        public string RaceName { get; set; }
        public int RaceFormatIndex { get; set; }

        public NewRace()
        {
            InitializeComponent();
            cbFormat.ItemsSource = RaceFormats.Formats;
            cbFormat.SelectedIndex = 0;
            tbRaceName.DataContext = this;
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            if (RaceName == null || RaceName == "")
            {
                MessageBox.Show("You must enter a name for the race.",
                    "No Name Entered", MessageBoxButton.OK, MessageBoxImage.Error);
                e.Handled = true;
            }
            else
            {
                RaceFormatIndex = cbFormat.SelectedIndex;
                DialogResult = true;
            }
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
