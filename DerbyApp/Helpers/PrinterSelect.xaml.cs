using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Printing;
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
    /// Interaction logic for PrinterSelect.xaml
    /// </summary>
    public partial class PrinterSelect : Window
    {
        public ObservableCollection<string> PrinterList = [];

        public PrinterSelect()
        {
            InitializeComponent();
            foreach (string s in PrinterSettings.InstalledPrinters)
            {
                PrinterList.Add(s);
            }
            qrPrinterBox.DataContext = PrinterList;
            licensePrinterBox.DataContext = PrinterList;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
