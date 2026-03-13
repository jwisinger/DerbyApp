using System;
using System.Windows;
using System.Windows.Media;
using DerbyApp.Helpers;
using DerbyApp.RaceStats;

namespace DerbyApp.Windows
{
    /// <summary>
    /// Interaction logic for ImageDisplay.xaml
    /// </summary>
    public partial class ImageDisplay : Window
    {
        private bool _isCapturing = false;
        private bool _needSnapshot = false;
        private readonly VideoHandler _videoHandler;
        private readonly Racer _racer;

        public ImageDisplay(ImageSource imageSource, Racer racer, VideoHandler videoHandler)
        {
            InitializeComponent();
            _videoHandler = videoHandler;
            _racer = racer;
            Picture.Source = imageSource;
        }

        private void ButtonCapture_Click(object sender, RoutedEventArgs e)
        {
            if (_isCapturing)
            {
#warning Z-FUTURE: Allow changing picture (build this into the ImageDisplay?
                _needSnapshot = true;
                CloseCancelButton.Content = "Close";
                ChangePhotoButton.Content = "Change Photo";
                _isCapturing = false;
            }
            else
            {
                CloseCancelButton.Content = "Cancel";
                ChangePhotoButton.Content = "Take Photo";
                _isCapturing = true;
            }
        }

        private void ButtonCloseCancel_Click(object sender, RoutedEventArgs e)
        {
            if (_isCapturing)
            {
                CloseCancelButton.Content = "Close";
                ChangePhotoButton.Content = "Change Photo";
                _isCapturing = false;
            }
            else Close();
        }

        private void OnImageCaptured(object sender, EventArgs e)
        {
            if (_needSnapshot)
            {
                _needSnapshot = false;
                Dispatcher.Invoke(new Action(() => { Picture.Source = _videoHandler.CurrentImageSource; }));
                _racer.Photo = (System.Drawing.Image)_videoHandler.CurrentImageBitmap.Clone();
            }
            if (_isCapturing) Application.Current.Dispatcher.Invoke(new Action(() => { Picture.Source = _videoHandler.CurrentImageSource; }));
        }

        private void ImageDisplay_Unloaded(object sender, RoutedEventArgs e)
        {
            _videoHandler.ImageCaptured -= OnImageCaptured;
        }

        private void ImageDisplay_Loaded(object sender, RoutedEventArgs e)
        {
            _videoHandler.ImageCaptured += OnImageCaptured;
        }
    }
}
