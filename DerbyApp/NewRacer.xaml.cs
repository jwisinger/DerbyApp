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

#warning TODO: Add a button to add the racer and make it do something

namespace DerbyApp
{
    public partial class NewRacer : Page, INotifyPropertyChanged
    {
        public DispatcherOperation VideoThread;
        public VideoCaptureDevice LocalWebCam;
        public FilterInfoCollection LocalWebCamsCollection;
        private static bool _needSnapshot = false;
        public Racer Racer = new Racer();
        public event PropertyChangedEventHandler PropertyChanged;

        public NewRacer()
        {
            InitializeComponent();
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

                MemoryStream ms = new MemoryStream();
                img.Save(ms, ImageFormat.Bmp);
                ms.Seek(0, SeekOrigin.Begin);
                BitmapImage bi = new BitmapImage();
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
            LocalWebCam = new VideoCaptureDevice(LocalWebCamsCollection[0].MonikerString);
            LocalWebCam.NewFrame += new NewFrameEventHandler(Cam_NewFrame);

            LocalWebCam.Start();
        }

        private void ButtonCamera_Click(object sender, RoutedEventArgs e)
        {
            _needSnapshot = true;
        }

        private void ButtonOk_Click(object sender, EventArgs e)
        {
            //Racer.RacerName = tbName.Text;
            //Racer.Troop = tbTroop.Text;
            //Racer.Level = cbLevel.Text;
            //Racer.Weight = nuWeight.Value;
            //Racer.Email = tbEmail.Text;
            /*foreach (string s in GirlScoutLevels.ScoutLevels)
            {
                cbLevel.Items.Add(s);
            }
            errorProvider1.SetError(tbName, "Name should not be left blank!");
            errorProvider1.SetError(cbLevel, "Level should not be left blank!");*/
        }

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        public ImageSource ImageSourceFromBitmap(Bitmap bmp)
        {
            var handle = bmp.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally { DeleteObject(handle); }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            VideoThread?.Abort();
            LocalWebCam?.Stop();
        }
    }
}
