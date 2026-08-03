namespace Onpoint.Store.Domin.Enums
{

    public enum OrderStatus
    {
        Pending = 0,
        Processing = 1,
        Completed = 2,
        Cancelled = 3,
        Refunded = 4,
        PendingPayment = 5,
        PaymentFailed = 6,
    }
}
