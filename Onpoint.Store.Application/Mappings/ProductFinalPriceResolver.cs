using AutoMapper;
using Onpoint.Store.Application.Common;
using Onpoint.Store.Domin.Entities;

namespace Onpoint.Store.Application.Mappings
{
    public class ProductFinalPriceResolver : IValueResolver<Product, object, decimal>
    {
        public decimal Resolve(Product source, object destination, decimal destMember, ResolutionContext context)
        {
            return PricingHelper.CalculateMinFinalPrice(source);
        }
    }
}