using System;

namespace DerbyApp.Helpers
{
    public delegate void VideoUploadedEventHandler(object sender, VideoUploadedEventArgs e);

    public class VideoUploadedEventArgs(string url, string raceName, int heatNumber) : EventArgs
    {
        public string Url { get; set; } = url;
        public string RaceName { get; set; } = raceName;
        public int HeatNumber { get; set; } = heatNumber;
    }
}
