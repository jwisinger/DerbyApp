using System.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;
using DerbyApp.RacerDatabase;
using DerbyApp.RaceStats;
using System.Windows.Controls;

namespace DerbyApp
{
    public partial class RacerTableView : Page, INotifyPropertyChanged
    {
        public ObservableCollection<Racer> Racers = new ObservableCollection<Racer>();
        private readonly Database _db;
        private bool _editHandle = true;
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

        public RacerTableView(Database db)
        {
            InitializeComponent();
            _db = db;
            _db.GetAllRacers(Racers);
            dataGridRacerTable.DataContext = Racers;
        }

        public void UpdateRacerList()
        {
            _db.GetAllRacers(Racers);
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
