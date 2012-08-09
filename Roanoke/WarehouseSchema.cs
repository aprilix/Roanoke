using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.TeamFoundation.Warehouse;

namespace Roanoke
{
    public class WarehouseSchema
    {
        private const string WarehouseSchemaVersionPropertyKey = "/Adapter/Schema/Roanoke/WarehouseSchemaVersion";
        private static readonly Version CurrentVersion = Assembly.GetExecutingAssembly().GetName().Version;
        private readonly IWarehouseContext _warehouseContext;

        public WarehouseSchema(IWarehouseContext warehouseContext)
        {
            _warehouseContext = warehouseContext;
        }

        public bool SchemaChangePending()
        {
            string versionString;
            using (var dac = _warehouseContext.CreateWarehouseDataAccessComponent())
            {
                versionString = dac.GetProperty(null, WarehouseSchemaVersionPropertyKey);
            }

            if (string.IsNullOrEmpty(versionString)) return true;

            Version warehouseVersion;
            if (!Version.TryParse(versionString, out warehouseVersion))
                throw new WarehouseException("Unexpected warehouse schema version string.");

            var compare = CurrentVersion.CompareTo(warehouseVersion);
            if (compare == 0) return false;
            if (compare > 0) return true;
            
            throw new WarehouseException("Warehouse schema version is newer than adapter version.");
        }

        public void MakeSchemaChanges()
        {
            var thisAssembly = Assembly.GetExecutingAssembly();
            var scripts = thisAssembly.GetManifestResourceNames().Where(s =>
                s.StartsWith("Roanoke.Schema.", StringComparison.InvariantCultureIgnoreCase) &&
                s.EndsWith(".sql", StringComparison.InvariantCultureIgnoreCase)
                ).OrderBy(s => s);
            using (var dac = _warehouseContext.CreateWarehouseDataAccessComponent())
            {

                foreach (var script in scripts)
                {
                    using (var reader = new StreamReader(thisAssembly.GetManifestResourceStream(script)))
                    {
                        dac.ExecuteNonQuery(CommandType.Text, reader.ReadToEnd(), new SqlParameter[0]);
                    }
                    
                }

                dac.SetProperty(null, WarehouseSchemaVersionPropertyKey, CurrentVersion.ToString());
            }
        }
    }
}