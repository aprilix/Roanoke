using Microsoft.TeamFoundation.Warehouse;

namespace Roanoke
{
    public class BuildDropWarehouseAdapter : WarehouseAdapter, IWarehouseInterupt
    {

        public override void Initialize()
        {
        }

        public override void MakeSchemaChanges()
        {
            var schema = new WarehouseSchema(new WarehouseContextAdapter(WarehouseContext));
            if (!schema.SchemaChangePending()) return;

            schema.MakeSchemaChanges();
        }

        public override DataChangesResult MakeDataChanges()
        {
            var schema = new WarehouseSchema(new WarehouseContextAdapter(WarehouseContext));
            if (schema.SchemaChangePending()) return DataChangesResult.SchemaChangesPending;

            var processor = new BuildDropProcessor(this, new WarehouseContextAdapter(WarehouseContext), new TeamFoundationBuildServiceAdapter(RequestContext), new FolderSizeCalculator());
            return processor.ProcessBuildDrops(RequestContext.ServiceHost.InstanceId);
        }

    }
}
