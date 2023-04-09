using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DerbyApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Database _db = new();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonAddRacer_Click(object sender, RoutedEventArgs e)
        {
            NewRacer nr = new();
            if (nr.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _db.AddRacer(nr.Racer);
            }
        }

        private void ButtonViewRacerTable_Click(object sender, RoutedEventArgs e)
        {
            new RacerTableView(_db).ShowDialog();
            new RaceTracker().Show();
        }

        private void ButtonCreateRace_Click(object sender, RoutedEventArgs e)
        {
            NewRace nr = new(_db);
            if (nr.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if(_db.CreateRaceTable(nr.Race))
                {
                    nr.Race.InProgress = true;
                }
            }
        }
    }
}
