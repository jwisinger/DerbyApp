#warning TODO: If database connection is lost when addrunoff, the column lets you fill it out on the screen, but it never gets added to the database
#warning CLEANUP: I'm not sure the addrunoffheat and heatcount stuff is done correctly
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using DerbyApp.Helpers;
using DerbyApp.RaceStats;

namespace DerbyApp.RacerDatabase
{
    public class Database : INotifyPropertyChanged
    {
        private readonly DatabaseGeneric _databaseGeneric;
        private readonly GoogleDriveAccess _googleDriveAccess;
        private string _currentRaceName = "";
        private string _eventName = "";
        private string _licensePrinter = "";
        private string _qrPrinter = "";
        private string _qrCodeLink = "";
        private string _outputFolderName;
        private bool _timeBasedScoring = false;
        private int _currentHeatNumber = 1;

        public readonly bool IsSqlite;
        public bool IsSynced = true;
        public bool InitGood = false;
        public bool RaceInProgress = false;
        public TrulyObservableCollection<Racer> Racers = [];
        public ObservableCollection<string> Races = [];
        public TrulyObservableCollection<Racer> CurrentRaceRacers = [];
        public DataTable ResultsTable = new();
        public RaceFormat RaceFormat = RaceFormats.Formats[RaceFormats.DefaultFormat].Clone();
        public int HeatCount;

        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangedEventHandler ColumnAdded;

        #region Public Properties
        public string CurrentRaceName
        {
            get => _currentRaceName;
            set
            {
#warning TODO: The cbName combobox in EditRace should get updated here, but the binding is broken
                _currentRaceName = value;
                DatabaseRegistry.StoreDatabaseRegistry(null, _currentRaceName, null, null, null, null, null, null, null);
                if ((_currentRaceName != null) && (_currentRaceName != ""))
                {
                    GetRaceFormat();
                    LoadRaceSettings();
                    GetCurrentRacers();
                    CreateResultsTable();
                }
                else
                {
                    CurrentRaceRacers.Clear();
                    ResultsTable.Clear();
                    RaceInProgress = false;
                }
            }
        }

        public string EventName
        {
            get => _eventName;
            set
            {
                _eventName = value;
                StoreRaceSettings();
            }
        }

        public string OutputFolderName
        {
            get => _outputFolderName;
            set
            {
                _outputFolderName = value;
                DatabaseRegistry.StoreDatabaseRegistry(null, null, value, null, null, null, null, null, null);
            }
        }

        public bool TimeBasedScoring
        {
            get => _timeBasedScoring;
            set
            {
                _timeBasedScoring = value;
                DatabaseRegistry.StoreDatabaseRegistry(null, null, null, value, null, null, null, null, null);
            }
        }

        public string LicensePrinter
        {
            get => _licensePrinter;
            set
            {
                _licensePrinter = value;
                DatabaseRegistry.StoreDatabaseRegistry(null, null, null, null, null, null, null, value, null);
            }
        }

        public string QrPrinter
        {
            get => _qrPrinter;
            set
            {
                _qrPrinter = value;
                DatabaseRegistry.StoreDatabaseRegistry(null, null, null, null, null, null, value, null, null);
            }
        }

        public string QrCodeLink
        {
            get => _qrCodeLink;
            set
            {
                _qrCodeLink = value;
                DatabaseRegistry.StoreDatabaseRegistry(null, null, null, null, null, value, null, null, null);
            }
        }

