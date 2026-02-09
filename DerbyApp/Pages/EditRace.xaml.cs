#warning TODO: Deleting racer from main list does not remove if already added to a race
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

namespace DerbyApp.Pages
{
    public partial class EditRace : Page, INotifyPropertyChanged
    {
        private readonly Database _db;
        private int _raceFormatIndex = 0;
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

        public Visibility DisplayPhotos
        {
            get => _displayPhotos;
            set
            {
                _displayPhotos = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayPhotos)));
            }
        }

        public Database Database => _db;

        public EditRace(Database db)
        {
            _db = db;
            InitializeComponent();
            cbName.DataContext = _db.Races;
            dataGridRacers.DataContext = _db.CurrentRaceRacers;
            cbLevels.ItemsSource = GirlScoutLevels.ScoutLevels;
            tbFormat.DataContext = this;
        }

        private void ComboBoxRaceName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_db.IsSynced)
            {
                _db.UpdateResultsTable(_db.ResultsTable, null, 0, 0);
                if (!_db.IsSynced)
                {
                    if(MessageBoxResult.Cancel == MessageBox.Show("Connection to the database has been lost. If you continue, any results from this race will be lost.",
                                    "Database Connection Lost", MessageBoxButton.OKCancel, MessageBoxImage.Warning))
                    {
                        cbName.SelectionChanged -= ComboBoxRaceName_SelectionChanged;
                        cbName.SelectedValue = _db.CurrentRaceName;
                        cbName.SelectionChanged += ComboBoxRaceName_SelectionChanged;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(cbName.SelectedValue)));
                        return;
                    }
                }
            }
            _db.CurrentRaceName = cbName.SelectedValue as string;
            try
            {
                if (_db.CurrentRaceName == null) buttonDeleteRace.IsEnabled = false;
                else if (_db.CurrentRaceName.Length > 0) buttonDeleteRace.IsEnabled = true;
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

        private void ButtonAddRacer_Click(object sender, RoutedEventArgs e)
        {
            ResponseEventArgs response = new();
            RaceChanging?.Invoke(this, response);
            if (response.Continue)
            {
                int order = 1;
                if (_db.CurrentRaceRacers.Count > RaceFormats.Formats[RaceFormatIndex].RacerCount)
                {
                    MessageBox.Show("Max number of racers already added.", "Max Racers Exceeded", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                try
                {
                    List<Racer> filteredRacers = [];
                    foreach (var item in GirlScoutLevels.ScoutLevels)
                    {
                        if (item.IsSelected)
                        {
                            var racers = _db.Racers.Where(x => x.Level == item.Level);
                            foreach (Racer r in racers)
                            {
                                if (!_db.CurrentRaceRacers.Where(x => x.Number == r.Number).Any()) filteredRacers.Add(r);
                            }
                        }
                    }

                    AddRacer addRacerWindow = new(filteredRacers, RaceFormats.Formats[RaceFormatIndex].RacerCount - _db.CurrentRaceRacers.Count);
                    addRacerWindow.ShowDialog();

                    // Check if added racer already in list
                    foreach (Racer racer in addRacerWindow.SelectedRacers)
                    {
                        IEnumerable<Racer> matches = _db.CurrentRaceRacers.Where(x => x.Number == racer.Number);
                        if (!matches.Any()) _db.CurrentRaceRacers.Add(racer);
                        else MessageBox.Show("Racer " + racer.RacerName + " is already in the list.", "Duplicate Racer", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    foreach (Racer r in _db.CurrentRaceRacers) r.RaceOrder = order++;
                    _db.ModifyResultsTable(_db.CurrentRaceRacers, cbName.Text, RaceFormats.Formats[RaceFormatIndex].HeatCount, RaceFormatIndex);
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
                    _db.CurrentRaceRacers.RemoveAt(dataGridRacers.SelectedIndex);
                    foreach (Racer r in _db.CurrentRaceRacers) r.RaceOrder = order++;
                    _db.ModifyResultsTable(_db.CurrentRaceRacers, cbName.Text, RaceFormats.Formats[RaceFormatIndex].HeatCount, 0);
                }
                catch { }
                RaceChanged?.Invoke(this, false);
            }
        }

        private void ButtonNewRace_Click(object sender, RoutedEventArgs e)
        {
            NewRace nr = new();
            if((bool)nr.ShowDialog())
            {
                if (!_db.Races.Contains(nr.RaceName))
                {
                    _db.Races.Add(nr.RaceName);
                    cbName.SelectedItem = nr.RaceName;
                    RaceFormatIndex = nr.RaceFormatIndex;
                    RaceFormatNameString = RaceFormats.Formats[nr.RaceFormatIndex].Name;
                    _db.CurrentRaceRacers.Clear();
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
                    _db.Races.Remove(cbName.SelectedItem as string);
                }
            }
        }

        private void RefreshDatabase(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _db.RefreshDatabase();
        }

        private void ZoomPicture(object sender, RoutedEventArgs e)
        {
            new ImageDisplay((sender as Image).Source, ((sender as Image).DataContext as Racer)).ShowDialog();
        }

        [GeneratedRegex(@"[^a-zA-Z0-9\s]")]
        private static partial Regex AlphaNumericRegex();
    }
}
