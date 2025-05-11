namespace APBD_9.Models;

public class OrderDTO
{
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Amount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? FulfilledAt { get; set; }
}