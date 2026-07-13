using Onpoint.Store.Domin.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Onpoint.Store.Domin.Entities
{
    public class Address : BaseEntity
    {
        public int UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser? User { get; set; }

        public string Title { get; set; } = string.Empty; // مثال: "المنزل"، "المكتب"


        public string Governorate { get; set; } = string.Empty; // العاصمة، الفروانية، حولي، إلخ
        public string Area { get; set; } = string.Empty;       // السالمية، حولي، إلخ
        public string Block { get; set; } = string.Empty;       // القطعة
        public string? Street { get; set; }                     // الشارع
        public string? Avenue { get; set; }                     // الجادة / الزقاق
        public string HouseNumber { get; set; } = string.Empty; // رقم الدار
        public string? Floor { get; set; }                      // الطابق
        public string? Apartment { get; set; }                  // رقم الشقة / الفلة

        public string? ExtraDirections { get; set; }            // ملاحظات إضافية (مثلاً: بيت الرجباني خلف المسجد)

        // عشان نعرف ده العنوان الأساسي ولا لأ
        public bool IsDefault { get; set; } = false;

        // علاقة مع الطلب (طلب واحد له عنوان واحد)
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
