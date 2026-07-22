// Onpoint.Store.Application/Mapping/CustomerProfile.cs
using AutoMapper;
using Onpoint.Store.Application.DTOs.Customer;
using Onpoint.Store.Domin.Enums;
using CustomerEntity = Onpoint.Store.Domin.Entities.Identity.Customer;

namespace Onpoint.Store.Application.Mapping
{
    public class CustomerProfile : Profile
    {
        public CustomerProfile()
        {
            CreateMap<CreateCustomerDto, CustomerEntity>();
            CreateMap<UpdateCustomerDto, CustomerEntity>();

            CreateMap<CustomerEntity, CustomerListItemDto>()
                .ForMember(d => d.OrdersCount, o => o.MapFrom(s => s.Orders.Count))
                .ForMember(d => d.TotalSpent, o => o.MapFrom(s =>
                    s.Orders.Where(x => x.Status == OrderStatus.Completed)
                            .Sum(x => (decimal?)x.TotalAmount) ?? 0))
                .ForMember(d => d.LastPurchase, o => o.MapFrom(s =>
                    s.Orders.Where(x => x.Status == OrderStatus.Completed)
                            .OrderByDescending(x => x.CreatedAt)
                            .Select(x => (DateTime?)x.CreatedAt)
                            .FirstOrDefault()));

            CreateMap<CustomerEntity, CustomerDetailsDto>()
                .ForMember(d => d.OrdersCount, o => o.MapFrom(s => s.Orders.Count))
                .ForMember(d => d.TotalSpent, o => o.MapFrom(s =>
                    s.Orders.Where(x => x.Status == OrderStatus.Completed)
                            .Sum(x => (decimal?)x.TotalAmount) ?? 0))
                .ForMember(d => d.LastPurchase, o => o.MapFrom(s =>
                    s.Orders.Where(x => x.Status == OrderStatus.Completed)
                            .OrderByDescending(x => x.CreatedAt)
                            .Select(x => (DateTime?)x.CreatedAt)
                            .FirstOrDefault()));
        }
    }
}