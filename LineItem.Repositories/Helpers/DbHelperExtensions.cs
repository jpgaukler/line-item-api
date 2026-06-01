using System;
using System.Collections.Generic;
using System.Data.Common;

namespace LineItem.Repositories.Helpers;

public static class DbDataReaderExtensions
{
    extension(DbDataReader reader)
    {
        public bool IsNull(string columnName)
        {
            return reader.IsDBNull(reader.GetOrdinal(columnName));
        }

        public long Long(string columnName)
        {
            return reader.GetInt64(reader.GetOrdinal(columnName));
        }

        public string String(string columnName)
        {
            return reader.GetString(reader.GetOrdinal(columnName));
        }

        public int Int(string columnName)
        {
            return reader.GetInt32(reader.GetOrdinal(columnName));
        }

        public bool Bool(string columnName)
        {
            return reader.GetBoolean(reader.GetOrdinal(columnName));
        }

        public DateTime DateTime(string columnName)
        {
            return reader.GetDateTime(reader.GetOrdinal(columnName));
        }

        public Guid Guid(string columnName)
        {
            return reader.GetGuid(reader.GetOrdinal(columnName));
        }
    }
}

public static class DbCommandExtensions
{
    extension(DbCommand command)
    {
        public void AddParameters(IEnumerable<KeyValuePair<string, object>> parameters)
        {
            foreach (var param in parameters)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = param.Key.StartsWith("@") ? param.Key : $"@{param.Key}";
                parameter.Value = param.Value ?? DBNull.Value;
                command.Parameters.Add(parameter);
            }
        }
    }
}