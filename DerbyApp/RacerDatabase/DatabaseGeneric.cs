using System.Collections.Generic;
using System.Data;

namespace DerbyApp.RacerDatabase
{
    public class DatabaseGeneric
    {
        public enum DataType
        {
            Integer = 0,
            Real = 1,
            Text = 2,
            Blob = 3
        }

        public struct SqlParameter
        {
            public string name;
            public DataType type;
            public object value;
        }

        private string _host = "ep-green-tree-afr4vi1v.c-2.us-west-2.retooldb.com";
        public bool InitGood = false;

        public string Host
        {
            get => _host;
            set
            {
                _host = value;
            }
        }

        public virtual string GetConnectionString()
        {
            return "";
        }

        public virtual bool TestConnection()
        {
            return false;
        }

        public virtual int ExecuteNonQuery(string sql)
        {
            return -1;
        }

        public virtual int ExecuteNonQueryWithParams(string sql, List<SqlParameter> parameters)
        {
            return -1;
        }

        public virtual void ExecuteReader(string sql)
        {
        }

        public virtual bool Read()
        {
            return false;
        }

        public virtual object GetReadValue(string name)
        {
            return null;
        }

        public virtual int GetReadFieldCount()
        {
            return 0;
        }

        public virtual string GetReadFieldName(int column)
        {
            return "";
        }

        public virtual IDataReader GetDataReader()
        {
            return null;
        }

        public virtual string GetDataBaseName()
        {
            return "";
        }
    }
}
