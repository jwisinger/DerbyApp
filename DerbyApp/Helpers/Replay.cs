using Emgu.CV;
using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows;

namespace DerbyApp.Helpers
{
    internal class Replay(System.Windows.Controls.Image i)
    {
        public enum VideoMethod
        {
            Viewing,
            Recording,
            None
        };

        private VideoCapture _videoCapture;
        private readonly Mat _currentFrame = new();
        private readonly System.Windows.Controls.Image _imageBox = i;
        private VideoWriter _videoWriter;
        private VideoMethod _currentState = VideoMethod.None;
        public event EventHandler ReplayEnded;
        public bool FlipImage = false;
        private double _frameRate = 0;
        private int _totalFrames = 0;
        private string _lastWrittenFile;
        private const double FRAME_RATE = 15.0;
        private int _selectedCamera = 0;
        private bool _loaded = false;

        public int SelectedCamera
        {
            get => _selectedCamera;
            set
            {
                _selectedCamera = value;
                _videoCapture?.Dispose();
                if (_loaded) Start();
            }
        }

        public void Cancel()
        {
            _videoCapture?.Dispose();
            ReplayEnded?.Invoke(this, null);
            _currentState = VideoMethod.None;
            if (_loaded) Start();
        }

        public void Stop()
        {
            _videoCapture?.Dispose();
            _currentState = VideoMethod.None;
            _loaded = false;
        }

        public void Start()
        {
            _videoCapture = new VideoCapture(SelectedCamera, VideoCapture.API.DShow);
            _videoCapture.ImageGrabbed += VideoCapture_NewFrame;
            _videoCapture.Set(Emgu.CV.CvEnum.CapProp.Fps, FRAME_RATE);
            _currentState = VideoMethod.None;
            _videoCapture.Start();
            _loaded = true;
        }

        public void ShowReplay()
        {
#warning 1: When recording is finished, need to trigger a remote write if needed
            _videoWriter?.Dispose();
            _videoCapture?.Dispose();
            _currentState = VideoMethod.Viewing;
            if (File.Exists(_lastWrittenFile))
            {
                _videoCapture = new VideoCapture(_lastWrittenFile);
                _videoCapture.ImageGrabbed += VideoCapture_NewFrame;
                _frameRate = _videoCapture.Get(Emgu.CV.CvEnum.CapProp.Fps);
                _totalFrames = (int)_videoCapture.Get(Emgu.CV.CvEnum.CapProp.FrameCount);
                _videoCapture.Start();
            }
        }

        public void StartRecording(string path, string raceName, int heatNumber)
        {
            try
            {
                _lastWrittenFile = string.Join("_", raceName.Split(Path.GetInvalidFileNameChars()));
                _lastWrittenFile = Path.Combine(path, _lastWrittenFile + "_" + heatNumber + ".mp4");
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                _videoWriter = new VideoWriter(_lastWrittenFile, -1, FRAME_RATE,
                    new System.Drawing.Size(_videoCapture.Width, _videoCapture.Height), true);
                _currentState = VideoMethod.Recording;
            }
            catch { }
        }

        private void ProcessFrame()
        {
            if (_currentState == VideoMethod.Viewing)
            {
                try
                {
                    //Wait to display correct framerate
                    Thread.Sleep((int)(3000.0 / _frameRate));

                    //Lets check to see if we have reached the end of the video
                    //If we have lets stop the capture and video as in pause button was pressed
                    //and reset the video back to start
                    if (_currentState == VideoMethod.Viewing)
                    {
                        double framenumber = _videoCapture.Get(Emgu.CV.CvEnum.CapProp.PosFrames);
                        if (framenumber == _totalFrames)
                        {
                            framenumber = 0;
                            _videoCapture.Set(Emgu.CV.CvEnum.CapProp.PosFrames, framenumber);
                            _videoCapture.Dispose();
                            ReplayEnded?.Invoke(this, null);
                            if (_loaded) Start();
                        }
                    }
                }
                catch
                {
                }
            }
            if (_currentState == VideoMethod.Recording)
            {
                if (_videoWriter.IsOpened) _videoWriter.Write(_currentFrame);
            }
        }

        private void VideoCapture_NewFrame(object sender, EventArgs e)
        {
            try
            {
                _videoCapture.Retrieve(_currentFrame);
                try
                {
                    Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        Bitmap bMap = _currentFrame.ToImage<Emgu.CV.Structure.Bgr, byte>().ToBitmap();
                        if (FlipImage) bMap.RotateFlip(RotateFlipType.Rotate180FlipNone);
                        _imageBox.Source = NewRacer.ImageSourceFromBitmap(bMap);
                    }));
                }
                catch { }
                ProcessFrame();
            }
            catch { }
        }
    }
}
