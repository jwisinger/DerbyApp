using Microsoft.Data.Sqlite;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace DerbyApp.Helpers
{

    public enum MigrationDirection { SqliteToPostgres, PostgresToSqlite }

    public class DatabaseMigrator()
    {
        public static void Migrate(MigrationDirection direction, string sqlitePath, string pgConnStr)
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

            foreach (var table in tables)
            {
                SyncSchema(source, target, table, isToPg);
                CopyData(source, target, table, isToPg);
            }
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
                    string type = reader.GetString(2).ToUpper();
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
                cmd.CommandText = "SELECT column_name, data_type FROM information_schema.columns WHERE table_name = @t AND table_schema = 'public';";
                var p = cmd.CreateParameter(); p.ParameterName = "@t"; p.Value = tableName; cmd.Parameters.Add(p);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string name = reader.GetString(0);
                    string pgType = reader.GetString(1).ToLower();
                    string liteType = pgType switch
                    {
                        "integer" or "bigint" or "smallint" => "INTEGER",
                        "real" or "double precision" or "numeric" => "REAL",
                        "bytea" => "BLOB",
                        "boolean" => "INTEGER",
                        _ => "TEXT"
                    };
                    columns.Add($"\"{name}\" {liteType}");
                }
            }

            using var createCmd = target.CreateCommand();
            createCmd.CommandText = $"CREATE TABLE IF NOT EXISTS \"{tableName}\" ({string.Join(", ", columns)});";
            createCmd.ExecuteNonQuery();
        }

        private static void CopyData(DbConnection source, DbConnection target, string tableName, bool toPg)
        {
            using var selectCmd = source.CreateCommand();
            selectCmd.CommandText = $"SELECT * FROM \"{tableName}\"";
            using var reader = selectCmd.ExecuteReader();

            // Start transaction on target (Crucial for SQLite performance)
            using var transaction = target.BeginTransaction();

            while (reader.Read())
            {
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

                    p.Value = val ?? DBNull.Value;
                    insertCmd.Parameters.Add(p);
                }

                insertCmd.CommandText = $"INSERT INTO \"{tableName}\" ({string.Join(",", cols)}) VALUES ({string.Join(",", pars)})";
                insertCmd.ExecuteNonQuery();
            }
            transaction.Commit();
        }
    }
}