using System.Collections.ObjectModel;
using System.Drawing.Printing;
using System.Windows;

namespace DerbyApp.Windows
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
