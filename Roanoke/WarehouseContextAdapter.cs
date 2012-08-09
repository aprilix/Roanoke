using Microsoft.TeamFoundation.Warehouse;

namespace Roanoke
{
    public class WarehouseContextAdapter : IWarehouseContext
    {
        private readonly WarehouseContext _context;

        public WarehouseContextAdapter(WarehouseContext context)
        {
            _context = context;
        }

        public IWarehouseDataAccessComponent CreateWarehouseDataAccessComponent()
        {
            return new WarehouseDataAccessComponentAdapter(_context.CreateWarehouseDataAccessComponent());
        }
    }
}