using BuildingBlocks.Results;
using Onpoint.Store.Domin.Enums;

namespace Onpoint.Store.Application.DTOs.Coupon
{
    public class CouponFilterRequest : PaginationRequest
    {
        public CouponType? DiscountType { get; set; }
    }
}