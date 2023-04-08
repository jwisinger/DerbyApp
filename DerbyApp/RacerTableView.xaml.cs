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
            Refreshdata(db);
            _db = db;
        }

        private static Image ByteArrayToImage(byte[] byteArrayIn)
        {
            MemoryStream ms = new(byteArrayIn);
            Image returnImage = Image.FromStream(ms);
            return returnImage;
        }

        public void Refreshdata(Database db)
        {
            SQLiteCommand cmd = new("SELECT * FROM raceTable", db.SqliteConn);
            SQLiteDataAdapter sda = new(cmd);
            DataSet ds = new();
            sda.Fill(ds);
            if (ds.Tables[0].Rows.Count > 0)
            {
                Racers.Clear();
                 dataGrid1.ItemsSource = Racers;
                foreach (DataRow dataRow in ds.Tables[0].Rows)
                {
                    try
                    {
                        Racers.Add(new Racer((Int64)dataRow[0],
                                         (string)dataRow[1],
                                         (decimal)dataRow[2],
                                         (string)dataRow[3],
                                         (string)dataRow[4],
                                         (string)dataRow[5],
                                         ByteArrayToImage((byte[])dataRow[6])));
                    }
                    catch { }
                }
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (DisplayPhotosChecked) DisplayPhotos = Visibility.Visible;
            else DisplayPhotos = Visibility.Collapsed;
        }

        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            Refreshdata(_db);
        }
    }
}
