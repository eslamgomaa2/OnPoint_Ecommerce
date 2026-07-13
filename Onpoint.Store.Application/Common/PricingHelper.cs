using Onpoint.Store.Domin.Entities;

namespace Onpoint.Store.Application.Common
{
    public static class PricingHelper
    {
        public static decimal CalculateFinalPrice(Product product)
        {
            var activeDiscount = product.Discounts?.FirstOrDefault(d =>
                d.IsActive && d.StartDate <= DateTime.UtcNow && d.EndDate >= DateTime.UtcNow);

            if (activeDiscount != null)
                return product.Price - (product.Price * (activeDiscount.DiscountPercentage / 100));

            return product.Price;
        }
    }
}