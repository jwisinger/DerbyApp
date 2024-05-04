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

namespace DerbyApp
{
    public partial class NewRacer : Page
    {
        private static bool _needSnapshot = false;
        public Racer Racer = new();
        public event EventHandler RacerAdded;
        public Mat CurrentFrame {get; set;}
        private VideoCapture _videoCapture = null;

        public NewRacer()
        {
            InitializeComponent();
            tbName.DataContext = Racer;
            tbTroop.DataContext = Racer;
            tbWeight.DataContext = Racer;
            tbEmail.DataContext = Racer;
            cbLevel.DataContext = Racer;
            frameVideo.DataContext = this;
            new Thread(() =>
            {
                _videoCapture = new VideoCapture(0, VideoCapture.API.DShow);
                CurrentFrame = new Mat();
                _videoCapture.ImageGrabbed += VideoCapture_NewFrame;
                _videoCapture.Start();
            }).Start();
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
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                Bitmap bMap = CurrentFrame.ToImage<Emgu.CV.Structure.Bgr, byte>().ToBitmap();
                frameVideo.Source = ImageSourceFromBitmap(bMap);
                if (_needSnapshot)
                {
                    Dispatcher.Invoke(new Action(() => { UpdateCaptureSnapshotManifast(bMap); }));
                }
            }));
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

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteObject([In] IntPtr hObject);

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
