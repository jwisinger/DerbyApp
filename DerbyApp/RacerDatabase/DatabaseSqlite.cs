using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows;

namespace DerbyApp.RacerDatabase
{
    internal class DatabaseSqlite : DatabaseGeneric
    {
        public readonly SqliteConnection SqliteConn;
        public readonly string EventFile = "";
        private SqliteDataReader _reader;

        private SqliteConnection CreateConnection()
        {
            try
            {
                SqliteConn.Open();
                InitGood = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return SqliteConn;
        }

        private static SqliteType GetSqliteType(DataType type)
        {
            return type switch
            {
                DataType.Integer => SqliteType.Integer,
                DataType.Real => SqliteType.Real,
                DataType.Text => SqliteType.Text,
                DataType.Blob => SqliteType.Blob,
                _ => SqliteType.Text,
            };
        }

        public DatabaseSqlite(string databaseFile)
        {
            if (File.Exists(databaseFile))
            {
                EventFile = databaseFile;
                SqliteConn = new SqliteConnection("Data Source = " + databaseFile);
                CreateConnection();
            }
        }

        public override bool TestConnection()
        {
            return true;
        }

        public override int ExecuteNonQuery(string sql)
        {
            SqliteCommand command = new(sql, SqliteConn);
            return command.ExecuteNonQuery();
        }

        public override int ExecuteNonQueryWithParams(string sql, List<SqlParameter> parameters)
        {
            SqliteCommand command = new(sql, SqliteConn);
            foreach (SqlParameter param in parameters)
            {

                command.Parameters.Add(param.name, GetSqliteType(param.type)).Value = param.value;
            }
            return command.ExecuteNonQuery();
        }

        public override bool ExecuteReader(string sql)
        {
            SqliteCommand command = new(sql, SqliteConn);
            _reader = command.ExecuteReader();
            return true;
        }

        public override bool Read()
        {
            return _reader.Read();
        }

        public override object GetReadValue(string name)
        {
            return _reader[name];
        }

        public override int GetReadFieldCount()
        {
            return _reader.FieldCount;
        }

        public override string GetReadFieldName(int column)
        {
            return _reader.GetName(column);
        }

        public override IDataReader GetDataReader()
        {
            return _reader;
        }

        public override string GetDataBaseName()
        {
            return EventFile;
        }
    }
}
