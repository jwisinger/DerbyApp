using DerbyApp.Pages;
using DerbyApp.RacerDatabase;
using DerbyApp.RaceStats;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace DerbyApp
{
    public partial class RacerTableView : Page, INotifyPropertyChanged
    {
        private readonly Database _db;
        private bool _editHandle = true;
        private Visibility _displayPhotos = Visibility.Visible;
        public event PropertyChangedEventHandler PropertyChanged;
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
            dataGridRacerTable.DataContext = _db.Racers;
            OutputFolder = outputFolder;
        }

        private void DataGridRacerTable_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if ((e.Row.GetIndex() >= 0) && (e.Row.GetIndex() < _db.Racers.Count))
            {
                if (_editHandle)
                {
                    int index = e.Row.GetIndex();
                    _editHandle = false;
                    dataGridRacerTable.CommitEdit();
                    _db.AddRacerToRacerTable(_db.Racers[index]);
                    _editHandle = true;
                }
            }
        }

        private void Delete_OnClick(object sender, RoutedEventArgs e)
        {
            if (dataGridRacerTable.SelectedIndex < _db.Racers.Count)
            {
                _db.RemoveRacerFromRacerTable(_db.Racers[dataGridRacerTable.SelectedIndex]);
            }
        }

        private void PrintLicense_OnClick(object sender, RoutedEventArgs e)
        {
            if (dataGridRacerTable.SelectedIndex < _db.Racers.Count)
            {
                Racer r = _db.Racers[dataGridRacerTable.SelectedIndex];
                GenerateLicense.Generate(r, _db.GetName(), OutputFolder, QrCodeLink, LicensePrinterName, QrPrinterName);
            }
        }
        
        private void ZoomPicture(object sender, RoutedEventArgs e)
        {
            new ImageDisplay((sender as Image).Source, ((sender as Image).DataContext as Racer)).ShowDialog();
        }

        private void RefreshDatabase(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _db.RefreshDatabase();
        }
    }
}
