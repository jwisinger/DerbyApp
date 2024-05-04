using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace DerbyApp.Pages
{
    /// <summary>
    /// Interaction logic for Help.xaml
    /// </summary>
    public partial class Help : Page
    {
        public Help()
        {
            InitializeComponent();
            HelpBox.Inlines.Add(new Run("\r\nDefinitions\r\n") { FontWeight = FontWeights.Bold, FontSize = 18 });
            HelpBox.Inlines.Add(new Run("Heat: ") { FontWeight = FontWeights.Bold, FontSize = 14 });
            HelpBox.Inlines.Add(new Run("A \"heat\" is one individual run of cars down the track.\r\n") { FontSize = 14 });
            HelpBox.Inlines.Add(new Run("Race: ") { FontWeight = FontWeights.Bold, FontSize = 14 });
            HelpBox.Inlines.Add(new Run("A \"race\" is a collection of heats run with the same set of cars to determine a winner.\r\n") { FontSize = 14 });
            HelpBox.Inlines.Add(new Run("Event: ") { FontWeight = FontWeights.Bold, FontSize = 14 });
            HelpBox.Inlines.Add(new Run("An \"event\" is a collection of races run on the same day.  An \"event\" contains independent races, or tiered races such that winners of one race face off against winners of other races.\r\n") { FontSize = 14 });
            HelpBox.Inlines.Add(new Run("\r\nSteps\r\n") { FontWeight = FontWeights.Bold, FontSize = 18 });
            HelpBox.Inlines.Add(new Run("1) ") { FontWeight = FontWeights.Bold, FontSize = 14 });
            HelpBox.Inlines.Add(new Run("Select an event.  There is generally one event per race day.  An event was selected at startup of the software, but ensure it is the correct event (the event title is listed in the bar at the top of the application).\r\n") { FontSize = 14 });
            HelpBox.Inlines.Add(new Run("2) ") { FontWeight = FontWeights.Bold, FontSize = 14 });
            HelpBox.Inlines.Add(new Run("During registration for your event, Add Racers into the system as they register and check in their cars.  The photo is optional, and can either be of the person or of the car.\r\n") { FontSize = 14 });
            HelpBox.Inlines.Add(new Run("3) ") { FontWeight = FontWeights.Bold, FontSize = 14 });
            HelpBox.Inlines.Add(new Run("You can View Racers in the system and edit or delete as necessary if any mistakes were made when entering them.\r\n") { FontSize = 14 });
            HelpBox.Inlines.Add(new Run("4) ") { FontWeight = FontWeights.Bold, FontSize = 14 });
            HelpBox.Inlines.Add(new Run("Once all racers have been added to the event, you can create individual races by going to Select Race.  In the top drop down box, you can either select an existing race (if you've already created one), or create a new race.  A race normally has 13 or less racers in it.  It's common to group racers by level (all Daisys, Brownies, etc.)\r\n") { FontSize = 14 });
            HelpBox.Inlines.Add(new Run("5) ") { FontWeight = FontWeights.Bold, FontSize = 14 });
            HelpBox.Inlines.Add(new Run("Once a race has been selected you can Start Race.  Once a race has started, you can control the starting gate and read data from the track if it is connected.  You can also manually enter times after each heat if you do not have a connected track.  Use the \"Previous Heat\" and \"Next Heat\" buttons to change heats and re-enter or edit any information if a heat needs to be re-run or if there was any error in transferring data from the track.\r\n") { FontSize = 14 });
            HelpBox.Inlines.Add(new Run("6) ") { FontWeight = FontWeights.Bold, FontSize = 14 });
            HelpBox.Inlines.Add(new Run("As the event continues, you can add and select new races and then start those races the same way.\r\n") { FontSize = 14 });
            HelpBox.Inlines.Add(new Run("7) ") { FontWeight = FontWeights.Bold, FontSize = 14 });
            HelpBox.Inlines.Add(new Run("When all races are complete, you can use View Report to general PDF reports for each racer.\r\n") { FontSize = 14 });
        }
    }
}
