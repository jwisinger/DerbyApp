using System;
using System.Collections.Generic;
using System.DirectoryServices;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DerbyApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Database _db = new();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            NewRacer _nr = new();
            if (_nr.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _db.AddRow(_nr.Racer);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            new DataBaseView(_db).ShowDialog();
        }
    }
}
