using Onpoint.Store.Domin.Entities;

namespace Onpoint.Store.Application.Common
{
    public static class PricingHelper
    {


        public static decimal CalculateFinalPrice(Product product, ProductVariant variant)
        {
            return CalculatePriceWithDiscount(variant.Price, product);
        }

        public static decimal CalculateMinFinalPrice(Product product)
        {
            if (product.Variants == null || !product.Variants.Any(v => v.IsActive))
                return 0;

            var minPrice = product.Variants
                .Where(v => v.IsActive)
                .Min(v => v.Price);

            return CalculatePriceWithDiscount(minPrice, product);
        }

        public static decimal CalculateFinalPrice(Product product, int variantId)
        {
            var variant = product.Variants?.FirstOrDefault(v => v.Id == variantId && v.IsActive);
            if (variant == null)
                return 0;

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