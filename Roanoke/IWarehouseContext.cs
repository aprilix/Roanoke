namespace Roanoke
{
    public interface IWarehouseContext
    {
        IWarehouseDataAccessComponent CreateWarehouseDataAccessComponent();
    }
}