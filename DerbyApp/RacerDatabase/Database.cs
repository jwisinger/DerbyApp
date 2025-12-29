using DerbyApp.Helpers;
using DerbyApp.RaceStats;
using Microsoft.Data.Sqlite;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;

namespace DerbyApp.RacerDatabase
{
    public class Database
    {
        public readonly SqliteConnection SqliteConn;
        public readonly string EventFile = "";
        private readonly string _racerTableName = "raceTable";
        private readonly string _settingsTableName = "settingsTable";

        private SqliteConnection CreateConnection()
        {
            try
            {
                SqliteConn.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return SqliteConn;
        }

        public Database(string databaseFile)
        {
            EventFile = databaseFile;
            SqliteConn = new SqliteConnection("Data Source = " + databaseFile);
            CreateConnection();
            CreateRacerTable();
        }

        private void CreateRacerTable()
        {
            string sql = "CREATE TABLE IF NOT EXISTS [" + _racerTableName + "] ([Number] INTEGER PRIMARY KEY AUTOINCREMENT, [Name] VARCHAR(50), [Weight(oz)] DECIMAL(5, 3), [Troop] VARCHAR(10), [Level] VARCHAR(20), [Email] VARCHAR(100), [Image] MEDIUMBLOB)";
            SqliteCommand command = new(sql, SqliteConn);
            command.ExecuteNonQuery();
            sql = "CREATE TABLE IF NOT EXISTS [" + _settingsTableName + "] ([Number] INTEGER PRIMARY KEY, [Name] VARCHAR(500))";
            command = new(sql, SqliteConn);
            command.ExecuteNonQuery();
        }

        public void StoreRaceSettings(string eventName)
        {
            string sql = "CREATE TABLE IF NOT EXISTS [" + _settingsTableName + "] ([Number] INTEGER PRIMARY KEY, [Name] VARCHAR(500))";
            SqliteCommand command = new(sql, SqliteConn);
            command.ExecuteNonQuery();

            sql = "REPLACE INTO [" + _settingsTableName + "] ([Number], [Name]) VALUES (@Number, @Name)";
            command = new SqliteCommand(sql, SqliteConn);
            command.Parameters.Add("@Number", SqliteType.Integer).Value = 1;
            command.Parameters.Add("@Name", SqliteType.Text).Value = eventName;
            command.ExecuteNonQuery();
        }

        public void LoadRaceSettings(out string eventName)
        {
            SqliteCommand command = new("SELECT * FROM [" + _settingsTableName + "] ORDER BY ROWID ASC LIMIT 1", SqliteConn);

            using var reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                reader.Read();
                //var id = reader.GetInt64(0);
                eventName = reader.GetString(1);
            }
            else
            {
                eventName = "";
            }
        }

        public void LoadResultsTable(RaceResults results)
        {
            try
            {
                if (results.RaceName == "") return;
                SqliteCommand cmd = new("SELECT *  FROM [" + _racerTableName + "] INNER JOIN [" + results.RaceName + "] ON [" + results.RaceName + "].number = [" + _racerTableName + "].Number", SqliteConn);
                SqliteDataReader r = cmd.ExecuteReader();
                DataTable dt = new();
                dt.Load(r);
                if (dt.Rows.Count > 0)
                {
                    results.InProgress = true;
                    results.ResultsTable.Clear();
                    foreach (DataRow row in dt.Rows)
                    {
                        results.ResultsTable.ImportRow(row);
                    }
                }
            }
            catch { }
        }

        public void DeleteResultsTable(string raceName)
        {
            string sql;
            SqliteCommand command;

            try
            {
                sql = "DROP TABLE [" + raceName + "]";
                command = new SqliteCommand(sql, SqliteConn);
                command.ExecuteNonQuery();
            }
            catch { }
        }

