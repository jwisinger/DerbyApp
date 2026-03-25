#warning 2-FAILSAFE: Try writing to SQLite instead of XML
#warning RUNOFF: If database connection is lost when addrunoff, the column lets you fill it out on the screen, but it never gets added to the database
#warning RUNOFF: When a runoff heat is added, entering info into it does not get stored in the databse. Closing and re-opening fixes this ... the issue is likely related to the data adapter adding columns

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DerbyApp.Helpers;
using DerbyApp.RaceStats;

namespace DerbyApp.RacerDatabase
{
    public class Database : INotifyPropertyChanged
    {
        private readonly DatabaseGeneric _databaseGeneric;
        private readonly GoogleDriveAccess _googleDriveAccess;
        private readonly Timer _updateTimer;
        private RaceFormat _raceFormat;
        private readonly string _databaseName = "";
        private string _currentRaceName = "";
        private string _eventName = "";
        private string _licensePrinter = "";
        private string _qrPrinter = "";
        private string _qrCodeLink = "";
        private string _outputFolderName;
        private bool _isSynced = false;
        private int _currentHeatNumber = 1;

        public ObservableCollection<GirlScoutLevel> GirlScoutLevels = new GirlScoutLevels().ScoutLevels;
        public TrulyObservableCollection<Racer> Racers = [];
        public TrulyObservableCollection<Racer> CurrentRaceRacers = [];
        public DataTable ResultsTable = new();
        public readonly bool IsSqlite;
        public bool InitGood = false;
        public bool RaceInProgress = false;

        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangedEventHandler ColumnAdded;
        public event PropertyChangedEventHandler ColumnRemoved;
        public event EventHandler SyncStatusChanged;
        #endregion

