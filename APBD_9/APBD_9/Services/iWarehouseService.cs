using APBD_9.Models;

namespace APBD_9.Services;

public interface iWarehouseService
{
    Task<bool> DoesProductExist(int productId);
    Task<bool> DoesOrderExist(int productId, int amount);
    
    Task<bool> DoesWarehouseExist(int warehouseId);
    Task<bool> OrderNotCompleted(int orderId);
    
    Task<int> GetOrderId(int productId, int amount);
    
    Task<decimal> GetProductPrice(int productId);
    Task<int> putInProductWarehouse(ProductInWarehouseDTO body);
    
    Task<int> putInProductWarehouseProcedure(ProductInWarehouseDTO body);
}