        public void ModifyResultsTable(ObservableCollection<Racer> racers, string raceName, int heatCount, int raceFormat)
        {
            int racerCount = 0;
            string sql;
            SqliteCommand command;

            try
            {
                sql = "DELETE FROM [" + raceName + "]";
                command = new SqliteCommand(sql, SqliteConn);
                command.ExecuteNonQuery();
            }
            catch
            {
                sql = "CREATE TABLE IF NOT EXISTS [" + raceName + "] ([RacePosition] INTEGER PRIMARY KEY AUTOINCREMENT, [Number] INTEGER, [RaceFormat] INTEGER";
                for (int i = 0; i < heatCount; i++) sql += ", [Heat " + (i + 1) + "] DOUBLE";
                sql += ")";
                command = new SqliteCommand(sql, SqliteConn);
                command.ExecuteNonQuery();
            }

            try
            {
                sql = "DELETE FROM sqlite_sequence WHERE NAME=[" + raceName + "]";
                command = new SqliteCommand(sql, SqliteConn);
                command.ExecuteNonQuery();
            }
            catch { }

            foreach (Racer r in racers)
            {
                sql = "INSERT INTO [" + raceName + "] ([Number], [RaceFormat]) VALUES (@Number, @RaceFormat)";
                command = new SqliteCommand(sql, SqliteConn);
                command.Parameters.Add("@Number", SqliteType.Integer).Value = r.Number;
                command.Parameters.Add("@RaceFormat", SqliteType.Integer).Value = raceFormat;
                racerCount += command.ExecuteNonQuery();
            }
        }

        public void AddRunOffHeat(string raceName, int heatCount)
        {
            string sql = "ALTER TABLE [" + raceName + "] ADD [Heat " + heatCount + "] DOUBLE";
            SqliteCommand command = new(sql, SqliteConn);
            try { command.ExecuteNonQuery(); }
            catch { }
        }

        public void UpdateResultsTable(string raceName, DataRow row)
        {
            string sql = "UPDATE [" + raceName + "] SET ";
            SqliteCommand command;

            for (int i = 2; i < row.ItemArray.Length; i++)
            {
                double? num = row.ItemArray[i] as double?;
                if (num != null)
                {
                    sql += "[Heat " + (i - 1) + "]=" + num + ", ";
                }
            }
            sql = sql[..^2];
            sql += " WHERE [Number]=" + (int)row["Number"];
            command = new SqliteCommand(sql, SqliteConn);
            command.ExecuteNonQuery();
        }

        public ObservableCollection<Racer> GetAllRacers(ObservableCollection<Racer> Racers = null)
        {
            SqliteCommand cmd = new("SELECT * FROM [" + _racerTableName + "]", SqliteConn);
            SqliteDataReader r = cmd.ExecuteReader();
            DataSet ds = new();
            if (Racers == null) Racers = [];
            else Racers.Clear();
            while (r.Read())
            {
                try
                {
                    Racers.Add(new Racer((Int64)r["number"],
                                     (string)r["name"],
                                     Convert.ToDecimal(r["weight(oz)"]),
                                     (string)r["troop"],
                                     (string)r["level"],
                                     (string)r["email"],
                                     ImageHandler.ByteArrayToImage((byte[])r["image"])));
                }
                catch { }
            }

            return Racers;
        }

        public (ObservableCollection<Racer>, int) GetRacers(string raceName, ObservableCollection<Racer> Racers = null)
        {
            if (Racers == null) Racers = [];
            else Racers.Clear();
            int raceFormatIndex = -1;
            if (raceName != "")
            {
                try
                {

                    string sql = "SELECT *  FROM [" + _racerTableName + "] INNER JOIN [" + raceName + "] ON [" + raceName + "].number = [" + _racerTableName + "].Number ORDER BY [" + raceName + "].RacePosition";
                    SqliteCommand cmd = new(sql, SqliteConn);
                    SqliteDataReader r = cmd.ExecuteReader();
                    DataSet ds = new();
                    while (r.Read())
                    {
                        try
                        {
                            Racers.Add(new Racer((Int64)r["number"],
                                             (string)r["name"],
                                             Convert.ToDecimal(r["weight(oz)"]),
                                             (string)r["troop"],
                                             (string)r["level"],
                                             (string)r["email"],
                                             ImageHandler.ByteArrayToImage((byte[])r["image"])));
                            raceFormatIndex = (int)(Int64)r[9];
                        }
                        catch { }
                    }
                }
                catch { }
            }

            return (Racers, raceFormatIndex);
        }

        public int GetHeatCount(string raceName)
        {
            int retVal = -1;
            if (raceName != "")
            {
                try
                {

                    string sql = "SELECT * FROM [" + raceName + "] LIMIT 1";
                    SqliteCommand cmd = new(sql, SqliteConn);
                    SqliteDataReader r = cmd.ExecuteReader();
                    retVal = 0;
                    for (int i = 0; i < r.FieldCount; i++)
                    {
                            if (r.GetName(i).Contains("Heat")) retVal++;
                    }
                }
                catch { }
            }

            return retVal;
        }

