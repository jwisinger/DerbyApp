using DerbyApp.Helpers;
using DerbyApp.RaceStats;
using System.Collections.Generic;
using System.IO;

namespace DerbyApp.RacerDatabase
{
    public static class DatabaseQueries
    {
        public const string RacerTableName = "raceTable";
        public const string VideoTableName = "videoTable";
        public const string SettingsTableName = "settingsTable";

        #region Support Table Queries
        public static string CreateVideoTable(bool isSqlite)
        {
            if (isSqlite)    // These differ because of how autoincrement is different between postgres and sqlite
            {
                return "CREATE TABLE IF NOT EXISTS [" + VideoTableName + "] ([Number] INTEGER PRIMARY KEY AUTOINCREMENT, [RaceName] VARCHAR(50), [HeatNumber] INTEGER, [Url] VARCHAR(200), UNIQUE ([RaceName], [HeatNumber]))";
            }
            else
            {
                return "CREATE TABLE IF NOT EXISTS [" + VideoTableName + "] ([Number] INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY, [RaceName] VARCHAR(50), [HeatNumber] INTEGER, [Url] VARCHAR(200), UNIQUE ([RaceName], [HeatNumber]))";
            }
        }

        public static string CreateRacerTable(bool isSqlite)
        {
            if (isSqlite)    // These differ because of how autoincrement is different between postgres and sqlite
            {
                return "CREATE TABLE IF NOT EXISTS [" + RacerTableName + "] ([Number] INTEGER PRIMARY KEY AUTOINCREMENT, [Name] VARCHAR(50), [Weight(oz)] DECIMAL(5, 3), [Troop] VARCHAR(10), [Level] VARCHAR(20), [Email] VARCHAR(100), [Image] MEDIUMBLOB, [ImageKey] VARCHAR(50))";
            }
            else
            {
                return "CREATE TABLE IF NOT EXISTS [" + RacerTableName + "] ([Number] INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY, [Name] VARCHAR(50), [Weight(oz)] DECIMAL(5, 3), [Troop] VARCHAR(10), [Level] VARCHAR(20), [Email] VARCHAR(100), [Image] VARCHAR(150), [ImageKey] VARCHAR(50))";
            }
        }

        public static string CreateSettingsTable()
        {
            return "CREATE TABLE IF NOT EXISTS [" + SettingsTableName + "] ([Number] INTEGER PRIMARY KEY, [Name] VARCHAR(500))";
        }

        public static string StoreRaceSettings(string eventName, out List<DatabaseGeneric.SqlParameter> parameters)
        {
            parameters =
            [
                new DatabaseGeneric.SqlParameter { name = "@Number", type = DatabaseGeneric.DataType.Integer, value = 1 },
                new DatabaseGeneric.SqlParameter { name = "@Name", type = DatabaseGeneric.DataType.Text, value = eventName },
            ];
            return "REPLACE INTO [" + SettingsTableName + "] ([Number], [Name]) VALUES (@Number, @Name)";
        }

        public static string LoadRaceSettings()
        {
            return "SELECT * FROM [" + SettingsTableName + "] ORDER BY [Number] ASC LIMIT 1";
        }

        public static string AddVideoToTable(VideoUploadedEventArgs e, out List<DatabaseGeneric.SqlParameter> parameters)
        {
            parameters =
            [
                new DatabaseGeneric.SqlParameter { name = "@url", type = DatabaseGeneric.DataType.Text, value = e.Url },
                new DatabaseGeneric.SqlParameter { name = "@heatnumber", type = DatabaseGeneric.DataType.Integer, value = e.HeatNumber },
                new DatabaseGeneric.SqlParameter { name = "@racename", type = DatabaseGeneric.DataType.Text, value = e.RaceName },
            ];
            return "INSERT INTO [" + VideoTableName + "] ([Url], [HeatNumber], [RaceName]) VALUES (@url, @heatnumber, @racename) ON CONFLICT ([HeatNumber], [RaceName]) DO UPDATE SET [Url] = EXCLUDED.[Url]";
        }
        #endregion

        # region Database Queries
        public static string GetListOfRaces(bool isSqlite, out string tableName)
        {
            if (isSqlite)    // These differ because of how tables are stored in postgres vs sqlite
            {
                tableName = "name";
                return "SELECT name FROM sqlite_schema WHERE type ='table' AND (name NOT LIKE 'sqlite_%') AND (name NOT LIKE 'settings%');";
            }
            else
            {
                tableName = "table_name";
                return "SELECT table_name FROM information_schema.tables WHERE table_schema NOT IN('pg_catalog', 'information_schema') AND table_type = 'BASE TABLE'";
            }
        }
        #endregion