        public int CurrentHeatNumber
        {
            get => _currentHeatNumber;
            set
            {
                _currentHeatNumber = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentHeatNumber)));
            }
        }
        #endregion

        #region Constructor
        public Database(string databaseFile, Credentials credentials, GoogleDriveAccess gda, string outputFolderName)
        {
            if (databaseFile.Contains(':')) IsSqlite = true;
            else IsSqlite = false;
            OutputFolderName = Path.Combine(outputFolderName, Path.GetFileNameWithoutExtension(databaseFile));

            if (IsSqlite) _databaseGeneric = new DatabaseSqlite(databaseFile); 
            else 
            {
                _googleDriveAccess = gda;
                _databaseGeneric = new DatabasePostgres(databaseFile, credentials);
            }

            if (_databaseGeneric.InitGood)
            {
                CreateRacerTable();
                CreateSettingsTable();
                CreateVideoTable();
                RefreshDatabase();
                LoadRaceSettings();
                InitGood = true;
            }
        }
        #endregion

        #region Database Management
        public void RefreshDatabase()
        {
            GetAllRacers();
            GetListOfRaces();
        }

        public void CopyDatabaseToLocal()
        {
            if (!IsSqlite)
            {
                string postGresConnStr = _databaseGeneric.GetConnectionString(false);

                if (postGresConnStr != "")
                {
                    DatabaseMigrator.Migrate(MigrationDirection.PostgresToSqlite, Path.Combine(OutputFolderName, EventName + "_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".sqlite"), postGresConnStr);
                }
            }
        }
        
        public bool TestConnection()
        {
#warning TODO: Put this back, but it needs to ensure it doesn't conflict with other database commands
            return true;// _databaseGeneric.TestConnection();
        }
        #endregion
                
        #region Settings Management
        private void CreateSettingsTable()
        {
            _databaseGeneric.ExecuteNonQuery(DatabaseQueries.CreateSettingsTable());
        }

        public void StoreRaceSettings()
        {
            string sql = DatabaseQueries.StoreRaceSettings(EventName, out List<DatabaseGeneric.SqlParameter> parameters);
            _databaseGeneric.ExecuteNonQueryWithParams(sql, parameters);
        }

        public void LoadRaceSettings()
        {
            _databaseGeneric.ExecuteReader(DatabaseQueries.LoadRaceSettings());
            if (_databaseGeneric.Read()) _eventName = (string)_databaseGeneric.GetReadValue("Name");
            else _eventName = "";
        }
        #endregion

        #region Video Management
        private void CreateVideoTable()
        {
            _databaseGeneric.ExecuteNonQuery(DatabaseQueries.CreateVideoTable(IsSqlite));
        }

        public void AddVideoToTable(VideoUploadedEventArgs e)
        {
            string sql = DatabaseQueries.AddVideoToTable(e, out List<DatabaseGeneric.SqlParameter> parameters);
            _databaseGeneric.ExecuteNonQueryWithParams(sql, parameters);
        }
        #endregion

        #region Race Management
        private void GetRaceFormat()
        {
            HeatCount = 0;
            _databaseGeneric.ExecuteReader(DatabaseQueries.GetRaceFormat(CurrentRaceName));
            for (int i = 0; i < _databaseGeneric.GetReadFieldCount(); i++)
            {
                if (_databaseGeneric.GetReadFieldName(i).Contains("Heat")) HeatCount++;
            }

            try
            {
                _databaseGeneric.Read();
                int raceFormatIndex = (int)Convert.ChangeType(_databaseGeneric.GetReadValue("RaceFormat"), _databaseGeneric.GetReadValue("RaceFormat").GetType());
                RaceFormat = RaceFormats.Formats[raceFormatIndex];
            }
            catch
            {
                RaceFormat = RaceFormats.Formats[RaceFormats.DefaultFormat];
            }
        }

        private void GetListOfRaces()
        {
            string sql = DatabaseQueries.GetListOfRaces(IsSqlite, out string tableName);
            _databaseGeneric.ExecuteReader(sql);
            Races.Clear();
            while (_databaseGeneric.Read()) Races.Add(Convert.ToString(_databaseGeneric.GetReadValue(tableName)));
            Races.Remove(DatabaseQueries.SettingsTableName);
            Races.Remove(DatabaseQueries.RacerTableName);
            Races.Remove(DatabaseQueries.VideoTableName);
        }

        public bool AddRace(string raceName, int raceFormatIndex)
        {
#warning CLEANUP: Store raceFormatIndex in settings table instead of a column in the table (maybe store runoff heat count there too)
            if (Races.Contains(raceName)) return false;
            Races.Add(raceName);
            _databaseGeneric.ExecuteNonQuery(DatabaseQueries.CreateResultsTable(raceName));
            CurrentRaceName = raceName;
            return true;
        }

        public void DeleteCurrentRace()
        {
            _databaseGeneric.ExecuteNonQuery(DatabaseQueries.DeleteResultsTable(CurrentRaceName));
            Races.Remove(CurrentRaceName);
            CurrentRaceName = "";
        }
        #endregion

        #region Racer Management
        private void CreateRacerTable()
        {
            _databaseGeneric.ExecuteNonQuery(DatabaseQueries.CreateRacerTable(IsSqlite));
        }

        private void GetRacers(string sql, TrulyObservableCollection<Racer> racers)
        {
            _databaseGeneric.ExecuteReader(sql);
            racers.Clear();
            while (_databaseGeneric.Read())
            {
                if (IsSqlite)
                {
                    try
                    {
                        racers.Add(new Racer(Convert.ToInt64(_databaseGeneric.GetReadValue("number")),
                                        (string)_databaseGeneric.GetReadValue("name"),
                                        Convert.ToDecimal(_databaseGeneric.GetReadValue("weight(oz)")),
                                        (string)_databaseGeneric.GetReadValue("troop"),
                                        (string)_databaseGeneric.GetReadValue("level"),
                                        (string)_databaseGeneric.GetReadValue("email"),
                                        ImageHandler.ByteArrayToImage((byte[])_databaseGeneric.GetReadValue("image"))));
                    }
                    catch { }
                }
                else
                {
                    string path = Path.Combine(OutputFolderName, "racer_images");
                    try
                    {
                        string guid = (string)_databaseGeneric.GetReadValue("imagekey");
                        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                        Racer racer = new(Convert.ToInt64(_databaseGeneric.GetReadValue("number")),
                                        (string)_databaseGeneric.GetReadValue("name"),
                                        Convert.ToDecimal(_databaseGeneric.GetReadValue("weight(oz)")),
                                        (string)_databaseGeneric.GetReadValue("troop"),
                                        (string)_databaseGeneric.GetReadValue("level"),
                                        (string)_databaseGeneric.GetReadValue("email"),
                                        null);
                        racers.Add(racer);
                        try { ImageDownloader.SetPhoto(racer, Path.Combine(path, guid + ".png")); }
                        catch { _ = ImageDownloader.DownloadImageAsync((string)_databaseGeneric.GetReadValue("image"), path, guid + ".png", racer); }
                    }
                    catch { }
                }
            }
        }

        private void GetAllRacers()
        {
            GetRacers(DatabaseQueries.GetAllRacers(), Racers);
        }

        private void GetCurrentRacers()
        {
            int order = 1;
            GetRacers(DatabaseQueries.GetSpecificRacers(CurrentRaceName), CurrentRaceRacers);
            foreach (Racer r in CurrentRaceRacers) r.RaceOrder = order++;
        }

        public void AddRacer(Racer racer)
        {
            string imageUrl = "";
            string guid = "";
            MemoryStream ms = new();
            racer.Photo.Save(ms, ImageFormat.Png);
            ms.Position = 0;

            if (!IsSqlite)
            {
                guid = ShortGuid.GenerateShortGuid();
                imageUrl = _googleDriveAccess.UploadFile(guid + ".png", ms);
                imageUrl = imageUrl.Replace("file/d/", "uc?export=download&id=");
                imageUrl = imageUrl.Replace("/view?usp=drivesdk", "");
                string path = Path.Combine(OutputFolderName, "racer_images");
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                racer.Photo.Save(Path.Combine(path, guid + ".png"), ImageFormat.Png);
            }
            string sql = DatabaseQueries.AddRacer(racer, ms, IsSqlite, guid, imageUrl, out List<DatabaseGeneric.SqlParameter> parameters);
            _databaseGeneric.ExecuteNonQueryWithParams(sql, parameters);
            Racers.Remove(Racers.Where(x => x.Number == racer.Number).First());
            Racers.Add(racer);
        }

        public void RemoveRacer(Racer racer)
        {
            string sql = DatabaseQueries.RemoveRacer(racer, out List<DatabaseGeneric.SqlParameter> parameters);
            _databaseGeneric.ExecuteNonQueryWithParams(sql, parameters);
            Racers.Remove(racer);
        }
        #endregion

        public void AddRunOffHeat()
        {
            _databaseGeneric.ExecuteNonQuery(DatabaseQueries.AddRunOffHeat(CurrentRaceName, HeatCount++));
            RaceFormat.AddRunOffHeat([.. CurrentRaceRacers]);
            ResultsTable.Columns.Add("Heat " + RaceFormat.HeatCount, Type.GetType("System.Double"));
            ColumnAdded?.Invoke(this, new PropertyChangedEventArgs("Heat " + RaceFormat.HeatCount));
        }

        private void CreateResultsTable()
        {
            int racerNum = 0;

            ResultsTable.Clear();
            if (ResultsTable.Columns.Count == 0)
            {
                ResultsTable.Columns.Add("Number", Type.GetType("System.Int32"));
                //ResultsTable.PrimaryKey = [ResultsTable.Columns["Number"]];
                ResultsTable.Columns.Add("Name", Type.GetType("System.String"));
                for (int i = 1; i <= RaceFormat.HeatCount; i++)
                {
                    ResultsTable.Columns.Add("Heat " + i, Type.GetType("System.Double"));
                }
            }
            else
            {

            }

            foreach (Racer r in CurrentRaceRacers)
            {
                DataRow row = ResultsTable.NewRow();
                row["Number"] = r.Number;
                row["Name"] = r.RacerName;
                ResultsTable.Rows.Add(row);
                r.RaceOrder = racerNum++;
            }
            while (RaceFormat.HeatCount < HeatCount) AddRunOffHeat();

#warning CLEANUP: See if this works to properly initialize the table when it does and does not exist in the database (check for calls to ModifyResultsTable as those create the table)
            _databaseGeneric.InitResultsTable(CurrentRaceName, ResultsTable);
            /*string sql = DatabaseQueries.LoadResultsTable();
            _databaseGeneric.ExecuteReader(sql);
            DataTable dt = new();
            dt.Load(_databaseGeneric.GetDataReader());
            if (dt.Rows.Count > 0)
            {
                RaceInProgress = true;
                ResultsTable.Clear();
                foreach (DataRow row in dt.Rows) ResultsTable.ImportRow(row);
            }*/
        }

#warning CLEANUP:
        public void ModifyResultsTable(ObservableCollection<Racer> racers, string raceName, int heatCount, int raceFormat)
        {
            int racerCount = 0;
            string sql;

            sql = "DELETE FROM [" + raceName + "]";
            if (_databaseGeneric.ExecuteNonQuery(sql) < 0)
            {
                sql = "CREATE TABLE IF NOT EXISTS [" + raceName + "] ([Number] INTEGER PRIMARY KEY, [RaceFormat] INTEGER";
                /*if (IsSqlite)    // These differ because of how autoincrement is different between postgres and sqlite
                {
                    sql = "CREATE TABLE IF NOT EXISTS [" + raceName + "] ([RacePosition] INTEGER PRIMARY KEY AUTOINCREMENT, [Number] INTEGER, [RaceFormat] INTEGER";
                }
                else
                {
                    sql = "CREATE TABLE IF NOT EXISTS [" + raceName + "] ([RacePosition] INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY, [Number] INTEGER, [RaceFormat] INTEGER";
                }*/

                for (int i = 0; i < heatCount; i++) sql += ", [Heat " + (i + 1) + "] REAL";
                sql += ")";
                _databaseGeneric.ExecuteNonQuery(sql);
                if (!Races.Contains(raceName)) Races.Add(raceName);
            }

            try
            {
                if (IsSqlite)    // These differ because of how reseting the ID key is different between postgres and sqlite
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

        public void UpdateResultsTable(string newString, int column, int row)
        {
            if ((row < ResultsTable.Rows.Count) && (double.TryParse(newString, out _)))
            {
                ResultsTable.Rows[row][column] = newString;
                RaceInProgress = true;
            }
            
#warning CLEANUP: See if this works to properly to update the remote database (and local) ... what are the return values, can I use this instead of "IsSynced"
            //_sqlAdapter.Update(ResultsTable);

            /*foreach (DataRow row in table.Rows)
            {
                string sql = "UPDATE [" + CurrentRaceName + "] SET ";
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

                if (_databaseGeneric.ExecuteNonQuery(sql) < 1)
                {
                    IsSynced = false;
                    break;
                }
                else IsSynced = true;
            }
            if (!IsSynced)
            {
                ResultsTable.TableName = CurrentRaceName;
                ResultsTable.WriteXml(Path.Combine(OutputFolderName, "databaseBackup_" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + ".xml"));
            }*/
        }

    }
}