        #region Public Properties
        public ObservableCollection<string> Races { get; set; } = [];
        public Leaderboard LdrBoard { get; set; }
        public bool IsSynced { get => _isSynced; }
        public RaceFormat RaceFormat
        {
            get => _raceFormat;
            set
            {
                _raceFormat = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RaceFormat)));
            }
        }

        public string CurrentRaceName
        {
            get => _currentRaceName;
            set
            {
                _currentRaceName = value;
                DatabaseRegistry.StoreDatabaseRegistry(null, _currentRaceName, null, null, null, null, null, null, null);
                if ((_currentRaceName != null) && (_currentRaceName != ""))
                {
                    LoadRaceInfo();
                    GetCurrentRacers();
                    CreateResultsTable();
                }
                else
                {
                    CurrentRaceRacers.Clear();
                    ResultsTable.Clear();
                    RaceInProgress = false;
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentRaceName)));
            }
        }

        public string EventName
        {
            get => _eventName;
            set
            {
                _eventName = value;
                string sql = DatabaseQueries.UpdateEventName(value, out List<DatabaseGeneric.SqlParameter> parameters);
                _databaseGeneric.ExecuteNonQueryWithParams(sql, parameters);
            }
        }

        public string RacerImageFolderName
        {
            get => Path.Combine(EventFolderName, "racer_images");
        }

        public string LicenseFolderName
        {
            get => Path.Combine(EventFolderName, "licenses");
        }

        public string VideoFolderName
        {
            get => Path.Combine(EventFolderName, "videos");
        }

        public string EventFolderName
        {
            get => Path.Combine(_outputFolderName, Path.GetFileNameWithoutExtension(_databaseName));
        }

        public string OutputFolderName
        {
            get => _outputFolderName;
            set
            {
                _outputFolderName = value;
                ErrorLogger.LogFilePath = Path.Combine(EventFolderName, "errorlog.log");
                ErrorLogger.EventFilePath = Path.Combine(EventFolderName, "eventlog.log");
                DatabaseRegistry.StoreDatabaseRegistry(null, null, value, null, null, null, null, null, null);
            }
        }

        public bool TimeBasedScoring
        {
            get => LdrBoard.TimeBasedScoring;
            set
            {
                LdrBoard.TimeBasedScoring = value;
                LdrBoard.CalculateResults(ResultsTable);
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
        public Database(string databaseFile, Credentials credentials, GoogleDriveAccess gda, string outputFolderName, bool timeBasedScoring)
        {
            OutputFolderName = outputFolderName;
            _databaseName = databaseFile;
            if (databaseFile.Contains(':')) IsSqlite = true;
            else IsSqlite = false;

            if (IsSqlite) _databaseGeneric = new DatabaseSqlite(databaseFile);
            else
            {
                _googleDriveAccess = gda;
                _databaseGeneric = new DatabasePostgres(databaseFile, credentials);
            }

            if (_databaseGeneric.InitGood)
            {
                RaceFormat = RaceFormats.Formats[RaceFormats.DefaultFormat].Clone();
                CreateRacerTable();
                CreateRaceTable();
                CreateVideoTable();
                CreateEventNameTable();
                RefreshDatabase();
                LoadRaceInfo();
                GetEventName();
                LdrBoard = new Leaderboard(CurrentRaceRacers, RaceFormat.LaneCount, timeBasedScoring);

                _updateTimer = new Timer((e) =>
                {
                    if (ResultsTable != null)
                    {
                        if (_databaseGeneric.UpdateResultsTable(ResultsTable, CurrentRaceName, RaceFormat.HeatCount) >= 0) _isSynced = true;
                        else _isSynced = false;
                        SyncStatusChanged?.Invoke(this, null);
                    }
                }, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
                InitGood = true;
            }
        }

        public void CheckSyncStatus()
        {
            if (ResultsTable != null)
            {
                if (_databaseGeneric.UpdateResultsTable(ResultsTable, CurrentRaceName, RaceFormat.HeatCount) >= 0) _isSynced = true;
                else _isSynced = false;
                SyncStatusChanged?.Invoke(this, null);
                ResultsTable.TableName = _databaseName;
                if (!IsSynced)
                {
                    if (!Directory.Exists(EventFolderName)) Directory.CreateDirectory(EventFolderName);
                    ResultsTable.WriteXml(Path.Combine(EventFolderName, "unsynced_results_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".xml"));
                }
            }
        }

        public void Close()
        {
            _updateTimer.Dispose();
            CheckSyncStatus();
            _databaseGeneric.Close();
        }
        #endregion

        #region Database Management
        public void RefreshDatabase()
        {
            GetAllRacers();
            GetListOfRaces();
        }

        public async Task CopyDatabaseToLocal()
        {
            if (!IsSqlite)
            {
                string postGresConnStr = _databaseGeneric.GetConnectionString(false);

                if (postGresConnStr != "")
                {
                    await DatabaseMigrator.Migrate(MigrationDirection.PostgresToSqlite, Path.Combine(EventFolderName, _databaseName + "_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".sqlite"), postGresConnStr);
                }
            }
        }
        #endregion

        #region Video Management
        private void CreateVideoTable()
        {
            _databaseGeneric.ExecuteNonQuery(DatabaseQueries.CreateVideoTable(IsSqlite));
        }

        private void CreateEventNameTable()
        {
            _databaseGeneric.ExecuteNonQuery(DatabaseQueries.CreateEventNameTable(IsSqlite));
        }

        public void AddVideoToTable(VideoUploadedEventArgs e)
        {
            string sql = DatabaseQueries.AddVideoToTable(e, out List<DatabaseGeneric.SqlParameter> parameters);
            _databaseGeneric.ExecuteNonQueryWithParams(sql, parameters);
        }
        #endregion

        #region Race Management
        public void GetEventName()
        {
            string sql = DatabaseQueries.GetEventName();
            _databaseGeneric.ExecuteReader(sql);
            if (_databaseGeneric.Read())
            {
                _eventName = (string)_databaseGeneric.GetReadValue("EventName");
                while (_databaseGeneric.Read()) ;
            }
            else
            {
                _eventName = _databaseName;
            }
        }

        public void LoadRaceInfo()
        {
            string sql = DatabaseQueries.LoadRaceInfo(CurrentRaceName, out List<DatabaseGeneric.SqlParameter> parameters);
            _databaseGeneric.ExecuteReaderWithParams(sql, parameters);
            if (_databaseGeneric.Read())
            {
                RaceFormat = RaceFormats.Formats.FirstOrDefault(x => x.Name == (string)_databaseGeneric.GetReadValue("Format"));
                RaceFormat ??= RaceFormats.Formats[RaceFormats.DefaultFormat].Clone();
                while (_databaseGeneric.Read()) ;
            }
            else
            {
                RaceFormat = RaceFormats.Formats[RaceFormats.DefaultFormat].Clone();
            }
        }

        private void GetListOfRaces()
        {
            string current = CurrentRaceName;
            string sql = DatabaseQueries.GetListOfRaces(IsSqlite, out string tableName);
            _databaseGeneric.ExecuteReader(sql);
            Races.Clear();
            while (_databaseGeneric.Read()) Races.Add(Convert.ToString(_databaseGeneric.GetReadValue(tableName)));
            Races.Remove(DatabaseQueries.RacerTableName);
            Races.Remove(DatabaseQueries.RaceTableName);
            Races.Remove(DatabaseQueries.VideoTableName);
            Races.Remove(DatabaseQueries.EventNameTableName);
            CurrentRaceName = current;
        }

        private void CreateRaceTable()
        {
            _databaseGeneric.ExecuteNonQuery(DatabaseQueries.CreateRaceTable(IsSqlite));
        }

        public bool AddRace(string raceName, int raceFormatIndex)
        {
            if (Races.Contains(raceName)) return false;
            Races.Add(raceName);
            string sql = DatabaseQueries.AddOrUpdateRace(raceName, RaceFormats.Formats[raceFormatIndex], out List<DatabaseGeneric.SqlParameter> parameters);
            _databaseGeneric.ExecuteNonQueryWithParams(sql, parameters);
            _databaseGeneric.ExecuteNonQuery(DatabaseQueries.CreateResultsTable(raceName, RaceFormats.Formats[raceFormatIndex].HeatCount));
            CurrentRaceName = raceName;
            return true;
        }

        public void DeleteCurrentRace()
        {
            _databaseGeneric.ExecuteNonQuery(DatabaseQueries.DeleteResultsTable(CurrentRaceName));
            string sql = DatabaseQueries.DeleteRace(CurrentRaceName, out List<DatabaseGeneric.SqlParameter> parameters);
            _databaseGeneric.ExecuteNonQueryWithParams(sql, parameters);
            Races.Remove(CurrentRaceName);
            CurrentRaceName = "";
        }

        public void AddRacerToCurrentRace(Racer r)
        {
            CurrentRaceRacers.Add(r);
            int order = 1;
            foreach (Racer racer in CurrentRaceRacers) racer.RaceOrder = order++;
            ResultsTable.Rows.Add([r.Number, r.RacerName]);
        }

        public void DeleteRacerFromCurrentRace(Racer r)
        {
            CurrentRaceRacers.Remove(r);
            int order = 1;
            foreach (Racer racer in CurrentRaceRacers) racer.RaceOrder = order++;
            ResultsTable.Select("Number = " + r.Number).First().Delete();
        }
        #endregion

        #region Racer Management
        private void CreateRacerTable()
        {
            _databaseGeneric.ExecuteNonQuery(DatabaseQueries.CreateRacerTable(IsSqlite));
        }

        private void GetAllRacers()
        {
            string sql = DatabaseQueries.GetAllRacers();
            _databaseGeneric.ExecuteReader(sql);
            Racers.Clear();
            while (_databaseGeneric.Read())
            {
                if (IsSqlite)
                {
                    try
                    {
                        string name = "";
                        if (_databaseGeneric.GetReadValue("name") != DBNull.Value) name = (string)_databaseGeneric.GetReadValue("name");
                        Racers.Add(new Racer(Convert.ToInt64(_databaseGeneric.GetReadValue("number")),
                                        name,
                                        Convert.ToDecimal(_databaseGeneric.GetReadValue("weight(oz)")),
                                        (string)_databaseGeneric.GetReadValue("troop"),
                                        (string)_databaseGeneric.GetReadValue("level"),
                                        (string)_databaseGeneric.GetReadValue("email"),
                                        ImageHandler.ByteArrayToImage((byte[])_databaseGeneric.GetReadValue("image"))));
                    }
                    catch (Exception ex)
                    {
                        ErrorLogger.LogError("Database.GetRacers", ex);
                    }
                }
                else
                {
                    try
                    {
                        string guid = (string)_databaseGeneric.GetReadValue("imagekey");
                        if (!Directory.Exists(RacerImageFolderName)) Directory.CreateDirectory(RacerImageFolderName);
                        Racer racer = new(Convert.ToInt64(_databaseGeneric.GetReadValue("number")),
                                        (string)_databaseGeneric.GetReadValue("name"),
                                        Convert.ToDecimal(_databaseGeneric.GetReadValue("weight(oz)")),
                                        (string)_databaseGeneric.GetReadValue("troop"),
                                        (string)_databaseGeneric.GetReadValue("level"),
                                        (string)_databaseGeneric.GetReadValue("email"),
                                        null);
                        Racers.Add(racer);
                        try { ImageDownloader.SetPhoto(racer, Path.Combine(RacerImageFolderName, guid + ".png")); }
                        catch { _ = ImageDownloader.DownloadAndSetImageAsync((string)_databaseGeneric.GetReadValue("image"), RacerImageFolderName, guid + ".png", racer); }
                    }
                    catch (Exception ex)
                    {
                        ErrorLogger.LogError("Database.GetRacers", ex);
                    }
                }
            }
        }

        public List<Racer> GetFilteredRacers()
        {
            List<Racer> filteredRacers = [];
            foreach (var item in GirlScoutLevels)
            {
                if (item.IsSelected)
                {
                    var racers = Racers.Where(x => x.Level == item.Level);
                    foreach (Racer r in racers)
                    {
                        if (!CurrentRaceRacers.Where(x => x.Number == r.Number).Any()) filteredRacers.Add(r);
                    }
                }
            }
            return filteredRacers;
        }

        private void GetCurrentRacers()
        {
            int order = 1;
            string sql = DatabaseQueries.GetSpecificRacers(CurrentRaceName);
            _databaseGeneric.ExecuteReader(sql);
            CurrentRaceRacers.Clear();
            while (_databaseGeneric.Read())
            {
                try
                {
                    Int64 number = -1;
                    if (_databaseGeneric.GetReadValue("number") != DBNull.Value)
                    {
                        number = Convert.ToInt64(_databaseGeneric.GetReadValue("number"));
                        List<Racer> result = [.. Racers.Where(x => x.Number == number)];
                        if (result.Count > 0) CurrentRaceRacers.Add(result[0]);
                    }
                }
                catch (Exception ex)
                {
                    ErrorLogger.LogError("Database.GetRacers", ex);
                }
            }
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
                if (!Directory.Exists(RacerImageFolderName)) Directory.CreateDirectory(RacerImageFolderName);
                racer.Photo.Save(Path.Combine(RacerImageFolderName, guid + ".png"), ImageFormat.Png);
            }
            string sql = DatabaseQueries.AddRacer(racer, ms, IsSqlite, guid, imageUrl, out List<DatabaseGeneric.SqlParameter> parameters);
            _databaseGeneric.ExecuteReaderWithParams(sql, parameters);
            if (_databaseGeneric.Read())
            {
                object o = _databaseGeneric.GetReadValue("Number");
                if (o is int i) racer.Number = i;
                else if (o is long l) racer.Number = l;
                while (_databaseGeneric.Read()) ;
            }
            if (!Racers.Contains(racer)) Racers.Add(racer);
        }

        public void RemoveRacer(Racer racer)
        {
            string sql = DatabaseQueries.RemoveRacer(racer, out List<DatabaseGeneric.SqlParameter> parameters);
            _databaseGeneric.ExecuteNonQueryWithParams(sql, parameters);
            Racers.Remove(racer);
        }
        #endregion

        #region Results Table Management
        public void AddRunOffHeat()
        {
            RaceFormat.AddRunOffHeat([.. CurrentRaceRacers]);
            _databaseGeneric.ExecuteNonQuery(DatabaseQueries.AddRunOffHeat(CurrentRaceName, RaceFormat.HeatCount));
            ResultsTable.Columns.Add("Heat " + RaceFormat.HeatCount, Type.GetType("System.Double"));
            ColumnAdded?.Invoke(this, new PropertyChangedEventArgs("Heat " + RaceFormat.HeatCount));
        }

        private void CreateResultsTable()
        {
            int order = 0;
            bool columnsExist = ResultsTable.Columns.Count > 0;
            ResultsTable.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToList().ForEach(x => ColumnRemoved?.Invoke(this, new PropertyChangedEventArgs(x)));
            _databaseGeneric.InitResultsTable(CurrentRaceName, ResultsTable);
            if (columnsExist) ResultsTable.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToList().ForEach(x => ColumnAdded?.Invoke(this, new PropertyChangedEventArgs(x)));
            int heats = ResultsTable.Columns.Cast<DataColumn>().Select(x => x.ColumnName.Contains("Heat")).Count(x => x);
            foreach (Racer racer in CurrentRaceRacers) racer.RaceOrder = order++;
            while (RaceFormat.HeatCount < heats) RaceFormat.AddRunOffHeat(null);
            foreach (DataRow r in ResultsTable.Rows)
            {
                long number = Convert.ToInt64(r["Number"]);
                Racer racer = CurrentRaceRacers.Where(x => x.Number == number).FirstOrDefault();
                if (racer != null) r["Name"] = racer.RacerName;
            }
            LdrBoard.CalculateResults(ResultsTable);
        }

        public void UpdateResultsTable(string newString, int column, int row)
        {
            if ((row < ResultsTable.Rows.Count) && (double.TryParse(newString, out _)))
            {
                ResultsTable.Rows[row][column] = newString;
                RaceInProgress = true;
            }
            LdrBoard.CalculateResults(ResultsTable);
        }
        #endregion
    }
}
