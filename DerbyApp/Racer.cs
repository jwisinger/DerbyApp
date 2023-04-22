using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Drawing.Imaging;
using System;
using System.ComponentModel;

namespace DerbyApp
{
    public class Racer : INotifyPropertyChanged
    {
        private Int64 _number = 0;
        private string _name = "None";
        private decimal _weight = 0;
        private string _troop = "";
        private string _level = "";
        private string _email = "";
        private int _score = 0;
        private ImageSource _photosource = GetImageSource(new Bitmap(640, 480));
        private Image _photo = new Bitmap(640, 480);

        public event PropertyChangedEventHandler PropertyChanged;

        public Int64 Number { get => _number; set => _number = value; }
        public string RacerName { get => _name; set => _name = value; }
        public decimal Weight { get => _weight; set => _weight = value; }
        public string Troop { get => _troop; set => _troop = value; }
        public string Level { get => _level; set => _level = value; }
        public string Email { get => _email; set => _email = value; }
        public int Score
        {
            get
            {
                return _score;
            }
            set
            {
                _score = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Score"));
            }
        }
        public int RaceOrder = 0;

        public ImageSource PhotoSource { get => _photosource; set => _photosource = value; }
        public Image Photo { get => _photo; set { _photo = value; PhotoSource = GetImageSource(_photo); } }

        private static ImageSource GetImageSource(Image photo)
        {
            using (var ms = new MemoryStream())
            {
                var bitmapImage = new BitmapImage();
                photo.Save(ms, ImageFormat.Bmp);
                ms.Seek(0, SeekOrigin.Begin);
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = ms;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }

        public Racer()
        {
        }

        public Racer(Int64 number, string racerName, decimal weight, string troop, string level, string email, Image photo)
        {
            Number = number;
            RacerName = racerName;
            Weight = weight;
            Troop = troop;
            Level = level;
            Email = email;
            Photo = photo;
        }
    }
}
