﻿using DerbyApp.RacerDatabase;
using DerbyApp.RaceStats;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace DerbyApp
{
    public partial class EditRace : Page, INotifyPropertyChanged
    {
        private readonly Database _db;
        private readonly Dictionary<string, CheckBox> _cbList = new Dictionary<string, CheckBox>();
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
            cbRacers.ItemsSource = AvailableRacers;
            dataGridRacers.DataContext = Racers;

            _cbList.Add("Daisy", cbDaisy);
            _cbList.Add("Brownie", cbBrownie);
            _cbList.Add("Junior", cbJunior);
            _cbList.Add("Cadette", cbCadette);
            _cbList.Add("Senior", cbSenior);
            _cbList.Add("Ambassador", cbAmbassador);
            _cbList.Add("Adult", cbAdult);

            UpdateRacerList();
        }

        public void UpdateRacerList()
        {
            AllRacers = _db.GetAllRacers();
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

        private void ComboBoxRaceName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                int order = 1;
                Racers = _db.GetRacers((string)e.AddedItems[0], Racers);
                foreach (Racer r in Racers) r.RaceOrder = order++;
            }
            else
            {
                Racers.Clear();
            }
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

        private void ButtonAddRacer_Click(object sender, RoutedEventArgs e)
        {
            int order = 1;
#warning HARDCODE: Get rid of this hardcoded 13
            if (Racers.Count > 13) return;

            // Check if added racer already in list
            if (Racers.Where(x => x.Number == (cbRacers.SelectedItem as Racer).Number).FirstOrDefault() == null)
            {
                Racers.Add(cbRacers.SelectedItem as Racer);
            }

            foreach (Racer r in Racers) r.RaceOrder = order++;
#warning HARDCODE: Get rid of this hardcoded 13
            _db.ModifyResultsTable(Racers, cbName.Text, 13);
        }

        private void Delete_OnClick(object sender, RoutedEventArgs e)
        {
            int order = 1;
            Racers.RemoveAt(dataGridRacers.SelectedIndex);
            foreach (Racer r in Racers) r.RaceOrder = order++;
#warning HARDCODE: Get rid of this hardcoded 13
            _db.ModifyResultsTable(Racers, cbName.Text, 13);
        }
    }
}
