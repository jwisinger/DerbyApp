using DerbyApp.Pages;
using DerbyApp.RacerDatabase;
using DerbyApp.RaceStats;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace DerbyApp
{
    public partial class RacerTableView : Page, INotifyPropertyChanged
    {
        public ObservableCollection<Racer> Racers = [];
        private readonly Database _db;
        private bool _editHandle = true;
        private Visibility _displayPhotos = Visibility.Visible;
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler RacerRemoved;
        public string OutputFolder = "";
        public string LicensePrinterName = "";
        public string QrPrinterName = "";
        public string QrCodeLink = "";

        public Visibility DisplayPhotos
        {
            get => _displayPhotos;
            set
            {
                _displayPhotos = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayPhotos)));
            }
        }

        public RacerTableView(Database db, string outputFolder)
        {
            InitializeComponent();
            _db = db;
            _db.GetAllRacers(Racers);
            dataGridRacerTable.DataContext = Racers;
            OutputFolder = outputFolder;
        }

        public void UpdateRacerList()
        {
            _db.GetAllRacers(Racers);
        }

        private void DataGridRacerTable_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if ((e.Row.GetIndex() >= 0) && (e.Row.GetIndex() < Racers.Count))
            {
                if (_editHandle)
                {
                    _editHandle = false;
                    dataGridRacerTable.CommitEdit();
                    _db.AddRacerToRacerTable(Racers[e.Row.GetIndex()]);
                    _editHandle = true;
                    _db.GetAllRacers(Racers);
                }
            }
        }

        private void Delete_OnClick(object sender, RoutedEventArgs e)
        {
            if (dataGridRacerTable.SelectedIndex < Racers.Count)
            {
                Racer r = Racers[dataGridRacerTable.SelectedIndex];
                _db.RemoveRacerFromRacerTable(Racers[dataGridRacerTable.SelectedIndex]);
                Racers.RemoveAt(dataGridRacerTable.SelectedIndex);
                RacerRemoved?.Invoke(this, new RacerEventArgs() { racer = r });
            }
        }

        private void PrintLicense_OnClick(object sender, RoutedEventArgs e)
        {
            if (dataGridRacerTable.SelectedIndex < Racers.Count)
            {
                Racer r = Racers[dataGridRacerTable.SelectedIndex];
                GenerateLicense.Generate(r, _db.GetName(), OutputFolder, QrCodeLink, LicensePrinterName, QrPrinterName);
            }
        }
        
        private void ZoomPicture(object sender, RoutedEventArgs e)
        {
            new ImageDisplay((sender as Image).Source).ShowDialog();
        }
    }
}
