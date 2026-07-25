using Onpoint.Store.Domin.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Onpoint.Store.Domin.Entities
{
    public class Invoice : BaseEntity
    {
        public int OrderId { get; set; }
        [ForeignKey(nameof(OrderId))]
        public virtual Order? Order { get; set; }

        public string InvoiceNumber { get; set; } = string.Empty;


        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; } = 0;
        public decimal TotalAmount { get; set; }
        public bool IsTaxable { get; set; } = false;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxPercentage { get; set; } = 0;

        // لو العميل شركة (B2B) ومطلوب منه الرقم الضريبي
        public string? CustomerTaxNumber { get; set; }

        public string? CustomerCompanyName { get; set; }

        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

        // لو بتولد PDF للفاتورة
        public string? PdfUrl { get; set; }
    }
}
