using System.Data.SQLite;
using System;
using System.Drawing;
using System.IO;
using System.Data;
using System.Windows;

namespace DerbyApp
{
    public class Database
    {
        readonly public SQLiteConnection SqliteConn = new("Data Source = DerbyDatabase.sqlite");

        private SQLiteConnection CreateConnection()
        {
            try
            {
                SqliteConn.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return SqliteConn;
        }

        private void CreateTable()
        {
            string sql = "CREATE TABLE IF NOT EXISTS raceTable([Number] INTEGER PRIMARY KEY AUTOINCREMENT, [Name] VARCHAR(50), [Weight(oz)] DECIMAL(5, 3), [Troop] VARCHAR(10), [Level] VARCHAR(20), [Email] VARCHAR(100), [Image] MEDIUMBLOB)";
            SQLiteCommand command = new(sql, SqliteConn);
            command.ExecuteNonQuery();
        }

        public Database()
        {
            if (!File.Exists("DerbyDatabase.sqlite"))
            {
                SQLiteConnection.CreateFile("DerbyDatabase.sqlite");
            }
            CreateConnection();
            CreateTable();
        }

        private static byte[] GetImageAsByteArray(Image img)
        {
            using var stream = new MemoryStream();
            img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            return stream.ToArray();
        }

        public void AddRow(Racer racer)
        {
            string sql = "INSERT INTO raceTable ([Name], [Weight(oz)], [Troop], [Level], [Email], [Image]) VALUES (@Name, @Weight, @Troop, @Level, @Email, @Image)";
            SQLiteCommand command = new(sql, SqliteConn);
            byte[] photo = GetImageAsByteArray(racer.Photo);

            command.Parameters.Add("@Name", DbType.String).Value = racer.RacerName;
            command.Parameters.Add("@Weight", DbType.Decimal).Value = racer.Weight;
            command.Parameters.Add("@Troop", DbType.String).Value = racer.Troop;
            command.Parameters.Add("@Level", DbType.String).Value = racer.Level;
            command.Parameters.Add("@Email", DbType.String).Value = racer.Email;
            command.Parameters.Add("@Image", DbType.Binary).Value = photo;
            MessageBox.Show(command.ExecuteNonQuery() + " racer(s) added.");
        }

    }
}