        #region Results Table Queries
        public static string AddRunOffHeat(string raceName, int heatCount)
        {
            return "ALTER TABLE [" + raceName + "] ADD [Heat " + heatCount + "] DOUBLE";
        }

        public static string CreateResultsTable(string raceName)
        {
            return "CREATE TABLE IF NOT EXISTS [" + raceName + "] ([Number] INTEGER PRIMARY KEY, [RaceFormat] INTEGER)";
        }

        public static string DeleteResultsTable(string raceName)
        {
            return "DROP TABLE [" + raceName + "]";
        }

        public static string GetRaceFormat(string currentRaceName)
        {
            return "SELECT * FROM [" + currentRaceName + "] LIMIT 1";
        }

#warning CLEANUP: This may never get used
        public static string LoadResultsTable(string currentRaceName)
        {
            return "SELECT *  FROM [" + RacerTableName + "] INNER JOIN [" + currentRaceName + "] ON [" + currentRaceName + "].[Number] = [" + RacerTableName + "].[Number]";
        }
        #endregion

        #region Racer Table Queries
        public static string GetAllRacers()
        {
            return "SELECT * FROM [" + RacerTableName + "]";
        }

        public static string GetSpecificRacers(string raceName)
        {
            return "SELECT * FROM [" + RacerTableName + "] INNER JOIN [" + raceName + "] ON [" + raceName + "].[Number] = [" + RacerTableName + "].[Number] ORDER BY [" + raceName + "].[Number]";
        }

        public static string AddRacer(Racer racer, MemoryStream ms, bool isSqlite, string guid, string imageUrl, out List<DatabaseGeneric.SqlParameter> parameters)
        {
            string sql;

            if (racer.Number == 0)
            {
                sql = "INSERT INTO [" + RacerTableName + "] ([Name], [Weight(oz)], [Troop], [Level], [Email], [Image], [ImageKey]) VALUES (@Name, @Weight, @Troop, @Level, @Email, @Image, @ImageKey)";
            }
            else
            {
                sql = "UPDATE [" + RacerTableName + "] SET [Name] = @Name, [Weight(oz)] = @Weight, [Troop] =  @Troop, [Level] = @Level, [Email] = @Email, [Image] = @Image, [ImageKey] = @ImageKey WHERE [Number] = @Number";
            }

            parameters =
            [
                new DatabaseGeneric.SqlParameter { name = "@Number", type = DatabaseGeneric.DataType.Integer, value = racer.Number },
                new DatabaseGeneric.SqlParameter { name = "@Name", type = DatabaseGeneric.DataType.Text, value = racer.RacerName },
                new DatabaseGeneric.SqlParameter { name = "@Weight", type = DatabaseGeneric.DataType.Real, value = racer.Weight },
                new DatabaseGeneric.SqlParameter { name = "@Troop", type = DatabaseGeneric.DataType.Text, value = racer.Troop },
                new DatabaseGeneric.SqlParameter { name = "@Level", type = DatabaseGeneric.DataType.Text, value = racer.Level },
                new DatabaseGeneric.SqlParameter { name = "@Email", type = DatabaseGeneric.DataType.Text, value = racer.Email },
            ];

            if (isSqlite)
            {
                parameters.Add(new DatabaseGeneric.SqlParameter { name = "@Image", type = DatabaseGeneric.DataType.Blob, value = ms.ToArray() });
            }
            else
            {
                parameters.Add(new DatabaseGeneric.SqlParameter { name = "@Image", type = DatabaseGeneric.DataType.Text, value = imageUrl });
                parameters.Add(new DatabaseGeneric.SqlParameter { name = "@ImageKey", type = DatabaseGeneric.DataType.Text, value = guid });
            }

            return sql;
        }

        public static string RemoveRacer(Racer racer, out List<DatabaseGeneric.SqlParameter> parameters)
        {
            parameters =
            [
                new DatabaseGeneric.SqlParameter { name = "@Number", type = DatabaseGeneric.DataType.Integer, value = racer.Number },
            ];
            return "DELETE FROM [" + RacerTableName + "] WHERE [Number]=@Number";
        }
        #endregion
    }
}
