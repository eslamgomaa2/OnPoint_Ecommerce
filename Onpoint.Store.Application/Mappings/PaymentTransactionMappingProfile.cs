using AutoMapper;
using Onpoint.Store.Application.DTOs.PaymentTransaction;
using Onpoint.Store.Domin.Entities.Sales;

namespace Onpoint.Store.Application.Mappings
{
    public class PaymentTransactionMappingProfile : Profile
    {
        public PaymentTransactionMappingProfile()
        {
            CreateMap<PaymentTransaction, PaymentTransactionDto>();
        }
    }
}