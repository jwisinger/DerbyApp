using DerbyApp.RaceStats;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Emgu.CV;
using System.Threading;
using DerbyApp.Helpers;

namespace DerbyApp
{
    public partial class NewRacer : Page
    {
        private static bool _needSnapshot = false;
        public Racer Racer = new();
        public event EventHandler RacerAdded;
        public Mat CurrentFrame {get; set;}
        public bool FlipImage = false;
        private VideoCapture _videoCapture = null;
        private int _selectedCamera = 0;
        private readonly USBScale _scale = null;
        public string OutputFolderName = null;
        public string EventFile = null;
        private readonly DispatcherTimer _scaleTimer;

        public int SelectedCamera
        {
            get => _selectedCamera;
            set
            {
                _selectedCamera = value;
                GetCamera();
            }
        }

        public NewRacer(string outputFolderName, string eventFile)
        {
            InitializeComponent();
            tbName.DataContext = Racer;
            tbTroop.DataContext = Racer;
            tbWeight.DataContext = Racer;
            tbEmail.DataContext = Racer;
            cbLevel.DataContext = Racer;
            _scale = new USBScale();
            OutputFolderName = outputFolderName;
            EventFile = eventFile;
            frameVideo.DataContext = this;
            GetCamera();
            _scaleTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _scaleTimer.Tick += ReadScale;
            _scaleTimer.Start();
        }

        private void ReadScale(object sender, EventArgs e)
        {
            if (!_scale.IsConnected)
            {
                _scale.Connect();
            }
            Racer.Weight = _scale.GetWeight();
            _scale.Disconnect();
        }

        public void GetCamera()
        {
            _videoCapture = new VideoCapture(SelectedCamera, VideoCapture.API.DShow);
            CurrentFrame = new Mat();
            _videoCapture.ImageGrabbed += VideoCapture_NewFrame;
            _videoCapture.Start();
        }

        public void ReleaseCamera()
        {
            _videoCapture.Stop();
            _videoCapture.Dispose();
        }

        public void UpdateCaptureSnapshotManifast(Bitmap image)
        {
            try
            {
                _needSnapshot = false;
                frameCapture.Source = ImageSourceFromBitmap(image);
                Racer.Photo = (System.Drawing.Image)image.Clone();
            }
            catch { }
        }

        private void VideoCapture_NewFrame(object sender, EventArgs e)
        {
            _videoCapture.Retrieve(CurrentFrame);
            try
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    Bitmap bMap = CurrentFrame.ToImage<Emgu.CV.Structure.Bgr, byte>().ToBitmap();
                    if (FlipImage) bMap.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    frameVideo.Source = ImageSourceFromBitmap(bMap);
                    if (_needSnapshot)
                    {
                        Dispatcher.Invoke(new Action(() => { UpdateCaptureSnapshotManifast(bMap); }));
                    }
                }));
            }
            catch { }
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
            GenerateLicense.Generate(Racer, EventFile, OutputFolderName);
        }
        

        [LibraryImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool DeleteObject(IntPtr hObject);

        public static ImageSource ImageSourceFromBitmap(Bitmap bmp)
        {
            var handle = bmp.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally { DeleteObject(handle); }
        }

        public void ClearRacer()
        {
            Racer.RacerName = "";
            Racer.Troop = "";
            Racer.Weight = 0;
            Racer.Email = "";
            Racer.Number = 0;
        }
    }
}
