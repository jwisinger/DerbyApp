using DerbyApp.RacerDatabase;
using DerbyApp.RaceStats;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

#warning TODO: Show race position
#warning TODO: When a racer is added, auto populate other fields
#warning TODO: Block same racer being selected twice
#warning TODO: Ensure a newly added racer is available in the drop down (any similar checks)?

namespace DerbyApp
{
    public partial class EditRace : Page, INotifyPropertyChanged
    {
        private readonly Database _db;
        private readonly Dictionary<string, CheckBox> _cbList = new Dictionary<string, CheckBox>();
        private bool _handleSelection = true;
        public List<string> Races;
        public ObservableCollection<Racer> Racers = new ObservableCollection<Racer>();
        public ObservableCollection<Racer> AllRacers = new ObservableCollection<Racer>();
        public ObservableCollection<Racer> AvailableRacers = new ObservableCollection<Racer>();
        private Visibility _displayPhotos = Visibility.Collapsed;
        public event PropertyChangedEventHandler PropertyChanged;

        public Visibility DisplayPhotos
        {
            get => _displayPhotos;
            set
            {
                _displayPhotos = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DisplayPhotos"));
            }
        }

        public EditRace(Database db)
        {
            _db = db;
            InitializeComponent();
            Races = _db.GetListOfRaces();
            cbName.DataContext = Races;
            AllRacers = _db.GetAllRacers();
            cbColumn.ItemsSource = AvailableRacers;
            dataGridRacers.DataContext = Racers;

            _cbList.Add("Daisy", cbDaisy);
            _cbList.Add("Brownie", cbBrownie);
            _cbList.Add("Junior", cbJunior);
            _cbList.Add("Cadette", cbCadette);
            _cbList.Add("Senior", cbSenior);
            _cbList.Add("Ambassador", cbAmbassador);
            _cbList.Add("Adult", cbAdult);

            AvailableRacers.Clear();
            foreach (Racer r in AllRacers) AvailableRacers.Add(r);
            /*for (int i = 0; i < _raceHeatList.RacerCount; i++)
            {
                tlpRacer.RowCount++;
                tlpRacer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                tlpRacer.Controls.Add(new Label() { Text = "Racer in Position " + (i + 1), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, i + 1);
                tlpRacer.Controls.Add(new ComboBox() { Text = "", Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList }, 1, i + 1);
            }*/

            /*tlpLevel.Controls.Add(new TextBox() { Multiline = true, ReadOnly = true, Dock = DockStyle.Fill, Text = "Choose the levels you want to include racers from and click \"Load Racers\", then choose the desired racer name for each position and click \"OK\"." });
            foreach (string s in GirlScoutLevels.ScoutLevels)
            {
                tlpLevel.RowCount++;
                tlpLevel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                tlpLevel.Controls.Add(new CheckBox() { Text = s, Dock = DockStyle.Fill, Checked = true, Name = "cb" + s }, 0, tlpLevel.RowCount - 1);
            }
            tlpLevel.RowCount++;
            tlpLevel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));*/
        }

        private void ComboBoxRaceName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                Racers = _db.GetRacers((string)e.AddedItems[0], Racers);
            }
            else
            {
                Racers.Clear();
            }
        }

        private void DataGridComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                /*int oldRacer = -1;
                int newRacer = -1;

                e.Handled = true;
                if (e.RemovedItems.Count > 0)
                {
                    oldRacer = Racers.IndexOf(Racers.Where(x => x.Number == (e.RemovedItems[0] as Racer).Number).FirstOrDefault());
                }
                newRacer = Racers.IndexOf(Racers.Where(x => x.Number == (e.AddedItems[0] as Racer).Number).FirstOrDefault());
                if (oldRacer < 0)
                {
                    Racers.Add(e.AddedItems[0] as Racer);
                }
                else if (newRacer == -1) Racers[oldRacer] = (e.AddedItems[0] as Racer);
                else
                {
                    if (_handleSelection)
                    {
                        _handleSelection = false;
                        (sender as ComboBox).SelectedItem = e.RemovedItems[0];
                        _handleSelection = true;
                    }
                }*/
            }
            catch { }

            //Racers[sender. e.Row.GetIndex()] = AllRacers.Where(racer => racer.RacerName == racerName).FirstOrDefault();
            //_db.AddRacerToRacerTable(Racers[e.Row.GetIndex()]);
        }

        private void DataGridRacers_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            /*for (int i = 0; i < Racers.Count; i++)
            {
                if (Racers[i] != null)
                {
                    Racer r = AllRacers.Where(x => x.RacerName == Racers[i].RacerName).FirstOrDefault();
                    if (r != null) Racers[i] = r;
                }
            }*/
#warning TODO: Get rid of these two hardcoded 13
            if (Racers.Count > 13) Racers.RemoveAt(Racers.Count - 1);
            _db.UpdateResultsTable(Racers, cbName.Text, 13);
        }

        private void DataGridRacers_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {
            if (Racers.Count > 13) Racers.RemoveAt(Racers.Count - 1);
        }

        private void CbName_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var regex = new Regex(@"[^a-zA-Z0-9\s]");
            cbName.Text = regex.Replace(cbName.Text, "");
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            AvailableRacers.Clear();
            foreach (var item in _cbList)
            {
                if ((bool)item.Value.IsChecked)
                {
                    var racers = AllRacers.Where(x => x.Level == item.Key);
                    foreach (Racer r in racers) AvailableRacers.Add(r);
                }
            }
        }
    }
}
