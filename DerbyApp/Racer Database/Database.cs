using System.Data.SQLite;
using System;
using System.Drawing;
using System.IO;
using System.Data;
using System.Windows;
using System.Collections.ObjectModel;
using Microsoft.Win32;

namespace DerbyApp
{
    public class Database
    {
        readonly public SQLiteConnection SqliteConn;

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
            string sql = "CREATE TABLE IF NOT EXISTS raceTable([Number] INTEGER PRIMARY KEY AUTOINCREMENT, [Name] VARCHAR(50), [Weight(oz)] DECIMAL(5, 3), [Troop] VARCHAR(10), [Level] VARCHAR(20), [Email] VARCHAR(100), [Image] MEDIUMBLOB)";
            SQLiteCommand command = new SQLiteCommand(sql, SqliteConn);
            command.ExecuteNonQuery();
        }

        public void AddRacer(Racer racer)
        {
            string sql = "INSERT INTO raceTable ([Name], [Weight(oz)], [Troop], [Level], [Email], [Image]) VALUES (@Name, @Weight, @Troop, @Level, @Email, @Image)";
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

        public bool CreateRaceTable(Race race)
        {
#warning VULNERABILITY: This check doesn't work if there are quotes in the raceName
#warning VULNERABILITY: I have not tested what happens with single quotes in the raceName
            string name = race.RaceName.Replace("\"", "\"\"");
            string sql = "SELECT name FROM sqlite_master WHERE type='table' AND name='" + name + "'";
            SQLiteCommand command = new SQLiteCommand(sql, SqliteConn);

            if (command.ExecuteScalar() != null)
            {
                MessageBox.Show("A Race with that name already exists.");
                return false;
            }

            sql = "CREATE TABLE IF NOT EXISTS \"" + name + "\" ([RacePosition] INTEGER PRIMARY KEY AUTOINCREMENT, [Number] INTEGER)";
            command = new SQLiteCommand(sql, SqliteConn);
            command.ExecuteNonQuery();

            int racerCount = 0;
            foreach (Racer r in race.Racers)
            {
                sql = "INSERT INTO \"" + name + "\" ([Number]) VALUES (@Number)";
                command = new SQLiteCommand(sql, SqliteConn);
                command.Parameters.Add("@Number", DbType.Int64).Value = r.Number;
                racerCount += command.ExecuteNonQuery();
            }

            MessageBox.Show(racerCount + " racer(s) added to " + race.RaceName + ".");

            if (racerCount > 0) return true;
            else return false;
        }

        public ObservableCollection<Racer> GetRacerData()
        {
            ObservableCollection<Racer> Racers = new ObservableCollection<Racer>();
            SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM raceTable", SqliteConn);
            SQLiteDataAdapter sda = new SQLiteDataAdapter(cmd);
            DataSet ds = new DataSet();
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

        public static void StoreDatabaseRegistry(string database)
        {
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\DerbyApp");
            key.SetValue("database", database);
            key.Close();
        }

        public static bool GetDatabaseRegistry(out string database)
        {
            database = "";
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\DerbyApp");
            if (key != null)
            {
                if (key.GetValue("database") is string s1) database = s1;
                return true;
            }
            return false;
        }

    }
}
