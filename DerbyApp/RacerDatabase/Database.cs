using System.Data.SQLite;
using System;
using System.IO;
using System.Data;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using DerbyApp.RaceStats;
using DerbyApp.Helpers;
using System.Linq;
using System.Drawing.Imaging;

namespace DerbyApp.RacerDatabase
{
    public class Database
    {
        public readonly SQLiteConnection SqliteConn;
        public readonly string EventFile = "";
        private readonly string _racerTableName = "raceTable";
        private readonly string _settingsTableName = "settingsTable";

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

        public Database(string databaseFile)
        {
            EventFile = databaseFile;
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
            string sql = "CREATE TABLE IF NOT EXISTS [" + _racerTableName + "] ([Number] INTEGER PRIMARY KEY AUTOINCREMENT, [Name] VARCHAR(50), [Weight(oz)] DECIMAL(5, 3), [Troop] VARCHAR(10), [Level] VARCHAR(20), [Email] VARCHAR(100), [Image] MEDIUMBLOB)";
            SQLiteCommand command = new(sql, SqliteConn);
            command.ExecuteNonQuery();
        }

        public void StoreRaceSettings(string eventName)
        {
            string sql = "CREATE TABLE IF NOT EXISTS [" + _settingsTableName + "] ([Number] INTEGER PRIMARY KEY, [Name] VARCHAR(500))";
            SQLiteCommand command = new(sql, SqliteConn);
            command.ExecuteNonQuery();

            sql = "REPLACE INTO [" + _settingsTableName + "] ([Number], [Name]) VALUES (@Number, @Name)";
            command = new SQLiteCommand(sql, SqliteConn);
            command.Parameters.Add("@Number", DbType.Int64).Value = 1;
            command.Parameters.Add("@Name", DbType.String).Value = eventName;
            command.ExecuteNonQuery();
        }

        public void LoadRaceSettings(out string eventName)
        {
            SQLiteCommand command = new("SELECT * FROM [" + _settingsTableName + "] ORDER BY ROWID ASC LIMIT 1", SqliteConn);

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
                SQLiteCommand cmd = new("SELECT *  FROM [" + _racerTableName + "] INNER JOIN [" + results.RaceName + "] ON [" + results.RaceName + "].number = [" + _racerTableName + "].Number", SqliteConn);
                SQLiteDataAdapter sda = new(cmd);
                DataSet ds = new();
                sda.Fill(ds);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    results.InProgress = true;
                    results.ResultsTable.Clear();
                    foreach (DataRow row in ds.Tables[0].Rows)
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
            SQLiteCommand command;

            try
            {
                sql = "DROP TABLE [" + raceName + "]";
                command = new SQLiteCommand(sql, SqliteConn);
                command.ExecuteNonQuery();
            }
            catch { }
        }

        public void ModifyResultsTable(ObservableCollection<Racer> racers, string raceName, int heatCount, int raceFormat)
        {
            int racerCount = 0;
            string sql;
            SQLiteCommand command;

            try
            {
                sql = "DELETE FROM [" + raceName + "]";
                command = new SQLiteCommand(sql, SqliteConn);
                command.ExecuteNonQuery();
            }
            catch
            {
                sql = "CREATE TABLE IF NOT EXISTS [" + raceName + "] ([RacePosition] INTEGER PRIMARY KEY AUTOINCREMENT, [Number] INTEGER, [RaceFormat] INTEGER";
                for (int i = 0; i < heatCount; i++) sql += ", [Heat " + (i + 1) + "] DOUBLE";
                sql += ")";
                command = new SQLiteCommand(sql, SqliteConn);
                command.ExecuteNonQuery();
            }

            try
            {
                sql = "DELETE FROM sqlite_sequence WHERE NAME=[" + raceName + "]";
                command = new SQLiteCommand(sql, SqliteConn);
                command.ExecuteNonQuery();
            }
            catch { }

            foreach (Racer r in racers)
            {
                sql = "INSERT INTO [" + raceName + "] ([Number], [RaceFormat]) VALUES (@Number, @RaceFormat)";
                command = new SQLiteCommand(sql, SqliteConn);
                command.Parameters.Add("@Number", DbType.Int64).Value = r.Number;
                command.Parameters.Add("@RaceFormat", DbType.Int64).Value = raceFormat;
                racerCount += command.ExecuteNonQuery();
            }
        }

        public void AddRunOffHeat(string raceName, int heatCount)
        {
            string sql = "ALTER TABLE [" + raceName + "] ADD [Heat " + heatCount + "] DOUBLE";
            SQLiteCommand command = new(sql, SqliteConn);
            try { command.ExecuteNonQuery(); }
            catch { }
        }

