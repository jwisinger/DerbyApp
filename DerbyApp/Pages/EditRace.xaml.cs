using DerbyApp.Helpers;
using DerbyApp.RacerDatabase;
using DerbyApp.RaceStats;
using DerbyApp.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace DerbyApp.Pages
{
    public partial class EditRace : Page, INotifyPropertyChanged
    {
        private readonly Database _db;
        private string _currentRaceName;
        private int _raceFormatIndex = 0;
        public ObservableCollection<string> Races;
        public ObservableCollection<Racer> Racers = [];
        public ObservableCollection<Racer> AllRacers = [];
        public ObservableCollection<Racer> AvailableRacers = [];
        private Visibility _displayPhotos = Visibility.Visible;
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<bool> RaceChanged;
        public event EventHandler<ResponseEventArgs> RaceChanging;

        public string RaceFormatNameString { get; set; }

        public int RaceFormatIndex
        {
            get => _raceFormatIndex;
            set
            {
                _raceFormatIndex = value;
                RaceFormatNameString = RaceFormats.Formats[_raceFormatIndex].Name;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RaceFormatNameString)));
            }
        }

        public string CurrentRaceName
        {
            get => _currentRaceName;
            set
            {
                cbName.SelectedValue = value;
                _currentRaceName = value;
                (Racers, int raceFormat) = _db.GetRacers((string)cbName.SelectedValue, Racers);
                if (raceFormat >= 0) RaceFormatIndex = raceFormat;
                int order = 1;
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
                    foreach (Racer r in racers)
                    {
                        if (!Racers.Where(x => x.Number == r.Number).Any()) AvailableRacers.Add(r);
                    }
                }
            }
        }

        private void ComboBoxRaceName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_db.IsSynced)
            {
                _db.ModifyResultsTable(Racers, cbName.Text, RaceFormats.Formats[RaceFormatIndex].HeatCount, RaceFormatIndex);
                if (!_db.IsSynced)
                {
                    if(MessageBoxResult.Cancel == MessageBox.Show("Connection to the database has been lost. If you continue, any results from this race will be lost.",
                                    "Database Connection Lost", MessageBoxButton.OKCancel, MessageBoxImage.Warning))
                    {
                        cbName.SelectedValue = CurrentRaceName;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(cbName.SelectedValue)));
                        return;
                    }
                }
            }
            CurrentRaceName = cbName.SelectedValue as string;
            try
            {
                if (CurrentRaceName == null) buttonDeleteRace.IsEnabled = false;
                else if (CurrentRaceName.Length > 0) buttonDeleteRace.IsEnabled = true;
                else buttonDeleteRace.IsEnabled = false;
            }
            catch
            {
                buttonDeleteRace.IsEnabled = false;
            }
            RaceChanged?.Invoke(this, true);
        }

        private void CbName_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var regex = AlphaNumericRegex();
            cbName.Text = regex.Replace(cbName.Text, "");
            if (cbName.Text.Length > 0) buttonDeleteRace.IsEnabled = true;
            else buttonDeleteRace.IsEnabled = false;
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            UpdateRacerList();
        }

        private void ButtonAddRacer_Click(object sender, RoutedEventArgs e)
        {
            ResponseEventArgs response = new();
            RaceChanging?.Invoke(this, response);
            if (response.Continue)
            {
                int order = 1;
                if (Racers.Count > RaceFormats.Formats[RaceFormatIndex].RacerCount)
                {
                    MessageBox.Show("Max number of racers already added.", "Max Racers Exceeded", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                try
                {
                    AddRacer addRacerWindow = new(AvailableRacers, RaceFormats.Formats[RaceFormatIndex].RacerCount - Racers.Count);
                    addRacerWindow.ShowDialog();

                    // Check if added racer already in list
                    foreach (Racer racer in addRacerWindow.SelectedRacers)
                    {
                        IEnumerable<Racer> matches = Racers.Where(x => x.Number == racer.Number);
                        if (!matches.Any()) Racers.Add(racer);
                        else MessageBox.Show("Racer " + racer.RacerName + " is already in the list.", "Duplicate Racer", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    foreach (Racer r in Racers) r.RaceOrder = order++;
                    _db.ModifyResultsTable(Racers, cbName.Text, RaceFormats.Formats[RaceFormatIndex].HeatCount, RaceFormatIndex);
                    UpdateRacerList();
                }
                catch { }
                RaceChanged?.Invoke(this, false);
            }
        }

        private void Delete_OnClick(object sender, RoutedEventArgs e)
        {
            ResponseEventArgs response = new();
            RaceChanging?.Invoke(this, response);
            if (response.Continue)
            {
                int order = 1;
                try
                {
                    Racers.RemoveAt(dataGridRacers.SelectedIndex);
                    foreach (Racer r in Racers) r.RaceOrder = order++;
                    _db.ModifyResultsTable(Racers, cbName.Text, RaceFormats.Formats[RaceFormatIndex].HeatCount, 0);
                }
                catch { }
                UpdateRacerList();
                RaceChanged?.Invoke(this, false);
            }
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
                    RaceFormatNameString = RaceFormats.Formats[nr.RaceFormatIndex].Name;
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

        private void ZoomPicture(object sender, RoutedEventArgs e)
        {
            new ImageDisplay((sender as Image).Source).ShowDialog();
        }

        [GeneratedRegex(@"[^a-zA-Z0-9\s]")]
        private static partial Regex AlphaNumericRegex();
    }
}
