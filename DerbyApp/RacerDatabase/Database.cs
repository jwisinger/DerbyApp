#warning TODO: If database connection is lost when addrunoff, the column lets you fill it out on the screen, but it never gets added to the database
using DerbyApp.Helpers;
using DerbyApp.RaceStats;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace DerbyApp.RacerDatabase
{
    public class Database
    {
        private readonly DatabaseGeneric _databaseGeneric;
        private readonly GoogleDriveAccess _googleDriveAccess;
        private static readonly HttpClient httpClient = new();
        private readonly string _racerTableName = "raceTable";
        private readonly string _videoTableName = "videoTable";
        private readonly string _settingsTableName = "settingsTable";
        private readonly string _outputFolderName;
        private readonly bool _sqlite;
        public bool IsSynced = true;
        public bool InitGood = false;

        public Database(string databaseFile, bool isSqlite, Credentials credentials, GoogleDriveAccess gda, string outputFolderName)
        {
            _sqlite = isSqlite;
            _googleDriveAccess = gda;
            _outputFolderName = outputFolderName;
            if (_sqlite) _databaseGeneric = new DatabaseSqlite(databaseFile); 
            else _databaseGeneric = new DatabasePostgres(databaseFile, credentials);
            if (_databaseGeneric.InitGood)
            {
                CreateRacerTable();
                CreateVideoTable();
                InitGood = true;
            }
        }

        public string GetName()
        {
            return _databaseGeneric.GetDataBaseName();
        }

        private void CreateVideoTable()
        {
            string sql;
            if (_sqlite)    // These differ because of how autoincrement is different between postgres and sqlite
            {
                sql = "CREATE TABLE IF NOT EXISTS [" + _videoTableName + "] ([Number] INTEGER PRIMARY KEY AUTOINCREMENT, [RaceName] VARCHAR(50), [HeatNumber] INTEGER, [Url] VARCHAR(200), UNIQUE ([RaceName], [HeatNumber]))";
            }
            else
            {
                sql = "CREATE TABLE IF NOT EXISTS [" + _videoTableName + "] ([Number] INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY, [RaceName] VARCHAR(50), [HeatNumber] INTEGER, [Url] VARCHAR(200), UNIQUE ([RaceName], [HeatNumber]))";
            }
            _databaseGeneric.ExecuteNonQuery(sql);
        }

        private void CreateRacerTable()
        {
            string sql;
            if (_sqlite)    // These differ because of how autoincrement is different between postgres and sqlite
            {
                sql = "CREATE TABLE IF NOT EXISTS [" + _racerTableName + "] ([Number] INTEGER PRIMARY KEY AUTOINCREMENT, [Name] VARCHAR(50), [Weight(oz)] DECIMAL(5, 3), [Troop] VARCHAR(10), [Level] VARCHAR(20), [Email] VARCHAR(100), [Image] MEDIUMBLOB, [ImageKey] VARCHAR(50))";
            }
            else
            {
                sql = "CREATE TABLE IF NOT EXISTS [" + _racerTableName + "] ([Number] INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY, [Name] VARCHAR(50), [Weight(oz)] DECIMAL(5, 3), [Troop] VARCHAR(10), [Level] VARCHAR(20), [Email] VARCHAR(100), [Image] VARCHAR(150), [ImageKey] VARCHAR(50))";
            }
            _databaseGeneric.ExecuteNonQuery(sql);
            sql = "CREATE TABLE IF NOT EXISTS [" + _settingsTableName + "] ([Number] INTEGER PRIMARY KEY, [Name] VARCHAR(500))";
            _databaseGeneric.ExecuteNonQuery(sql);
        }

        public void StoreRaceSettings(string eventName)
        {
            string sql = "CREATE TABLE IF NOT EXISTS [" + _settingsTableName + "] ([Number] INTEGER PRIMARY KEY, [Name] VARCHAR(500))";
            _databaseGeneric.ExecuteNonQuery(sql);

            sql = "REPLACE INTO [" + _settingsTableName + "] ([Number], [Name]) VALUES (@Number, @Name)";
            List<DatabaseGeneric.SqlParameter> param =
            [
                new DatabaseGeneric.SqlParameter { name = "@Number", type = DatabaseGeneric.DataType.Integer, value = 1 },
                new DatabaseGeneric.SqlParameter { name = "@Name", type = DatabaseGeneric.DataType.Text, value = eventName },
            ];
            _databaseGeneric.ExecuteNonQueryWithParams(sql, param);
        }

        public void LoadRaceSettings(out string eventName)
        {
            string sql = "SELECT * FROM [" + _settingsTableName + "] ORDER BY [Number] ASC LIMIT 1";
            _databaseGeneric.ExecuteReader(sql);
            if (_databaseGeneric.Read())
            {
                eventName = (string)_databaseGeneric.GetReadValue("Name");
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
                string sql = "SELECT *  FROM [" + _racerTableName + "] INNER JOIN [" + results.RaceName + "] ON [" + results.RaceName + "].[Number] = [" + _racerTableName + "].[Number]";
                _databaseGeneric.ExecuteReader(sql);
                DataTable dt = new();
                dt.Load(_databaseGeneric.GetDataReader());
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
            try
            {
                string sql = "DROP TABLE [" + raceName + "]";
                _databaseGeneric.ExecuteNonQuery(sql);
            }
            catch { }
        }

        public void ModifyResultsTable(ObservableCollection<Racer> racers, string raceName, int heatCount, int raceFormat)
        {
            int racerCount = 0;
            string sql;

            sql = "DELETE FROM [" + raceName + "]";
            if (_databaseGeneric.ExecuteNonQuery(sql) < 0)
            {
                if (_sqlite)    // These differ because of how autoincrement is different between postgres and sqlite
                {
                    sql = "CREATE TABLE IF NOT EXISTS [" + raceName + "] ([RacePosition] INTEGER PRIMARY KEY AUTOINCREMENT, [Number] INTEGER, [RaceFormat] INTEGER";
                }
                else
                {
                    sql = "CREATE TABLE IF NOT EXISTS [" + raceName + "] ([RacePosition] INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY, [Number] INTEGER, [RaceFormat] INTEGER";
                }

                for (int i = 0; i < heatCount; i++) sql += ", [Heat " + (i + 1) + "] REAL";
                sql += ")";
                _databaseGeneric.ExecuteNonQuery(sql);
            }

            try
            {
                if (_sqlite)    // These differ because of how reseting the ID key is different between postgres and sqlite
                {
                    sql = "DELETE FROM sqlite_sequence WHERE NAME=[" + raceName + "]";
                }
                else
                {
                    sql = "TRUNCATE TABLE [" + raceName + "] RESTART IDENTITY";
                }
                racerCount += _databaseGeneric.ExecuteNonQuery(sql);
            }
            catch { }

            foreach (Racer r in racers)
            {
                sql = "INSERT INTO [" + raceName + "] ([Number], [RaceFormat]) VALUES (@Number, @RaceFormat)";
                List<DatabaseGeneric.SqlParameter> param =
                [
                    new DatabaseGeneric.SqlParameter { name = "@Number", type = DatabaseGeneric.DataType.Integer, value = r.Number },
                    new DatabaseGeneric.SqlParameter { name = "@RaceFormat", type = DatabaseGeneric.DataType.Integer, value = raceFormat },
                ];
                _databaseGeneric.ExecuteNonQueryWithParams(sql, param);
            }
        }

        public void AddRunOffHeat(string raceName, int heatCount)
        {
            string sql = "ALTER TABLE [" + raceName + "] ADD [Heat " + heatCount + "] DOUBLE";
            try
            {
                _databaseGeneric.ExecuteNonQuery(sql);
            }
            catch { }
        }

        public void UpdateResultsTable(string raceName, DataRow row)
        {
            string sql = "UPDATE [" + raceName + "] SET ";

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

            try
            {
                if (_databaseGeneric.ExecuteNonQuery(sql) == 0) IsSynced = false;
                else IsSynced = true;
            }
            catch
            {
                IsSynced = false;
            }
        }

        public void AddVideoToTable(VideoUploadedEventArgs e)
        {
            string sql = "INSERT INTO [" + _videoTableName + "] ([Url], [HeatNumber], [RaceName]) VALUES (@url, @heatnumber, @racename) ON CONFLICT ([HeatNumber], [RaceName]) DO UPDATE SET [Url] = EXCLUDED.[Url]";

            List<DatabaseGeneric.SqlParameter> param =
            [
                new DatabaseGeneric.SqlParameter { name = "@url", type = DatabaseGeneric.DataType.Text, value = e.Url },
                new DatabaseGeneric.SqlParameter { name = "@heatnumber", type = DatabaseGeneric.DataType.Integer, value = e.HeatNumber },
                new DatabaseGeneric.SqlParameter { name = "@racename", type = DatabaseGeneric.DataType.Text, value = e.RaceName },
            ];

            try
            {
                _databaseGeneric.ExecuteNonQueryWithParams(sql, param);
            }
            catch
            {
            }
        }

        public void UpdateResultsTable(string raceName, DataTable table, string outputFolderName)
        {
            foreach (DataRow row in table.Rows)
            {
                string sql = "UPDATE [" + raceName + "] SET ";
                bool emptyRow = true;

                for (int i = 2; i < row.ItemArray.Length; i++)
                {
                    double? num = row.ItemArray[i] as double?;
                    if (num != null)
                    {
                        sql += "[Heat " + (i - 1) + "]=" + num + ", ";
                        emptyRow = false;
                    }
                }
                if (emptyRow) continue;
                sql = sql[..^2]; 
                sql += " WHERE [Number]=" + (int)row["Number"];

                try
                {
                    if (_databaseGeneric.ExecuteNonQuery(sql) < 1)
                    {
                        IsSynced = false;
                        break;
                    }
                    else IsSynced = true;
                }
                catch
                {
                    IsSynced = false;
                    break;
                }
            }
            if (!IsSynced)
            {
                table.TableName = raceName;
                table.WriteXml(Path.Combine(outputFolderName, "databaseBackup_" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + ".xml"));
            }
        }

        public ObservableCollection<Racer> GetAllRacers(ObservableCollection<Racer> Racers = null)
        {
            string sql = "SELECT * FROM [" + _racerTableName + "]";
            _databaseGeneric.ExecuteReader(sql);
            if (Racers == null) Racers = [];
            else Racers.Clear();
            while (_databaseGeneric.Read())
            {
                try
                {
                    if (_sqlite)
                    {
                        Racers.Add(new Racer(Convert.ToInt64(_databaseGeneric.GetReadValue("number")),
                                         (string)_databaseGeneric.GetReadValue("name"),
                                         Convert.ToDecimal(_databaseGeneric.GetReadValue("weight(oz)")),
                                         (string)_databaseGeneric.GetReadValue("troop"),
                                         (string)_databaseGeneric.GetReadValue("level"),
                                         (string)_databaseGeneric.GetReadValue("email"),
                                         ImageHandler.ByteArrayToImage((byte[])_databaseGeneric.GetReadValue("image"))));
                    }
                    else
                    {
                        string guid = (string)_databaseGeneric.GetReadValue("imagekey");
                        string path = Path.Combine(_outputFolderName, "racer_images");
                        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                        Racer racer = new(Convert.ToInt64(_databaseGeneric.GetReadValue("number")),
                                         (string)_databaseGeneric.GetReadValue("name"),
                                         Convert.ToDecimal(_databaseGeneric.GetReadValue("weight(oz)")),
                                         (string)_databaseGeneric.GetReadValue("troop"),
                                         (string)_databaseGeneric.GetReadValue("level"),
                                         (string)_databaseGeneric.GetReadValue("email"),
                                         null);
                        Racers.Add(racer);
                        try
                        {
                            ImageDownloader.SetPhoto(racer, Path.Combine(path, guid + ".png"));
                        }
                        catch
                        {
                            _ = ImageDownloader.DownloadImageAsync((string)_databaseGeneric.GetReadValue("image"), path, guid + ".png", racer);
                        }
                    }
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
                    string sql = "SELECT * FROM [" + _racerTableName + "] INNER JOIN [" + raceName + "] ON [" + raceName + "].[Number] = [" + _racerTableName + "].[Number] ORDER BY [" + raceName + "].[RacePosition]";
                    _databaseGeneric.ExecuteReader(sql);
                    while (_databaseGeneric.Read())
                    {
                        try
                        {
                            if (_sqlite)
                            {
                                Racers.Add(new Racer(Convert.ToInt64(_databaseGeneric.GetReadValue("number")),
                                                 (string)_databaseGeneric.GetReadValue("name"),
                                                 Convert.ToDecimal(_databaseGeneric.GetReadValue("weight(oz)")),
                                                 (string)_databaseGeneric.GetReadValue("troop"),
                                                 (string)_databaseGeneric.GetReadValue("level"),
                                                 (string)_databaseGeneric.GetReadValue("email"),
                                                 ImageHandler.ByteArrayToImage((byte[])_databaseGeneric.GetReadValue("image"))));
                            }
                            else
                            {
                                string guid = (string)_databaseGeneric.GetReadValue("imagekey");
                                string path = Path.Combine(_outputFolderName, "racer_images");
                                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                                Racer racer = new(Convert.ToInt64(_databaseGeneric.GetReadValue("number")),
                                                 (string)_databaseGeneric.GetReadValue("name"),
                                                 Convert.ToDecimal(_databaseGeneric.GetReadValue("weight(oz)")),
                                                 (string)_databaseGeneric.GetReadValue("troop"),
                                                 (string)_databaseGeneric.GetReadValue("level"),
                                                 (string)_databaseGeneric.GetReadValue("email"),
                                                 null);
                                Racers.Add(racer);
                                try
                                {
                                    ImageDownloader.SetPhoto(racer, Path.Combine(path, guid + ".png"));
                                }
                                catch
                                {
                                    _ = ImageDownloader.DownloadImageAsync((string)_databaseGeneric.GetReadValue("image"), path, guid + ".png", racer);
                                }
                            }
                            raceFormatIndex = (int)Convert.ChangeType(_databaseGeneric.GetReadValue("RaceFormat"), _databaseGeneric.GetReadValue("RaceFormat").GetType());
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
                    _databaseGeneric.ExecuteReader(sql);
                    retVal = 0;
                    for (int i = 0; i < _databaseGeneric.GetReadFieldCount(); i++)
                    {
                        if (_databaseGeneric.GetReadFieldName(i).Contains("Heat")) retVal++;
                    }
                }
                catch { }
            }

            return retVal;
        }

        public ObservableCollection<string> GetListOfRaces()
        {
            ObservableCollection<string> retVal = [];
            string sql;
            if (_sqlite)    // These differ because of how tables are stored in postgres vs sqlite
            {
                sql = "SELECT name FROM sqlite_schema WHERE type ='table' AND (name NOT LIKE 'sqlite_%') AND (name NOT LIKE 'settings%');";
                _databaseGeneric.ExecuteReader(sql);
                while (_databaseGeneric.Read()) retVal.Add(Convert.ToString(_databaseGeneric.GetReadValue("name")));
            }
            else
            {
                sql = "SELECT table_name FROM information_schema.tables WHERE table_schema NOT IN('pg_catalog', 'information_schema') AND table_type = 'BASE TABLE'";
                _databaseGeneric.ExecuteReader(sql);
                while (_databaseGeneric.Read()) retVal.Add(Convert.ToString(_databaseGeneric.GetReadValue("table_name")));
            }
            retVal.Remove(_settingsTableName);
            retVal.Remove(_racerTableName);
            retVal.Remove(_videoTableName);
            return retVal;
        }

        public void AddRacerToRacerTable(Racer racer)
        {
            string sql;

            if (racer.Number == 0)
            {
                sql = "INSERT INTO [" + _racerTableName + "] ([Name], [Weight(oz)], [Troop], [Level], [Email], [Image], [ImageKey]) VALUES (@Name, @Weight, @Troop, @Level, @Email, @Image, @ImageKey)";
            }
            else
            {
                sql = "UPDATE [" + _racerTableName + "] SET [Name] = @Name, [Weight(oz)] = @Weight, [Troop] =  @Troop, [Level] = @Level, [Email] = @Email, [Image] = @Image, [ImageKey] = @ImageKey WHERE [Number] = @Number";
            }

            List<DatabaseGeneric.SqlParameter> param =
            [
                new DatabaseGeneric.SqlParameter { name = "@Number", type = DatabaseGeneric.DataType.Integer, value = racer.Number },
                new DatabaseGeneric.SqlParameter { name = "@Name", type = DatabaseGeneric.DataType.Text, value = racer.RacerName },
                new DatabaseGeneric.SqlParameter { name = "@Weight", type = DatabaseGeneric.DataType.Real, value = racer.Weight },
                new DatabaseGeneric.SqlParameter { name = "@Troop", type = DatabaseGeneric.DataType.Text, value = racer.Troop },
                new DatabaseGeneric.SqlParameter { name = "@Level", type = DatabaseGeneric.DataType.Text, value = racer.Level },
                new DatabaseGeneric.SqlParameter { name = "@Email", type = DatabaseGeneric.DataType.Text, value = racer.Email },
            ];

            MemoryStream ms = new();
            racer.Photo.Save(ms, ImageFormat.Png);
            ms.Position = 0;

            if (_sqlite)
            {
                param.Add(new DatabaseGeneric.SqlParameter { name = "@Image", type = DatabaseGeneric.DataType.Blob, value = ms.ToArray() });
            }
            else
            {
                string guid = ShortGuid.GenerateShortGuid();
                param.Add(new DatabaseGeneric.SqlParameter { name = "@Image", type = DatabaseGeneric.DataType.Text, value = _googleDriveAccess.UploadFile(guid + ".png", ms) });
                param.Add(new DatabaseGeneric.SqlParameter { name = "@ImageKey", type = DatabaseGeneric.DataType.Text, value = guid });
                string path = Path.Combine(_outputFolderName, "racer_images");
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                racer.Photo.Save(Path.Combine(path, guid + ".png"), ImageFormat.Png);
            }
            _databaseGeneric.ExecuteNonQueryWithParams(sql, param);
        }

        public void RemoveRacerFromRacerTable(Racer racer)
        {
            string sql = "DELETE FROM [" + _racerTableName + "] WHERE [Number]=@Number";
            List<DatabaseGeneric.SqlParameter> param =
            [
                new DatabaseGeneric.SqlParameter { name = "@Number", type = DatabaseGeneric.DataType.Integer, value = racer.Number },
            ];
            _databaseGeneric.ExecuteNonQueryWithParams(sql, param);
        }

        public static void StoreDatabaseRegistry(string database, string activeRace, string outputFolderName, bool timeBasedScoring, int maxRaceTime, string qrCodeLink, string qrPrinterName, string licensePrinterName, string password)
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
                key.SetValue("password", password);
            }
            key.Close();
        }

        public static bool GetDatabaseRegistry(out string database, out string activeRace, out string outputFolderName, out bool timeBasedScoring, out int maxRaceTime, out string qrCodeLink, out string qrPrinterName, out string licensePrinterName, out string password)
        {
            database = "";
            activeRace = "";
            outputFolderName = "";
            qrCodeLink = "";
            qrPrinterName = "";
            licensePrinterName = "";
            password = "";
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
                if (key.GetValue("password") is string s9) password = s9;
                return true;
            }
            return false;
        }
    }
}
