using AutoMapper;
using Onpoint.Store.Domin.Entities;

namespace Onpoint.Store.Application.Mappings
{
    public class ProductFinalPriceResolver : IValueResolver<Product, object, decimal>
    {
        public decimal Resolve(Product source, object destination, decimal destMember, ResolutionContext context)
        {
            var activeDiscount = source.Discounts?.FirstOrDefault(d =>
                d.IsActive &&
                d.StartDate <= DateTime.UtcNow &&
                d.EndDate >= DateTime.UtcNow);

            if (activeDiscount != null)
            {
                var discountAmount = source.Price * (activeDiscount.DiscountPercentage / 100);
                return source.Price - discountAmount;
            }

            return source.Price;
        }
    }
}
