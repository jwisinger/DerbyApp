using System.Data.SQLite;
using System;
using System.Drawing;
using System.IO;
using System.Data;
using System.Windows;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace DerbyApp.RacerDatabase
{
    public class Database
    {
        readonly public SQLiteConnection SqliteConn;
        private readonly string _racerTableName = "raceTable";

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

        private static byte[] ImageToByteArray(Image img)
        {
            using (var stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }

        private static Image ByteArrayToImage(byte[] byteArrayIn)
        {
            MemoryStream ms = new MemoryStream(byteArrayIn);
            Image returnImage = Image.FromStream(ms);
            return returnImage;
        }

        public Database(string databaseFile)
        {
            if (!File.Exists(databaseFile))
            {
                SQLiteConnection.CreateFile(databaseFile);
            }
            SqliteConn = new SQLiteConnection("Data Source = " + databaseFile);
            CreateConnection();
            CreateRacerTable();
        }

        private void CreateRacerTable()
        {
            string sql = "CREATE TABLE IF NOT EXISTS " + _racerTableName + " ([Number] INTEGER PRIMARY KEY AUTOINCREMENT, [Name] VARCHAR(50), [Weight(oz)] DECIMAL(5, 3), [Troop] VARCHAR(10), [Level] VARCHAR(20), [Email] VARCHAR(100), [Image] MEDIUMBLOB)";
            SQLiteCommand command = new SQLiteCommand(sql, SqliteConn);
            command.ExecuteNonQuery();
        }

#warning CODE CLEANUP: Remove the need to access the "RACE" class
        public bool CreateResultsTable(Race race)
        {
            int racerCount = 0;
            string sql;
            var regex = new Regex(@"[^a-zA-Z0-9\s]");
            if (regex.IsMatch(race.RaceName))
            {
                MessageBox.Show("Invalid character in race name.");
                return false;
            }

            sql = "SELECT name FROM sqlite_master WHERE type='table' AND name='" + race.RaceName + "'";
            SQLiteCommand command = new SQLiteCommand(sql, SqliteConn);
            if (command.ExecuteScalar() != null)
            {
                MessageBox.Show("A Race with that name already exists.");
                return false;
            }

            sql = "CREATE TABLE IF NOT EXISTS \"" + race.RaceName + "\" ([RacePosition] INTEGER PRIMARY KEY AUTOINCREMENT, [Number] INTEGER";
            for (int i = 0; i < race.HeatInfo.MaxHeats; i++) sql += ", [Heat " + (i + 1) + "] DOUBLE";
            sql += ")";
            command = new SQLiteCommand(sql, SqliteConn);
            command.ExecuteNonQuery();

            foreach (Racer r in race.Racers)
            {
                sql = "INSERT INTO \"" + race.RaceName + "\" ([Number]) VALUES (@Number)";
                command = new SQLiteCommand(sql, SqliteConn);
                command.Parameters.Add("@Number", DbType.Int64).Value = r.Number;
                racerCount += command.ExecuteNonQuery();
            }

            MessageBox.Show(racerCount + " racer(s) added to " + race.RaceName + ".");

            if (racerCount > 0) return true;
            else return false;
        }

        public ObservableCollection<Racer> GetRacerCollection(ObservableCollection<Racer> Racers = null)
        {
            SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM " + _racerTableName, SqliteConn);
            SQLiteDataAdapter sda = new SQLiteDataAdapter(cmd);
            DataSet ds = new DataSet();
            if (Racers == null) Racers = new ObservableCollection<Racer>();
            else Racers.Clear();
            sda.Fill(ds);
            if (ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dataRow in ds.Tables[0].Rows)
                {
                    try
                    {
                        Racers.Add(new Racer((Int64)dataRow[0],
                                         (string)dataRow[1],
                                         (decimal)dataRow[2],
                                         (string)dataRow[3],
                                         (string)dataRow[4],
                                         (string)dataRow[5],
                                         ByteArrayToImage((byte[])dataRow[6])));
                    }
                    catch { }
                }
            }

            return Racers;
        }

        public List<string> GetRacerNames(string raceName)
        {
            List<string> retVal = new List<string>();
            if (raceName != "")
            {
                string sql = "SELECT " + _racerTableName + ".name, " + _racerTableName + ".number FROM " + _racerTableName + " INNER JOIN " + raceName + " ON " + raceName + ".number = " + _racerTableName + ".Number ORDER BY " + raceName + ".RacePosition";
                SQLiteCommand command = new SQLiteCommand(sql, SqliteConn);
                SQLiteDataReader r = command.ExecuteReader();
                while (r.Read()) retVal.Add(Convert.ToString(r["Name"]) + "; " + Convert.ToString(r["Number"]));
            }

            return retVal;
        }

        public List<string> GetListOfRaces()
        {
            List<string> retVal = new List<string>();
            string sql = "SELECT name FROM sqlite_schema WHERE type ='table' AND name NOT LIKE 'sqlite_%';";
            SQLiteCommand command = new SQLiteCommand(sql, SqliteConn);
            SQLiteDataReader r = command.ExecuteReader();
            while (r.Read()) retVal.Add(Convert.ToString(r["name"]));
            retVal.Remove(_racerTableName);
            return retVal;
        }

        public void AddRacerToRacerTable(Racer racer)
        {
            string sql = "INSERT INTO " + _racerTableName + " ([Name], [Weight(oz)], [Troop], [Level], [Email], [Image]) VALUES (@Name, @Weight, @Troop, @Level, @Email, @Image)";
            SQLiteCommand command = new SQLiteCommand(sql, SqliteConn);
            byte[] photo = ImageToByteArray(racer.Photo);

            command.Parameters.Add("@Name", DbType.String).Value = racer.RacerName;
            command.Parameters.Add("@Weight", DbType.Decimal).Value = racer.Weight;
            command.Parameters.Add("@Troop", DbType.String).Value = racer.Troop;
            command.Parameters.Add("@Level", DbType.String).Value = racer.Level;
            command.Parameters.Add("@Email", DbType.String).Value = racer.Email;
            command.Parameters.Add("@Image", DbType.Binary).Value = photo;
            MessageBox.Show(command.ExecuteNonQuery() + " racer(s) added.");
        }

        public static void StoreDatabaseRegistry(string database, string activeRace)
        {
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\DerbyApp");
            key.SetValue("database", database);
            key.SetValue("activeRace", activeRace);
            key.Close();
        }

        public static bool GetDatabaseRegistry(out string database, out string activeRace)
        {
            database = "";
            activeRace = "";
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\DerbyApp");
            if (key != null)
            {
                if (key.GetValue("database") is string s1) database = s1;
                if (key.GetValue("activeRace") is string s2) activeRace = s2;
                return true;
            }
            return false;
        }

    }
}
