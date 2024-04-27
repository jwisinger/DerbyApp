#warning FUN: Any way to improve the look of this page?
using AForge.Video;
using AForge.Video.DirectShow;
using DerbyApp.RaceStats;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace DerbyApp
{
    public partial class NewRacer : Page, INotifyPropertyChanged
    {
        public DispatcherOperation VideoThread;
        public VideoCaptureDevice LocalWebCam;
        public FilterInfoCollection LocalWebCamsCollection;
        private static bool _needSnapshot = false;
        public Racer Racer = new();
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler RacerAdded;

        public NewRacer()
        {
            InitializeComponent();
            tbName.DataContext = Racer;
            tbTroop.DataContext = Racer;
            tbWeight.DataContext = Racer;
            tbEmail.DataContext = Racer;
            cbLevel.DataContext = Racer;
        }

        public void UpdateCaptureSnapshotManifast(Bitmap image)
        {
            try
            {
                _needSnapshot = false;
                frameCapture.Source = ImageSourceFromBitmap(image);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CapturedImage"));
                Racer.Photo = (System.Drawing.Image)image.Clone();
            }
            catch { }
        }

        void Cam_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                Bitmap img = (Bitmap)eventArgs.Frame.Clone();

                MemoryStream ms = new();
                img.Save(ms, ImageFormat.Bmp);
                ms.Seek(0, SeekOrigin.Begin);
                BitmapImage bi = new();
                bi.BeginInit();
                bi.StreamSource = ms;
                bi.EndInit();
                bi.Freeze();
                VideoThread = Dispatcher.BeginInvoke(new ThreadStart(delegate
                {
                    frameVideo.Source = bi;
                }));

                if (_needSnapshot)
                {
                    Dispatcher.Invoke(new Action(() => { UpdateCaptureSnapshotManifast(img); }));
                }
            }
            catch
            {
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LocalWebCamsCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (LocalWebCamsCollection.Count > 0)
            {
                LocalWebCam = new VideoCaptureDevice(LocalWebCamsCollection[0].MonikerString);
                LocalWebCam.NewFrame += new NewFrameEventHandler(Cam_NewFrame);
                LocalWebCam.Start();
            }
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

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            VideoThread?.Abort();
            LocalWebCam?.Stop();
        }
    }
}
