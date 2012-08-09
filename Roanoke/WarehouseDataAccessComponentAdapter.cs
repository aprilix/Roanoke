using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Microsoft.TeamFoundation.Warehouse;

namespace Roanoke
{
    public class WarehouseDataAccessComponentAdapter : IWarehouseDataAccessComponent
    {
        private readonly WarehouseDataAccessComponent _dac;

        public WarehouseDataAccessComponentAdapter(WarehouseDataAccessComponent dac)
        {
            _dac = dac;
        }

        public void ExecuteNonQuery(CommandType commandType, string sqlStatement, IEnumerable<SqlParameter> parameters)
        {
            _dac.ExecuteNonQuery(commandType, sqlStatement, parameters);
        }

        public void SetProperty(string scope, string key, string value)
        {
            _dac.SetProperty(scope, key, value);
        }

        public string GetProperty(string scope, string key)
        {
            return _dac.GetProperty(scope, key);
        }

        public void Dispose()
        {
            _dac.Dispose();
        }
    }
}