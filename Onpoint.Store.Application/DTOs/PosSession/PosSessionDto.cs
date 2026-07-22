using Onpoint.Store.Application.DTOs.PosSession;
using Onpoint.Store.Domin.Enums;

public class PosSessionDto
{
    public int Id { get; set; }
    public string CashierName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public PosSessionStatus Status { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CouponCode { get; set; }
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }

    public decimal TaxAmount { get; set; }
    public decimal AmountReceived { get; set; }
    public decimal Change { get; set; }
    public ICollection<PosSessionItemDto> Items { get; set; } = new List<PosSessionItemDto>();
    public DateTime CreatedAt { get; set; }
}