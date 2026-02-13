#warning CLEANUP: Put breakpoints in every function in this file and confirm they work
#warning TODO: Deleting racer from main list does not remove if already added to a race
using DerbyApp.RacerDatabase;
using DerbyApp.RaceStats;
using DerbyApp.Windows;
using System.Collections.Generic;
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

        #region Public Properties
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
        #endregion

        public EditRace(Database db)
        {
            _db = db;
            InitializeComponent();
            cbName.DataContext = _db.Races;
            dataGridRacers.DataContext = _db.CurrentRaceRacers;
            cbLevels.ItemsSource = GirlScoutLevels.ScoutLevels;
            tbFormat.DataContext = this;
        }

        #region UI Event Handlers
        private void CbName_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var regex = AlphaNumericRegex();
            cbName.Text = regex.Replace(cbName.Text, "");
            if (cbName.Text.Length > 0) buttonDeleteRace.IsEnabled = true;
            else buttonDeleteRace.IsEnabled = false;
        }

#warning CLEANUP: EditRace
        private void ComboBoxRaceName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_db.IsSynced)
            {
                _db.UpdateResultsTable(null, 0, 0);
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
            if (_db.CurrentHeatNumber > 1) buttonAddRacer.IsEnabled = false;
            else buttonAddRacer.IsEnabled = true;
        }
        #endregion

        #region Button Handlers
#warning CLEANUP: EditRace
        private void ButtonAddRacer_Click(object sender, RoutedEventArgs e)
        {
            if (CheckRaceInProgress())
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
                _db.RaceInProgress = false;
                if (_db.CurrentHeatNumber > 1) buttonAddRacer.IsEnabled = false;
                else buttonAddRacer.IsEnabled = true;
            }
        }

        private void Delete_OnClick(object sender, RoutedEventArgs e)
        {
            if (CheckRaceInProgress())
            {
                int order = 1;
                try
                {
#warning CLEANUP: Handle this inside Database.cs
                    _db.CurrentRaceRacers.RemoveAt(dataGridRacers.SelectedIndex);
                    foreach (Racer r in _db.CurrentRaceRacers) r.RaceOrder = order++;
                    _db.ModifyResultsTable(_db.CurrentRaceRacers, cbName.Text, RaceFormats.Formats[RaceFormatIndex].HeatCount, 0);
                }
                catch { }
                _db.RaceInProgress = false;
                if (_db.CurrentHeatNumber > 1) buttonAddRacer.IsEnabled = false;
                else buttonAddRacer.IsEnabled = true;
            }
        }

        private void ButtonNewRace_Click(object sender, RoutedEventArgs e)
        {
            NewRace nr = new();
            if((bool)nr.ShowDialog())
            {
                if(!_db.AddRace(nr.RaceName, nr.RaceFormatIndex))
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
                    _db.DeleteCurrentRace();
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
        #endregion

        #region Methods
        private bool CheckRaceInProgress()
        {
            if (_db.RaceInProgress)
            {
                if (MessageBoxResult.OK == MessageBox.Show(
                    "Adding or removing a racer will reset the race in progress and erase all results.",
                    "Race Results Will Be Erased", MessageBoxButton.OKCancel, MessageBoxImage.Warning))
                {
                    return true;
                }
                else return false;
            }
            else return true;
        }

        [GeneratedRegex(@"[^a-zA-Z0-9\s]")]
        private static partial Regex AlphaNumericRegex();
        #endregion
    }
}
