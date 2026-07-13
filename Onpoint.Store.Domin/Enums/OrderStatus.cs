namespace Onpoint.Store.Domin.Enums
{

    public enum OrderStatus
    {
        Pending = 0,       // جديد (لم يتم الدفع أو التأكيد بعد)
        Processing = 1,    // قيد التجهيز
        Shipped = 2,       // تم الشحن
        Delivered = 3,     // تم التسليم
        Cancelled = 4,     // ملغي
        Refunded = 5       // مسترد
    }
}
