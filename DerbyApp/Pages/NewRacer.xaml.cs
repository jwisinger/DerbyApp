#warning TEST NEWRACER: Check scale
#warning TEST NEWRACER: Check license printing
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using DerbyApp.Helpers;
using DerbyApp.RacerDatabase;
using DerbyApp.RaceStats;

namespace DerbyApp
{
    public partial class NewRacer : Page
    {
        private readonly Database _db;
        private readonly VideoHandler _videoHandler;
        private readonly USBScale _scale = null;
        private readonly DispatcherTimer _scaleTimer;
        private static bool _needSnapshot = false;

        public Racer Racer = new();
        public event EventHandler RacerAdded;

        public NewRacer(Database db, VideoHandler vh)
        {
            InitializeComponent();
            _db = db;
            _videoHandler = vh;
            _scale = new USBScale();
            _scaleTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _scaleTimer.Tick += ReadScale;

            cbLevel.ItemsSource = _db.GirlScoutLevels;
            tbName.DataContext = Racer;
            tbTroop.DataContext = Racer;
            tbWeight.DataContext = Racer;
            tbEmail.DataContext = Racer;
            cbLevel.DataContext = Racer;
            frameVideo.DataContext = this;
        }

        private void ReadScale(object sender, EventArgs e)
        {
            if (!_scale.IsConnected) _scale.Connect();
            if (_scale.IsConnected)
            {
                Racer.Weight = _scale.GetWeight();
                _scale.Disconnect();
            }
        }

        public void ClearRacer()
        {
            Racer.RacerName = "";
            Racer.Troop = "";
            Racer.Weight = 0;
            Racer.Email = "";
            Racer.Number = 0;
        }

        private void ButtonCamera_Click(object sender, RoutedEventArgs e)
        {
            _needSnapshot = true;
        }

        private void ButtonAddRacer_Click(object sender, EventArgs e)
        {
            if (Racer.RacerName == "")
            {
                MessageBox.Show("Name cannot be left blank.", "Invalid Entry", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (Racer.Level == "")
            {
                MessageBox.Show("Level cannot be left blank.", "Invalid Entry", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            RacerAdded?.Invoke(this, new EventArgs());
        }

        private void ButtonPrintLicense_Click(object sender, EventArgs e)
        {
            if (Racer.RacerName == "")
            {
                MessageBox.Show("Name cannot be left blank.", "Invalid Entry", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (Racer.Level == "")
            {
                MessageBox.Show("Level cannot be left blank.", "Invalid Entry", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            GenerateLicense.Generate(Racer, _db);
        }

        private void OnImageCaptured(object sender, EventArgs e)
        {
            if (_needSnapshot)
            {
                _needSnapshot = false;
                Dispatcher.Invoke(new Action(() => { frameCapture.Source = _videoHandler.CurrentImageSource; }));
                Racer.Photo = (System.Drawing.Image)_videoHandler.CurrentImageBitmap.Clone();
            }
            Application.Current.Dispatcher.Invoke(new Action(() => { frameVideo.Source = _videoHandler.CurrentImageSource; }));
        }

        private void NewRacerPage_Unloaded(object sender, RoutedEventArgs e)
        {
            _scaleTimer.Stop();
            _videoHandler.ImageCaptured -= OnImageCaptured;
        }

        private void NewRacerPage_Loaded(object sender, RoutedEventArgs e)
        {
            _scaleTimer.Start();
            _videoHandler.ImageCaptured += OnImageCaptured;
        }
    }
}
