using System.Windows;
using System.Windows.Media;

namespace DerbyApp.Pages
{
    /// <summary>
    /// Interaction logic for ImageDisplay.xaml
    /// </summary>
    public partial class ImageDisplay : Window
    {
        public ImageDisplay(ImageSource imageSource)
        {
            InitializeComponent();
            Picture.Source = imageSource;
        }
    }
}
