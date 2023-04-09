using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Drawing;
using System.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System;

namespace DerbyApp
{
    public partial class RacerTableView : Window, INotifyPropertyChanged
    {
#warning "It would be nice to figure out how to make this a bool"
        private Visibility _displayPhotos = Visibility.Collapsed;
        private bool _displayPhotosChecked = false;
        public ObservableCollection<Racer> Racers = new();
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

        public event PropertyChangedEventHandler? PropertyChanged; 
        
        private void NotifyPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public RacerTableView(Database db)
        {
            InitializeComponent();
            _db = db;
            Racers = _db.GetRacerData();
#warning "It would be nice to move this into the XAML"
            dataGrid1.ItemsSource = Racers;
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
