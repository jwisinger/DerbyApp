using System.Data;

namespace DerbyApp
{
    public class LeaderBoard
    {
        public DataTable Table = new DataTable();
        public LeaderBoard()
        {
            Table.Columns.Add("Number");
            Table.Columns.Add("Name");
            Table.Columns.Add("Score");
            Table.Columns.Add("Photo");
        }
    }
}
