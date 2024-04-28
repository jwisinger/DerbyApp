#warning 3: If no racer is added to a new race, it gets saved to the database without a format

using DerbyApp.RacerDatabase;
using DerbyApp.RaceStats;
using DerbyApp.Windows;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace DerbyApp.Pages
{
    public partial class EditRace : Page, INotifyPropertyChanged
    {
        private readonly Database _db;
        private int _raceFormatIndex = 0;
        public ObservableCollection<string> Races;
        public ObservableCollection<Racer> Racers = new();
        public ObservableCollection<Racer> AllRacers = new();
        public ObservableCollection<Racer> AvailableRacers = new();
        private Visibility _displayPhotos = Visibility.Collapsed;
        private int _heatCount = RaceHeats.Heats[RaceHeats.DefaultHeat].HeatCount;
        private int _racerCount = RaceHeats.Heats[RaceHeats.DefaultHeat].RacerCount;
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler RaceChanged;

        public int RaceFormatIndex
        {
            get => _raceFormatIndex;
            set
            {
                _raceFormatIndex = value;
                RaceFormat = RaceHeats.Heats[_raceFormatIndex].Name;
                _heatCount = RaceHeats.Heats[_raceFormatIndex].HeatCount;
                _racerCount = RaceHeats.Heats[_raceFormatIndex].RacerCount;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RaceFormat)));
            }
        }

        public string RaceFormat { get; set; }

        public string CurrentRace
        {
            get => (string)cbName.SelectedValue;
            set
            {
                int order = 1;
                int raceFormat;
                cbName.SelectedValue = value;
                (Racers, raceFormat) = _db.GetRacers((string)cbName.SelectedValue, Racers);
                if (raceFormat >= 0)
                {
                    _heatCount = RaceHeats.Heats[raceFormat].HeatCount;
                    _racerCount = RaceHeats.Heats[raceFormat].RacerCount;
                    RaceFormatIndex = raceFormat;
                }
                foreach (Racer r in Racers) r.RaceOrder = order++;
            }
        }

        public Visibility DisplayPhotos
        {
            get => _displayPhotos;
            set
            {
                _displayPhotos = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayPhotos)));
            }
        }

        public EditRace(Database db)
        {
            _db = db;
            InitializeComponent();
            Races = _db.GetListOfRaces();
            cbName.DataContext = Races;
            dataGridRacers.DataContext = Racers;
            cbLevels.ItemsSource = GirlScoutLevels.ScoutLevels;
            tbFormat.DataContext = this;

            UpdateRacerList();
        }

        public void UpdateRacerList()
        {
            AllRacers = _db.GetAllRacers();
            AvailableRacers.Clear();
            foreach (var item in GirlScoutLevels.ScoutLevels)
            {
                if (item.IsSelected)
                {
                    var racers = AllRacers.Where(x => x.Level == item.Level);
                    foreach (Racer r in racers) AvailableRacers.Add(r);
                }
            }
        }

        private void ComboBoxRaceName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CurrentRace = cbName.SelectedValue as string;
            try
            {
                if ((cbName.SelectedValue as string).Length > 0) buttonDeleteRace.IsEnabled = true;
                else buttonDeleteRace.IsEnabled = false;
            }
            catch
            {
                buttonDeleteRace.IsEnabled = false;
            }
            RaceChanged?.Invoke(this, null);
        }

        private void CbName_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var regex = new Regex(@"[^a-zA-Z0-9\s]");
            cbName.Text = regex.Replace(cbName.Text, "");
            if (cbName.Text.Length > 0) buttonDeleteRace.IsEnabled = true;
            else buttonDeleteRace.IsEnabled = false;
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            AvailableRacers.Clear();
            foreach (var item in GirlScoutLevels.ScoutLevels)
            {
                if (item.IsSelected)
                {
                    var racers = AllRacers.Where(x => x.Level == item.Level);
                    foreach (Racer r in racers) AvailableRacers.Add(r);
                }
            }
        }

        private void ButtonAddRacer_Click(object sender, RoutedEventArgs e)
        {
#warning 4: send up a signal here to change the racetracker (maybe warn user if race has already started)
            int order = 1;
            if (Racers.Count > _racerCount)
            {
                MessageBox.Show("Max number of racers already added.", "Max Racers Exceeded", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                AddRacer addRacerWindow = new(AllRacers, _racerCount - Racers.Count);
                addRacerWindow.ShowDialog();

                // Check if added racer already in list
                foreach (Racer racer in addRacerWindow.SelectedRacers)
                {
                    if (!Racers.Contains(racer)) Racers.Add(racer);
                }
                foreach (Racer r in Racers) r.RaceOrder = order++;
                _db.ModifyResultsTable(Racers, cbName.Text, _heatCount, RaceFormatIndex);
            }
            catch { }
        }

        private void Delete_OnClick(object sender, RoutedEventArgs e)
        {
#warning 4: send up a signal here to change the racetracker (maybe warn user if race has already started)
            int order = 1;
            try
            {
                Racers.RemoveAt(dataGridRacers.SelectedIndex);
                foreach (Racer r in Racers) r.RaceOrder = order++;
                _db.ModifyResultsTable(Racers, cbName.Text, _heatCount, 0);
            }
            catch { }
        }

        private void CbLevels_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cbLevels.Text = "A";
        }

        private void ButtonNewRace_Click(object sender, RoutedEventArgs e)
        {
            NewRace nr = new();
            if((bool)nr.ShowDialog())
            {
                if (!Races.Contains(nr.RaceName))
                {
                    Races.Add(nr.RaceName);
                    cbName.SelectedItem = nr.RaceName;
                    RaceFormatIndex = nr.RaceFormatIndex;
                    RaceFormat = RaceHeats.Heats[nr.RaceFormatIndex].Name;
                    Racers.Clear();
                }
                else
                {
                    MessageBox.Show("A race with that name already exists.", "Name Exists",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ButtonDeleteRace_Click(object sender, RoutedEventArgs e)
        {
            if (cbName.SelectedItem != null)
            {
                if(MessageBox.Show("This will delete the race named " + cbName.SelectedItem + " and all associated data. Are you sure?",
                    "Delete Race", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
                {
                    _db.DeleteResultsTable(cbName.SelectedItem as string);
                    Races.Remove(cbName.SelectedItem as string);
                }
            }
        }
    }
}
