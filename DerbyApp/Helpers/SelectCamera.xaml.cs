using DirectShowLib;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DerbyApp.Helpers
{
    /// <summary>
    /// Interaction logic for SelectCamera.xaml
    /// </summary>
    public partial class SelectCamera : Window
    {
        public ObservableCollection<string> CameraList = [];

        public SelectCamera()
        {
            InitializeComponent();
            cbCamera.DataContext = CameraList;
            GetAllConnectedCameras();
        }

        public int GetSelectedCamera()
        {
            return cbCamera.SelectedIndex;
        }

        private void GetAllConnectedCameras()
        {
            DsDevice [] capDevices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            CameraList.Clear();

            foreach (DsDevice dev in capDevices)
            {
                CameraList.Add(dev.Name);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
