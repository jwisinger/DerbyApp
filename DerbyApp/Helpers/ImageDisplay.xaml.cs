using DerbyApp.RaceStats;
using System.Windows;
using System.Windows.Media;

namespace DerbyApp.Pages
{
    /// <summary>
    /// Interaction logic for ImageDisplay.xaml
    /// </summary>
    public partial class ImageDisplay : Window
    {
        public ImageDisplay(ImageSource imageSource, Racer racer)
        {
            InitializeComponent();
            Picture.Source = imageSource;
        }
    }
}
