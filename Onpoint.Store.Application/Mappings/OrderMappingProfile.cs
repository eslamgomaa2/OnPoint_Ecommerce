using AutoMapper;
using Onpoint.Store.Application.DTOs;
using Onpoint.Store.Application.DTOs.Order;
using Onpoint.Store.Application.DTOs.PosSession;
using Onpoint.Store.Domin.Entities;

namespace Onpoint.Store.Application.Mappings
{
    public class OrderMappingProfile : Profile
    {
        public OrderMappingProfile()
        {
            CreateMap<OrderItem, SalesOrderItemDto>();

            CreateMap<Order, PosOrderDto>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.OrderItems))
                .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : string.Empty))
                .ForMember(dest => dest.CashierName, opt => opt.MapFrom(src => src.Cashier != null ? src.Cashier.UserName : string.Empty))
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? $"{src.Customer.FName} {src.Customer.LName}" : string.Empty))
                .ForMember(dest => dest.CustomerPhone, opt => opt.MapFrom(src => src.PhoneNumber));

            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.OrderNumber,
                    opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.InvoiceNumber) ? src.InvoiceNumber : src.Id.ToString()));


            CreateMap<OrderItem, OrderItemDto>();
        }
    }
}