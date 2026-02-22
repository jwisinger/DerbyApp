using System;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Emgu.CV;

namespace DerbyApp.Helpers
{
    public class VideoHandler (Credentials credentials)
    {
        private const string _webhookUrl = "https://api.retool.com/v1/workflows/5e126bc0-067c-45f8-a4be-cea4aec7f395/startTrigger";
        private const double FRAME_RATE = 15.0;

        private VideoCapture _videoCapture = null;
        private VideoWriter _videoWriter;
        private VideoState _videoState = VideoState.None;
        private RecordedVideoInfo _recordedVideoInfo;
        private readonly Credentials _credentials = credentials;
        private double _playbackFrameRate = 0;
        private int _playbackTotalFrames = 0;
        private int _selectedCamera = 0;

        public bool FlipImage = false;
        public ImageSource CurrentImageSource;
        public Bitmap CurrentImageBitmap;

        public event EventHandler ReplayEnded;
        public event VideoUploadedEventHandler VideoUploaded;
        private EventHandler _imageCaptured;
        public event EventHandler ImageCaptured
        {
            add
            {
                if (_imageCaptured == null) GetCamera(); // Start camera if this is the first handler being added
                _imageCaptured += value;
            }
            remove
            {
                _imageCaptured -= value;
                if (_imageCaptured == null) ReleaseCamera(); // Stop camera if there are no more handlers
            }
        }

        private enum VideoState
        {
            Viewing,
            Recording,
            None
        };

        private struct RecordedVideoInfo
        {
            public string FilePath;
            public string RaceName;
            public int HeatNumber;
        }

#warning B: Can this be a struct (and still work with JSON)?
        private class VideoLink
        {
            public string Id { get; set; }
            public string Url { get; set; }
        }

        public int SelectedCamera
        {
            get => _selectedCamera;
            set
            {
                _selectedCamera = value;
                GetCamera();
            }
        }

        private void VideoCapture_NewFrame(object sender, EventArgs e)
        {
            try
            {
                Mat currentFrame = new();
                _videoCapture.Retrieve(currentFrame);
                Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
                {
                    CurrentImageBitmap = currentFrame.ToImage<Emgu.CV.Structure.Bgr, byte>().ToBitmap();
                    if (FlipImage) CurrentImageBitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    CurrentImageSource = ImageHandler.ImageSourceFromBitmap(CurrentImageBitmap);
                    _imageCaptured?.Invoke(this, null);
                    switch (_videoState)
                    {
                        case VideoState.None:
                            break;
                        case VideoState.Recording:
                            Application.Current.Dispatcher.Invoke(new Action(() => {
                                if (_videoWriter.IsOpened) _videoWriter.Write(currentFrame);
                            }));
                            break;
                        case VideoState.Viewing:
                            Thread.Sleep((int)(3000.0 / _playbackFrameRate)); // Wait to display correct framerate (1/3rd speed)
                            if (_playbackTotalFrames == _videoCapture.Get(Emgu.CV.CvEnum.CapProp.PosFrames))
                            {
                                ReplayEnded?.Invoke(this, null);
                                if (_imageCaptured != null) GetCamera();
                            }
                            break;
                    }
                }));
            }
            catch { }
        }

        public void GetCamera()
        {
            if (_videoCapture != null) ReleaseCamera();
            _videoState = VideoState.None;
            _videoCapture = new VideoCapture(SelectedCamera, VideoCapture.API.DShow);
            _videoCapture.ImageGrabbed += VideoCapture_NewFrame;
            _videoCapture.Start();
        }

        public void ReleaseCamera()
        {
            _videoCapture?.Stop();
            _videoCapture?.Dispose();
            _videoCapture = null;
        }

        public void StartRecording(string path, string raceName, int heatNumber)
        {
            try
            {
                _recordedVideoInfo.FilePath = string.Join("_", raceName.Split(Path.GetInvalidFileNameChars()));
                _recordedVideoInfo.FilePath = Path.Combine(path, _recordedVideoInfo.FilePath + "_" + heatNumber + ".mp4");
                _recordedVideoInfo.RaceName = raceName;
                _recordedVideoInfo.HeatNumber = heatNumber;
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                _videoWriter = new VideoWriter(_recordedVideoInfo.FilePath, -1, FRAME_RATE,
                    new System.Drawing.Size(_videoCapture.Width, _videoCapture.Height), true);
                _videoState = VideoState.Recording;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError("VideoHandler.StartRecording", ex);
            }
        }

        public void StopRecording()
        {
            _videoState = VideoState.None;
            _videoWriter?.Dispose();
            if (File.Exists(_recordedVideoInfo.FilePath)) _ = UploadVideo();
        }

        public void ShowReplay()
        {
            _videoState = VideoState.Viewing;
            if (File.Exists(_recordedVideoInfo.FilePath))
            {
                if (_videoCapture != null) ReleaseCamera();
                _videoCapture = new VideoCapture(_recordedVideoInfo.FilePath);
                _videoCapture.ImageGrabbed += VideoCapture_NewFrame;
                _playbackFrameRate = _videoCapture.Get(Emgu.CV.CvEnum.CapProp.Fps);
                _playbackTotalFrames = (int)_videoCapture.Get(Emgu.CV.CvEnum.CapProp.FrameCount);
                _videoCapture.Start();
            }
        }

        public void CancelReplay()
        {
            ReplayEnded?.Invoke(this, null);
            _videoState = VideoState.None;
        }

        private async Task UploadVideo()
        {
            Byte[] bytes = File.ReadAllBytes(_recordedVideoInfo.FilePath);
            String file = Convert.ToBase64String(bytes);

            var payload = new
            {
                filename = Path.GetFileName(_recordedVideoInfo.FilePath),
                data = file
            };

            string jsonPayload = System.Text.Json.JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            using HttpClient client = new();
            client.DefaultRequestHeaders.Add("X-Workflow-Api-Key", _credentials.FileUploaderApiKey);

            try
            {
                HttpResponseMessage response = await client.PostAsync(_webhookUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    VideoUploaded?.Invoke(this, new VideoUploadedEventArgs(System.Text.Json.JsonSerializer.Deserialize<VideoLink>(responseBody).Url,
                                                                           _recordedVideoInfo.RaceName, _recordedVideoInfo.HeatNumber));
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError("VideoHandler.UploadVideo", ex);
            }
        }
    }
}
