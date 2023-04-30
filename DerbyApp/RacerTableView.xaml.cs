using System.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DerbyApp.RacerDatabase;
using DerbyApp.RaceStats;
using System.Windows.Controls;

namespace DerbyApp
{
    public partial class RacerTableView : Page, INotifyPropertyChanged
    {
        private Visibility _displayPhotos = Visibility.Collapsed;
        private bool _displayPhotosChecked = false;
        public ObservableCollection<Racer> Racers = new ObservableCollection<Racer>();
        private readonly Database _db;
        private bool _editHandle = true;

        public Visibility DisplayPhotos
        {
            get => _displayPhotos;
            set
            {
                _displayPhotos = value;
                NotifyPropertyChanged();
            }
        }

        public bool DisplayPhotosChecked { get => _displayPhotosChecked; set => _displayPhotosChecked = value; }

        public event PropertyChangedEventHandler PropertyChanged; 
        
        private void NotifyPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public RacerTableView(Database db)
        {
            InitializeComponent();
            _db = db;
            _db.GetAllRacers(Racers);
            dataGridRacerTable.DataContext = Racers;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (DisplayPhotosChecked) DisplayPhotos = Visibility.Visible;
            else DisplayPhotos = Visibility.Collapsed;
        }

        private void DataGridRacerTable_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.Row.GetIndex() >= 0)
            {
                if (_editHandle)
                {
                    _editHandle = false;
                    dataGridRacerTable.CommitEdit();
                    _db.AddRacerToRacerTable(Racers[e.Row.GetIndex()]);
                    _editHandle = true;
                }
            }
            _db.GetAllRacers(Racers);
        }

        private void Delete_OnClick(object sender, RoutedEventArgs e)
        {
            _db.RemoveRacerFromRacerTable(Racers[dataGridRacerTable.SelectedIndex]);
            Racers.RemoveAt(dataGridRacerTable.SelectedIndex);
        }
    }
}
