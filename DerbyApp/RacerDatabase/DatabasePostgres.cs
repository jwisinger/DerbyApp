using DerbyApp.Helpers;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;

namespace DerbyApp.RacerDatabase
{
    public class DatabasePostgres : DatabaseGeneric
    {
        public readonly string EventName = "";
        public NpgsqlConnection PostgresConn;
        private NpgsqlDataReader _reader;
        private NpgsqlDataAdapter _dataAdapter;
        private readonly Credentials _credentials;
        private static readonly string[] _defaultDatabaseNames = ["postgres", "retool", "template0", "template1"];

        private static NpgsqlDbType GetPostgresType(DataType type)
        {
            return type switch
            {
                DataType.Integer => NpgsqlDbType.Bigint,
                DataType.Real => NpgsqlDbType.Real,
                DataType.Text => NpgsqlDbType.Text,
                DataType.Blob => NpgsqlDbType.Bytea,
                _ => NpgsqlDbType.Text,
            };
        }

        private bool ConnectToRootDatabase(bool showErrorMessage)
        {
            try
            {
                PostgresConn?.Close();
                PostgresConn = new("Host=" + Host + "; Username=" + _credentials.DatabaseUsername + ";Password=" + _credentials.DatabasePassword.ToString() + ";Database=postgres");
                PostgresConn.Open();
            }
            catch (Exception ex)
            {
                if (showErrorMessage) MessageBox.Show(ex.Message, "Database Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        public bool AddNewDatabase(string databaseName)
        {
            try
            {
                string sql = "SELECT EXISTS(SELECT datname FROM pg_catalog.pg_database WHERE lower(datname) = lower('" + databaseName + "'));";
                ExecuteReader(sql);
                _reader.Read();
                if (_reader.GetBoolean(0))
                {
                    MessageBox.Show("An event with that name already exists.", "Event Exists", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                {
                    sql = "CREATE DATABASE " + databaseName.Replace("\"", null);
                    ExecuteNonQuery(sql);
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        private bool ConnectToDatabase()
        {
            if ((PostgresConn == null) || PostgresConn.State == ConnectionState.Closed)
            {
                try
                {
                    PostgresConn = new("Host=" + Host + "; Username=" + _credentials.DatabaseUsername + ";Password=" + _credentials.DatabasePassword + ";Database=" + EventName.ToLower());
                    PostgresConn.Open();
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }

        public DatabasePostgres(string databaseName, Credentials credentials)
        {
            EventName = databaseName;
            _credentials = credentials;

#warning CLEANUP: I'm not sure this try block will ever trigger
            try
            {
                if (ConnectToDatabase())
                {
                    InitGood = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public override string GetConnectionString(bool microsoftFormat)
        {
            if (microsoftFormat) return "Server=" + Host + "; User Id=" + _credentials.DatabaseUsername + ";Password=" + _credentials.DatabasePassword.ToString() + ";Database=" + EventName.ToLower();
            else return "Host=" + Host + "; Username=" + _credentials.DatabaseUsername + ";Password=" + _credentials.DatabasePassword.ToString() + ";Database=" + EventName.ToLower();
        }

        public override bool TestConnection()
        {
            try
            {
                NpgsqlCommand command = new("SELECT 1", PostgresConn)
                {
                    CommandTimeout = 3
                };
                command.ExecuteScalar();
                return true;
            }
            catch
            {
                try
                {
                    PostgresConn.Close();
                    PostgresConn.Open();
                }
                catch
                {
                    return false;
                }
                return false;
            }
        }

        public override int ExecuteNonQuery(string sql)
        {
            if (!ConnectToDatabase()) return -1;
            try
            {
                NpgsqlCommand command = new(sql.Replace('[', '"').Replace(']', '"'), PostgresConn)
                {
                    CommandTimeout = 3
                };
                _reader?.Close();
                return command.ExecuteNonQuery();
            }
            catch
            {
                return -1;
            }
        }

        public override int ExecuteNonQueryWithParams(string sql, List<SqlParameter> parameters)
        {
            if (!ConnectToDatabase()) return -1;
            try
            {
                NpgsqlCommand command = new(sql.Replace('[', '"').Replace(']', '"'), PostgresConn);
                _reader?.Close();
                foreach (SqlParameter param in parameters)
                {
                    if (param.type == DataType.Real) command.Parameters.Add(param.name, GetPostgresType(param.type)).Value = Convert.ToDouble(param.value);
                    else command.Parameters.Add(param.name, GetPostgresType(param.type)).Value = param.value;
                }
                return command.ExecuteNonQuery();
            }
            catch
            {
                return -1;
            }
        }

        public override bool ExecuteReader(string sql)
        {
            if (!ConnectToDatabase()) return false;
            try
            {
                NpgsqlCommand command = new(sql.Replace('[', '"').Replace(']', '"'), PostgresConn);
                _reader?.Close();
                _reader = command.ExecuteReader();
            }
            catch
            {
                return false;
            }
            return true;
        }

        public override bool Read()
        {
            if (_reader == null) return false;
            if (!_reader.IsClosed) return _reader.Read();
            else return false;
        }

        public override object GetReadValue(string name)
        {
            if (_reader == null) return false;
            if (!_reader.IsClosed) return _reader.GetValue(name);
            else return null;
        }

        public override int GetReadFieldCount()
        {
            if (_reader == null) return 0;
            if (!_reader.IsClosed) return _reader.FieldCount;
            else return 0;
        }

        public override string GetReadFieldName(int column)
        {
            if (_reader == null) return "";
            if (!_reader.IsClosed) return _reader.GetName(column);
            else return "";
        }

#warning CLEANUP: Can probably get rid of this eventually
        public override IDataReader GetDataReader()
        {
            return _reader;
        }

        public override string GetDataBaseName()
        {
            return EventName;
        }

        public override void InitResultsTable(string raceName, DataTable table)
        {
            string sql = "SELECT * FROM \"" + raceName + "\"";
            _reader?.Close();
            _dataAdapter = new NpgsqlDataAdapter(sql, PostgresConn);
            //SqlCommandBuilder builder = new(_sqlAdapter);
            _dataAdapter.Fill(table);
        }

        public ObservableCollection<string> GetEventList()
        {
            ObservableCollection<string> retVal = [];
            ConnectToRootDatabase(true);
            string sql = "SELECT datname FROM pg_database;";
            if (ExecuteReader(sql))
            {
                try
                {
                    while (_reader.Read())
                    {
                        string s = (string)_reader.GetValue(0);
                        if (!_defaultDatabaseNames.Any(s.Contains)) retVal.Add(s);
                    }
                }
                catch { }
            }

            return retVal;
        }

    }
}
