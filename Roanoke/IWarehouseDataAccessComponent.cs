using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Roanoke
{
    public interface IWarehouseDataAccessComponent : IDisposable
    {
        void ExecuteNonQuery(CommandType commandType, string commandText, IEnumerable<SqlParameter> sqlParameters);
        void SetProperty(string scope, string key, string value);
        string GetProperty(string scope, string key);
    }
}