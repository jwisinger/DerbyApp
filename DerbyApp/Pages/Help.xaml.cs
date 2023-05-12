using System.Windows.Controls;

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
            HelpBox.Text = "1) Select an event.  There is generally one event per race day.  An event was selected at startup of the software, but ensure it is the correct event (the event title is listed in the bar at the top of the application).\r\n2) During registration for your event, Add Racers into the system as the register and check in their cars.  The photo is optional, and can either be of the person or of the car.\r\n3) You can View Racers in the system and edit or delete as necessary if any mistakes were made when entering them.\r\n4) Once all racers have been added to the event, you can create individual races by going to Select Race.  In the top drop down box, you can either select an existing race (if you've already created one), or create a new race.  A race normally has 13 or less racers in it.  It's common to group racers by level (all Daisys, Brownies, etc.)\r\n5) Once a race has been selected you can Start Race.  Once a race has started, you can control the starting gate and read data from the track if it is connected.  You can also manually enter times after each heat if you do not have a connected track.  Use the \"Previous Heat\" and \"Next Heat\" buttons to change heats and re-enter or edit any information if a heat needs to be re-run or if there was any error in transferring data from the track.\r\n6) As the event continues, you can add and select new races and then start those races the same way.\r\n7) When all races are complete, you can use View Report to general PDF reports for each racer.";
        }
    }
}
