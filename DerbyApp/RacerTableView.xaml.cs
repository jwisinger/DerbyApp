using System.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

#warning FEATURE: Add ability to edit and store to database
#warning LINK: https://learn.microsoft.com/en-us/answers/questions/828767/how-to-update-sqlite-tables-using-datagrid-selecte

namespace DerbyApp
{
    public partial class RacerTableView : Window, INotifyPropertyChanged
    {
#warning CODE CLEANUP: "It would be nice to figure out how to make this a bool"
        private Visibility _displayPhotos = Visibility.Collapsed;
        private bool _displayPhotosChecked = false;
        public ObservableCollection<Racer> Racers = new ObservableCollection<Racer>();
        private readonly Database _db;

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
            Racers = _db.GetRacerData();
            dataGridRacerTable.ItemsSource = Racers;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (DisplayPhotosChecked) DisplayPhotos = Visibility.Visible;
            else DisplayPhotos = Visibility.Collapsed;
        }

        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            Racers = _db.GetRacerData();
        }
    }
}
