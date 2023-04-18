using System.Collections.Generic;
using System.Data;

namespace DerbyApp
{
#warning FEATURE: Instead of using a Table here, I should use an ObservableCollection like RacerTableView
    public class LeaderBoard
    {
        public DataTable Table = new DataTable();
        public LeaderBoard() : this(new List<Racer>()) { }

        public LeaderBoard(List<Racer> racers)
        {
            Table.Columns.Add("Number");
            Table.Columns.Add("Name");
            Table.Columns.Add("Score");
            Table.Columns.Add("Photo");
            Table.PrimaryKey = new DataColumn[] { Table.Columns["Number"] };

            foreach (Racer r in racers)
            {
                Table.Rows.Add(r.Number, r.RacerName, 0, r.Photo);
            }
        }
    }
}
