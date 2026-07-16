using Onpoint.Store.Domin.Common;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Entities.Sales;

public class PosSessionItem : BaseEntity
{
    public int PosSessionId { get; set; }
    public virtual PosSession? PosSession { get; set; }
    public int ProductId { get; set; }
    public virtual Product? Product { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice => UnitPrice * Quantity;
}