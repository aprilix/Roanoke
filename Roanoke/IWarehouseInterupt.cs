namespace Roanoke
{
    public interface IWarehouseInterupt
    {
        bool IsWarehouseSchemaLockRequested { get; }
        bool IsWarehouseHostCancelled { get; }
    }
}