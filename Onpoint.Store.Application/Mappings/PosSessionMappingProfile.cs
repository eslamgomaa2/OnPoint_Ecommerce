using AutoMapper;
using Onpoint.Store.Application.DTOs.Customer;
using Onpoint.Store.Application.DTOs.PaymentTransaction;
using Onpoint.Store.Application.DTOs.PosSession;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Entities.Identity;
using Onpoint.Store.Domin.Entities.Sales;

namespace Onpoint.Store.Application.Mappings
{
    public class PosSessionMappingProfile : Profile
    {
        public PosSessionMappingProfile()
        {
            // ✅ حماية Product عند جلب SKU
            CreateMap<PosSessionItem, PosSessionItemDto>()
                .ForMember(dest => dest.SKU, opt => opt.MapFrom(src =>
                    src.ProductVariant != null ? src.ProductVariant.Sku :
                    src.Product != null ? src.Product.Sku : string.Empty));

            // ✅ حماية Items عند حساب المبالغ
            CreateMap<PosSession, PosSessionDto>()
                .ForMember(dest => dest.CashierName, opt => opt.MapFrom(src => src.Cashier != null ? src.Cashier.UserName : string.Empty))
                .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : string.Empty))
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? $"{src.Customer.FName} {src.Customer.LName}" : string.Empty))
                .ForMember(dest => dest.SubTotal, opt => opt.MapFrom(src => src.Items != null ? src.Items.Sum(i => i.UnitPrice * i.Quantity) : 0m))
                .ForMember(dest => dest.TaxAmount, opt => opt.MapFrom(src => 0m))
                .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.Items != null ? (src.Items.Sum(i => i.UnitPrice * i.Quantity) - src.DiscountAmount) : 0m));

            CreateMap<OrderItem, PosOrderItemDto>();

            CreateMap<PaymentTransaction, PaymentTransactionDto>();

            CreateMap<Order, PosOrderDto>()
                .ForMember(dest => dest.CashierName, opt => opt.MapFrom(src => src.Cashier != null ? src.Cashier.UserName : string.Empty))
                .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : string.Empty))
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? $"{src.Customer.FName} {src.Customer.LName}" : string.Empty))
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.OrderItems));

            CreateMap<Customer, CustomerDto>();
        }
    }
}