        public ObservableCollection<string> GetListOfRaces()
        {
            ObservableCollection<string> retVal = [];
            string sql = "SELECT name FROM sqlite_schema WHERE type ='table' AND (name NOT LIKE 'sqlite_%') AND (name NOT LIKE 'settings%');";
            SqliteCommand command = new(sql, SqliteConn);
            SqliteDataReader r = command.ExecuteReader();
            while (r.Read()) retVal.Add(Convert.ToString(r["name"]));
            retVal.Remove(_racerTableName);
            return retVal;
        }

        public void AddRacerToRacerTable(Racer racer)
        {
            string sql;
            if (racer.Number == 0)
            {
                sql = "REPLACE INTO [" + _racerTableName + "] ([Name], [Weight(oz)], [Troop], [Level], [Email], [Image]) VALUES (@Name, @Weight, @Troop, @Level, @Email, @Image)";
            }
            else
            {
                sql = "REPLACE INTO [" + _racerTableName + "] ([Number], [Name], [Weight(oz)], [Troop], [Level], [Email], [Image]) VALUES (@Number, @Name, @Weight, @Troop, @Level, @Email, @Image)";
            }
            SqliteCommand command = new(sql, SqliteConn);
            MemoryStream ms = new();
            racer.Photo.Save(ms, ImageFormat.Jpeg);
            ms.Position = 0;
            byte[] photo = ms.ToArray();

            command.Parameters.Add("@Number", SqliteType.Integer).Value = racer.Number;
            command.Parameters.Add("@Name", SqliteType.Text).Value = racer.RacerName;
            command.Parameters.Add("@Weight", SqliteType.Real).Value = racer.Weight;
            command.Parameters.Add("@Troop", SqliteType.Text).Value = racer.Troop;
            command.Parameters.Add("@Level", SqliteType.Text).Value = racer.Level;
            command.Parameters.Add("@Email", SqliteType.Text).Value = racer.Email;
            command.Parameters.Add("@Image", SqliteType.Blob).Value = photo;
            command.ExecuteNonQuery();
        }

        public void RemoveRacerFromRacerTable(Racer racer)
        {
            string sql = "DELETE FROM [" + _racerTableName + "] WHERE [Number]=@Number";
            SqliteCommand command = new(sql, SqliteConn);
            command.Parameters.Add("@Number", SqliteType.Integer).Value = racer.Number;
            command.ExecuteNonQuery();
        }

        public static void StoreDatabaseRegistry(string database, string activeRace, string outputFolderName, bool timeBasedScoring, int maxRaceTime, string qrCodeLink, string qrPrinterName, string licensePrinterName)
        {
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\DerbyApp");
            if (database != null)
            {
                key.SetValue("database", database);
                if (activeRace != null) key.SetValue("activeRace", activeRace);
                key.SetValue("outputFolderName", outputFolderName);
                key.SetValue("timeBasedScoring", timeBasedScoring);
                key.SetValue("maxRaceTime", maxRaceTime);
                key.SetValue("qrCodeLink", qrCodeLink);
                key.SetValue("qrPrinterName", qrPrinterName);
                key.SetValue("licensePrinterName", licensePrinterName);
            }
            key.Close();
        }

        public static bool GetDatabaseRegistry(out string database, out string activeRace, out string outputFolderName, out bool timeBasedScoring, out int maxRaceTime, out string qrCodeLink, out string qrPrinterName, out string licensePrinterName)
        {
            database = "";
            activeRace = "";
            outputFolderName = "";
            qrCodeLink = "";
            qrPrinterName = "";
            licensePrinterName = "";
            timeBasedScoring = false;
            maxRaceTime = 10;
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\DerbyApp");
            if (key != null)
            {
                if (key.GetValue("database") is string s1) database = s1;
                if (key.GetValue("activeRace") is string s2) activeRace = s2;
                if (key.GetValue("outputFolderName") is string s3) outputFolderName = s3;
                if (key.GetValue("timeBasedScoring") is string s4) if (bool.TryParse(s4, out bool b)) timeBasedScoring = b;
                if (key.GetValue("maxRaceTime") is int s5) maxRaceTime = s5;
                if (key.GetValue("qrCodeLink") is string s6) qrCodeLink = s6;
                if (key.GetValue("qrPrinterName") is string s7) qrPrinterName = s7;
                if (key.GetValue("licensePrinterName") is string s8) licensePrinterName = s8;
                return true;
            }
            return false;
        }

    }
}