        public void UpdateResultsTable(string raceName, DataRow row)
        {
            string sql = "UPDATE [" + raceName + "] SET ";
            SQLiteCommand command;

            for (int i = 2; i < row.ItemArray.Length; i++)
            {
                double? num = row.ItemArray[i] as double?;
                if (num != null)
                {
                    sql += "[Heat " + (i - 1) + "]=" + num + ", ";
                }
            }
            sql = sql.Remove(sql.Length - 2);
            sql += " WHERE [Number]=" + (int)row["Number"];
            command = new SQLiteCommand(sql, SqliteConn);
            command.ExecuteNonQuery();
        }

        public ObservableCollection<Racer> GetAllRacers(ObservableCollection<Racer> Racers = null)
        {
            SQLiteCommand cmd = new("SELECT * FROM [" + _racerTableName + "]", SqliteConn);
            SQLiteDataAdapter sda = new(cmd);
            DataSet ds = new();
            if (Racers == null) Racers = [];
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
                                         ImageHandler.ByteArrayToImage((byte[])dataRow[6])));
                    }
                    catch { }
                }
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
                string sql = "SELECT *  FROM [" + _racerTableName + "] INNER JOIN [" + raceName + "] ON [" + raceName + "].number = [" + _racerTableName + "].Number ORDER BY [" + raceName + "].RacePosition";
                SQLiteCommand cmd = new(sql, SqliteConn);
                SQLiteDataAdapter sda = new(cmd);
                DataSet ds = new();
                try
                {
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
                                                 ImageHandler.ByteArrayToImage((byte[])dataRow[6])));
                                raceFormatIndex = (int)((Int64)dataRow[9]);
                            }
                            catch { }
                        }
                    }
                }
                catch { /* invalid race name stored in registry */ }
            }

            return (Racers, raceFormatIndex);
        }

        public int GetHeatCount(string raceName)
        {
            int retVal = -1;
            if (raceName != "")
            {
                string sql = "SELECT * FROM [" + raceName + "] LIMIT 1";
                SQLiteCommand cmd = new(sql, SqliteConn);
                SQLiteDataAdapter sda = new(cmd);
                DataSet ds = new();
                try
                {
                    sda.Fill(ds);
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        string[] columnNames = (from dc in ds.Tables[0].Columns.Cast<DataColumn>()
                                                select dc.ColumnName).ToArray();
                        retVal = columnNames.Where(x => x.Contains("Heat")).Count();
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
            SQLiteCommand command = new(sql, SqliteConn);
            SQLiteDataReader r = command.ExecuteReader();
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
            SQLiteCommand command = new(sql, SqliteConn);
            MemoryStream ms = new();
            racer.Photo.Save(ms, ImageFormat.Jpeg);
            ms.Position = 0;
            byte[] photo = ms.ToArray();

            command.Parameters.Add("@Number", DbType.Int64).Value = racer.Number;
            command.Parameters.Add("@Name", DbType.String).Value = racer.RacerName;
            command.Parameters.Add("@Weight", DbType.Decimal).Value = racer.Weight;
            command.Parameters.Add("@Troop", DbType.String).Value = racer.Troop;
            command.Parameters.Add("@Level", DbType.String).Value = racer.Level;
            command.Parameters.Add("@Email", DbType.String).Value = racer.Email;
            command.Parameters.Add("@Image", DbType.Binary).Value = photo;
            command.ExecuteNonQuery();
        }

        public void RemoveRacerFromRacerTable(Racer racer)
        {
            string sql = "DELETE FROM [" + _racerTableName + "] WHERE [Number]=@Number";
            SQLiteCommand command = new(sql, SqliteConn);
            command.Parameters.Add("@Number", DbType.Int64).Value = racer.Number;
            command.ExecuteNonQuery();
        }

        public static void StoreDatabaseRegistry(string database, string activeRace, string outputFolderName)
        {
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\DerbyApp");
            if (database != null)
            {
                key.SetValue("database", database);
                if (activeRace != null) key.SetValue("activeRace", activeRace);
                key.SetValue("outputFolderName", outputFolderName);
            }
            key.Close();
        }

        public static bool GetDatabaseRegistry(out string database, out string activeRace, out string outputFolderName)
        {
            database = "";
            activeRace = "";
            outputFolderName = "";
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\DerbyApp");
            if (key != null)
            {
                if (key.GetValue("database") is string s1) database = s1;
                if (key.GetValue("activeRace") is string s2) activeRace = s2;
                if (key.GetValue("outputFolderName") is string s3) outputFolderName = s3;
                return true;
            }
            return false;
        }

    }
}
