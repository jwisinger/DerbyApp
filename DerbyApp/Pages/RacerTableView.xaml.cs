using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using DerbyApp.Helpers;
using DerbyApp.RacerDatabase;
using DerbyApp.RaceStats;
using DerbyApp.Windows;

namespace DerbyApp
{
    public partial class RacerTableView : Page, INotifyPropertyChanged
    {
        private readonly Database _db;
        private readonly VideoHandler _videoHandler;
        private bool _editHandle = true;
        private Visibility _displayPhotos = Visibility.Visible;
        public event PropertyChangedEventHandler PropertyChanged;

        public Visibility DisplayPhotos
        {
            get => _displayPhotos;
            set
            {
                _displayPhotos = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayPhotos)));
            }
        }

        public RacerTableView(Database db, VideoHandler videoHandler)
        {
            InitializeComponent();
            _db = db;
            dataGridRacerTable.DataContext = _db.Racers;
            LevelComboBox.ItemsSource = _db.GirlScoutLevels;
            _videoHandler = videoHandler;
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
                    _db.AddRacer(_db.Racers[index]);
                    _editHandle = true;
                }
                ErrorLogger.LogEvent($"Cell Edited: [RacerTableView] {_db.Racers[e.Row.GetIndex()].RacerName}");
            }
        }

        private void Delete_OnClick(object sender, RoutedEventArgs e)
        {
            if (dataGridRacerTable.SelectedIndex < _db.Racers.Count)
            {
                _db.RemoveRacer(_db.Racers[dataGridRacerTable.SelectedIndex]);
                ErrorLogger.LogEvent($"Row Deleted: [RacerTableView] {_db.Racers[dataGridRacerTable.SelectedIndex].RacerName}");
            }
        }

        private void PrintLicense_OnClick(object sender, RoutedEventArgs e)
        {
            if (dataGridRacerTable.SelectedIndex < _db.Racers.Count)
            {
                Racer r = _db.Racers[dataGridRacerTable.SelectedIndex];
                GenerateLicense.Generate(r, _db);
            }
        }

        private void ZoomPicture(object sender, RoutedEventArgs e)
        {
            new ImageDisplay((sender as Image).Source, (sender as Image).DataContext as Racer, _videoHandler, _db).ShowDialog();
        }

        private void RefreshDatabase(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _db.RefreshDatabase();
        }
    }
}
