using Onpoint.Store.Domin.Entities;

namespace Onpoint.Store.Application.Common
{
    public static class PricingHelper
    {
        public static decimal CalculateFinalPrice(Product product)
        {
            return CalculatePriceWithDiscount(product.Price, product);
        }

        public static decimal CalculateFinalPrice(Product product, ProductVariant variant)
        {
            return CalculatePriceWithDiscount(variant.Price, product);
        }

        private static decimal CalculatePriceWithDiscount(decimal basePrice, Product product)
        {
            var activeDiscount = GetActiveDiscount(product);

            if (activeDiscount != null && activeDiscount.DiscountPercentage > 0)
            {

                decimal discountAmount = basePrice * (activeDiscount.DiscountPercentage / 100m);
                return basePrice - discountAmount;
            }

            return basePrice;
        }

        private static Discount? GetActiveDiscount(Product product)
        {
            var now = DateTime.UtcNow;

            return product.Discounts?.FirstOrDefault(d =>
                d.IsActive &&
                d.StartDate.ToUniversalTime() <= now &&
                d.EndDate.ToUniversalTime() >= now);
        }
    }
}