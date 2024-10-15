using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using DerbyApp.RaceStats;

namespace DerbyApp.Windows
{
    /// <summary>
    /// Interaction logic for AddRacer.xaml
    /// </summary>
    public partial class AddRacer : Window
    {
        private readonly int _maxRacers;
        public List<Racer> SelectedRacers = [];
        public AddRacer(IEnumerable<Racer> racer, int maxRacers)
        {
            InitializeComponent();
            lb_Racers.DataContext = racer;
            _maxRacers = maxRacers;
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            if (lb_Racers.SelectedItems.Count > _maxRacers)
            {
                MessageBox.Show("The max number of additional racers for this race format is " + _maxRacers + ".",
                    "Too Many Racers Selected", MessageBoxButton.OK, MessageBoxImage.Error);
                e.Handled = true;
            }
            else
            {
                foreach (var racer in lb_Racers.SelectedItems) { SelectedRacers.Add(racer as Racer); }
                DialogResult = true;
            }
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
