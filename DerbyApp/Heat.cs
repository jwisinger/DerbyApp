using System.Data;

namespace DerbyApp
{
    public class Heat
    {
        public DataTable Table = new DataTable();
        public Heat()
        {
            Table.Columns.Add("Number");
            Table.Columns.Add("Name");
            Table.Columns.Add("Score");
            Table.Columns.Add("Photo");
        }
    }
}
