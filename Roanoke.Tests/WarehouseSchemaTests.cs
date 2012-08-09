using System.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Roanoke.Tests
{
    [TestClass]
    public class WarehouseSchemaTests
    {
        [TestMethod]
        public void WarehouseSchema_should_execute_sql_scripts_when_making_changes()
        {
            var context = Substitute.For<IWarehouseContext>();
            var dac = Substitute.For<IWarehouseDataAccessComponent>();
            context.CreateWarehouseDataAccessComponent().Returns(dac);

            var schema = new WarehouseSchema(context);
            schema.MakeSchemaChanges();

            dac.ReceivedWithAnyArgs().ExecuteNonQuery(CommandType.Text, null, null);
        }

        [TestMethod]
        public void WarehouseSchema_should_update_the_schema_version_when_making_changes()
        {
            var context = Substitute.For<IWarehouseContext>();
            var dac = Substitute.For<IWarehouseDataAccessComponent>();
            context.CreateWarehouseDataAccessComponent().Returns(dac);

            var schema = new WarehouseSchema(context);
            schema.MakeSchemaChanges();

            dac.Received().SetProperty(null, "/Adapter/Schema/Roanoke/WarehouseSchemaVersion", "1.0.0.0");
        }

        [TestMethod]
        public void WarehouseSchema_should_require_schema_changes_when_the_version_is_absent()
        {
            var context = Substitute.For<IWarehouseContext>();
            var dac = Substitute.For<IWarehouseDataAccessComponent>();
            context.CreateWarehouseDataAccessComponent().Returns(dac);
            dac.GetProperty(null, null).ReturnsForAnyArgs(string.Empty);

            var schema = new WarehouseSchema(context);
            var result = schema.SchemaChangePending();

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void WarehouseSchema_should_not_require_schema_changes_when_the_version_is_current()
        {
            var context = Substitute.For<IWarehouseContext>();
            var dac = Substitute.For<IWarehouseDataAccessComponent>();
            context.CreateWarehouseDataAccessComponent().Returns(dac);
            dac.GetProperty(null, null).ReturnsForAnyArgs(typeof(WarehouseSchema).Assembly.GetName().Version.ToString());

            var schema = new WarehouseSchema(context);
            var result = schema.SchemaChangePending();

            Assert.IsFalse(result);
        }

    
    }
}
