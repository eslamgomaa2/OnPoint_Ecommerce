namespace Onpoint.Store.Domin.Enums
{
    public enum PaymentMethod
    {
        CashOnDelivery = 0,
        Knet = 1,           // مطلوب جداً في الكويت

        CreditCard = 2,     // فيزا/ماستركارد
        BenefitPay = 3,     // محفظة بينفيت
        ApplePay = 4,
        Cash = 5
    }
}
