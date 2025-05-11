namespace APBD_9.Models;

public class ProductInWarehouseDTO
{
    public int IdProduct { get; set; }
    public int IdWarehouse { get; set; }
    public int Amount { get; set; }
    public DateTime CreatedAt{ get; set; }
}