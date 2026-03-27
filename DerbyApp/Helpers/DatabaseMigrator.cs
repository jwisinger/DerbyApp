using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using DerbyApp.Windows;
using Microsoft.Data.Sqlite;
using Npgsql;

namespace DerbyApp.Helpers
{

    public enum MigrationDirection { SqliteToPostgres, PostgresToSqlite }

    public class DatabaseMigrator()
    {
        public static async Task Migrate(MigrationDirection direction, string sqlitePath, string pgConnStr)
        {
            string _sqliteConnStr = sqlitePath.StartsWith("Data Source") ? sqlitePath : $"Data Source={sqlitePath}";
            string _pgConnStr = pgConnStr;
            using var sqliteConn = new SqliteConnection(_sqliteConnStr);
            using var pgConn = new NpgsqlConnection(_pgConnStr);

            sqliteConn.Open();
            pgConn.Open();

            bool isToPg = direction == MigrationDirection.SqliteToPostgres;
            DbConnection source = isToPg ? sqliteConn : pgConn;
            DbConnection target = isToPg ? pgConn : sqliteConn;

            var tables = GetTableNames(source, isToPg);

            var pw = new ProgressWindow();
            pw.Show();
            foreach (var table in tables)
            {
                SyncSchema(source, target, table, isToPg);
                await CopyData(source, target, table, isToPg, pw);
            }
            pw.Close();
            sqliteConn.Close();
            pgConn.Close();
        }

        private static List<string> GetTableNames(DbConnection source, bool isSourceSqlite)
        {
            var tables = new List<string>();
            string sql = isSourceSqlite
                ? "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%';"
                : "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' AND table_type = 'BASE TABLE';";

            using var cmd = source.CreateCommand();
            cmd.CommandText = sql;
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) tables.Add(reader.GetString(0));
            return tables;
        }

        private static void SyncSchema(DbConnection source, DbConnection target, string tableName, bool toPg)
        {
            var columns = new List<string>();

            if (toPg) // SQLite -> Postgres
            {
                using var cmd = source.CreateCommand();
                cmd.CommandText = $"PRAGMA table_info(\"{tableName}\");";

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string name = reader.GetString(1);
                    string type = reader.GetString(2).ToUpperInvariant();
                    bool isPk = reader.GetInt32(5) == 1;

                    string pgType = type switch
                    {
                        "INTEGER" when isPk => "SERIAL PRIMARY KEY",
                        "INTEGER" => "BIGINT",
                        "REAL" or "FLOAT" or "DOUBLE" => "DOUBLE PRECISION",
                        "BLOB" => "BYTEA",
                        _ => "TEXT"
                    };

                    columns.Add($"\"{name}\" {pgType}");
                }
            }
            else // Postgres -> SQLite
            {
                using var cmd = source.CreateCommand();
                cmd.CommandText = @"
            SELECT
                c.column_name,
                c.data_type,
                CASE WHEN kcu.column_name IS NOT NULL THEN 1 ELSE 0 END AS is_primary_key
            FROM information_schema.columns c
            LEFT JOIN (
                SELECT kcu.table_schema, kcu.table_name, kcu.column_name
                FROM information_schema.table_constraints tc
                INNER JOIN information_schema.key_column_usage kcu
                    ON tc.constraint_name = kcu.constraint_name
                   AND tc.table_schema = kcu.table_schema
                   AND tc.table_name = kcu.table_name
                WHERE tc.constraint_type = 'PRIMARY KEY'
            ) kcu
                ON c.table_schema = kcu.table_schema
               AND c.table_name = kcu.table_name
               AND c.column_name = kcu.column_name
            WHERE c.table_name = @t
              AND c.table_schema = 'public'
            ORDER BY c.ordinal_position;";

                var p = cmd.CreateParameter();
                p.ParameterName = "@t";
                p.Value = tableName;
                cmd.Parameters.Add(p);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string name = reader.GetString(0);
                    string pgType = reader.GetString(1).ToLowerInvariant();
                    bool isPk = reader.GetInt32(2) == 1;

                    string liteType = pgType switch
                    {
                        "integer" or "bigint" or "smallint" => "INTEGER",
                        "real" or "double precision" or "numeric" => "REAL",
                        "bytea" => "BLOB",
                        "boolean" => "INTEGER",
                        _ => "TEXT"
                    };

                    if (isPk)
                    {
                        columns.Add($"\"{name}\" INTEGER PRIMARY KEY");
                    }
                    else if (name == "Image")
                    {
                        columns.Add($"\"{name}\" MEDIUMBLOB");
                    }
                    else
                    {
                        columns.Add($"\"{name}\" {liteType}");
                    }
                }
            }

            using var createCmd = target.CreateCommand();
            createCmd.CommandText = $"CREATE TABLE IF NOT EXISTS \"{tableName}\" ({string.Join(", ", columns)});";
            createCmd.ExecuteNonQuery();
        }

        private static async Task CopyData(DbConnection source, DbConnection target, string tableName, bool toPg, ProgressWindow pw)
        {
            using var selectCmd = source.CreateCommand();
            selectCmd.CommandText = $"SELECT COUNT(*) FROM \"{tableName}\"";
            int totalCount = Convert.ToInt32(selectCmd.ExecuteScalar());
            int count = 0;
            selectCmd.CommandText = $"SELECT * FROM \"{tableName}\"";
            using var reader = selectCmd.ExecuteReader();

            // Start transaction on target (Crucial for SQLite performance)
            using var transaction = target.BeginTransaction();

            while (reader.Read())
            {
                pw.ProgressValue = 100.0 * (count++ / (double)totalCount);
                using var insertCmd = target.CreateCommand();
                insertCmd.Transaction = transaction;

                var cols = new List<string>();
                var pars = new List<string>();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    string name = reader.GetName(i);
                    cols.Add($"\"{name}\"");
                    string paramName = $"@p{i}";
                    pars.Add(paramName);

                    var p = insertCmd.CreateParameter();
                    p.ParameterName = paramName;

                    object val = reader.GetValue(i);
                    // Type translation for logic differences
                    if (!toPg && val is bool b) val = b ? 1 : 0; // Pg Bool to SQLite Int
                    if (!toPg && val is Guid g) val = g.ToString(); // Pg UUID to SQLite Text

                    if (name == "Image")
                    {
                        if (val is string s)
                        {
                            p.Value = await ImageDownloader.DownloadImageAsync(s);
                        }
                    }
                    else
                    {
                        p.Value = val ?? DBNull.Value;
                    }
                    insertCmd.Parameters.Add(p);
                }

                insertCmd.CommandText = $"INSERT INTO \"{tableName}\" ({string.Join(",", cols)}) VALUES ({string.Join(",", pars)})";
                try
                {
                    insertCmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    ErrorLogger.LogError("DatabaseMigrator.CopyData", ex);
                }
            }
            transaction.Commit();
        }
    }
}