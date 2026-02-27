#warning TEST RACERTABLEVIEW: Make sure we can still edit racer details on this page in local

using DerbyApp.RacerDatabase;
using DerbyApp.RaceStats;
using DerbyApp.Windows;
using System.ComponentModel;
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

        public Visibility DisplayPhotos
        {
            get => _displayPhotos;
            set
            {
                _displayPhotos = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayPhotos)));
            }
        }

        public RacerTableView(Database db)
        {
            InitializeComponent();
            _db = db;
            dataGridRacerTable.DataContext = _db.Racers;
            LevelComboBox.ItemsSource = _db.GirlScoutLevels;
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
            }
        }

        private void Delete_OnClick(object sender, RoutedEventArgs e)
        {
            if (dataGridRacerTable.SelectedIndex < _db.Racers.Count)
            {
                _db.RemoveRacer(_db.Racers[dataGridRacerTable.SelectedIndex]);
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
            new ImageDisplay((sender as Image).Source, (sender as Image).DataContext as Racer).ShowDialog();
        }

        private void RefreshDatabase(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _db.RefreshDatabase();
        }
    }
}
