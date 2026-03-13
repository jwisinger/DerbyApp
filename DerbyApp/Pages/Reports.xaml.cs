using System.Windows;
using System.Windows.Controls;
using DerbyApp.RacerDatabase;

namespace DerbyApp.Pages
{
    /// <summary>
    /// Interaction logic for Reports.xaml
    /// </summary>
    public partial class Reports : Page
    {
        private readonly Database _db;

        public Reports(Database db)
        {
            InitializeComponent();
            _db = db;
            cbRace1.DataContext = _db.Races;
            cbRace2.DataContext = _db.Races;
            cbRace3.DataContext = _db.Races;

            if (_db.Races.Count > 0) cbRace1.SelectedItem = _db.Races[0];
            if (_db.Races.Count > 1) cbRace2.SelectedItem = _db.Races[1];
            if (_db.Races.Count > 2) cbRace3.SelectedItem = _db.Races[2];
        }

        private void ButtonReport_Click(object sender, RoutedEventArgs e)
        {
            GenerateReport.Generate(_db);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_db.Races.Count == 0) return;
            _db.CurrentRaceName = (sender as ComboBox).SelectedItem as string;
            if (_db.InitGood)
            {
                if (_db.LdrBoard.Board.Count > 0)
                {
                    switch ((sender as ComboBox).Name)
                    {
                        case "cbRace1":
                            if (_db.LdrBoard.Board.Count > 0) racer11Image.DataContext = _db.LdrBoard.Board[0]; else racer11Image.DataContext = null;
                            if (_db.LdrBoard.Board.Count > 0) racer11Name.DataContext = _db.LdrBoard.Board[0]; else racer11Name.DataContext = null;
                            if (_db.LdrBoard.Board.Count > 1) racer12Image.DataContext = _db.LdrBoard.Board[1]; else racer12Image.DataContext = null;
                            if (_db.LdrBoard.Board.Count > 1) racer12Name.DataContext = _db.LdrBoard.Board[1]; else racer12Name.DataContext = null;
                            if (_db.LdrBoard.Board.Count > 2) racer13Image.DataContext = _db.LdrBoard.Board[2]; else racer13Image.DataContext = null;
                            if (_db.LdrBoard.Board.Count > 2) racer13Name.DataContext = _db.LdrBoard.Board[2]; else racer13Name.DataContext = null;
                            racer11Name.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
                            racer11Image.GetBindingExpression(Image.SourceProperty).UpdateTarget();
                            racer12Name.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
                            racer12Image.GetBindingExpression(Image.SourceProperty).UpdateTarget();
                            racer13Name.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
                            racer13Image.GetBindingExpression(Image.SourceProperty).UpdateTarget();
                            break;
                        case "cbRace2":
                            if (_db.LdrBoard.Board.Count > 0) racer21Image.DataContext = _db.LdrBoard.Board[0]; else racer21Image.DataContext = null;
                            if (_db.LdrBoard.Board.Count > 0) racer21Name.DataContext = _db.LdrBoard.Board[0]; else racer21Name.DataContext = null;
                            if (_db.LdrBoard.Board.Count > 1) racer22Image.DataContext = _db.LdrBoard.Board[1]; else racer22Image.DataContext = null;
                            if (_db.LdrBoard.Board.Count > 1) racer22Name.DataContext = _db.LdrBoard.Board[1]; else racer22Name.DataContext = null;
                            if (_db.LdrBoard.Board.Count > 2) racer23Image.DataContext = _db.LdrBoard.Board[2]; else racer23Image.DataContext = null;
                            if (_db.LdrBoard.Board.Count > 2) racer23Name.DataContext = _db.LdrBoard.Board[2]; else racer23Name.DataContext = null;
                            racer21Name.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
                            racer21Image.GetBindingExpression(Image.SourceProperty).UpdateTarget();
                            racer22Name.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
                            racer22Image.GetBindingExpression(Image.SourceProperty).UpdateTarget();
                            racer23Name.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
                            racer23Image.GetBindingExpression(Image.SourceProperty).UpdateTarget();
                            break;
                        case "cbRace3":
                            if (_db.LdrBoard.Board.Count > 0) racer31Image.DataContext = _db.LdrBoard.Board[0]; else racer31Image.DataContext = null;
                            if (_db.LdrBoard.Board.Count > 0) racer31Name.DataContext = _db.LdrBoard.Board[0]; else racer31Name.DataContext = null;
                            if (_db.LdrBoard.Board.Count > 1) racer32Image.DataContext = _db.LdrBoard.Board[1]; else racer32Image.DataContext = null;
                            if (_db.LdrBoard.Board.Count > 1) racer32Name.DataContext = _db.LdrBoard.Board[1]; else racer32Name.DataContext = null;
                            if (_db.LdrBoard.Board.Count > 2) racer33Image.DataContext = _db.LdrBoard.Board[2]; else racer33Image.DataContext = null;
                            if (_db.LdrBoard.Board.Count > 2) racer33Name.DataContext = _db.LdrBoard.Board[2]; else racer33Name.DataContext = null;
                            racer31Name.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
                            racer31Image.GetBindingExpression(Image.SourceProperty).UpdateTarget();
                            racer32Name.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
                            racer32Image.GetBindingExpression(Image.SourceProperty).UpdateTarget();
                            racer33Name.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
                            racer33Image.GetBindingExpression(Image.SourceProperty).UpdateTarget();
                            break;
                    }
                }
            }
        }

        private void SlideShow_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}
