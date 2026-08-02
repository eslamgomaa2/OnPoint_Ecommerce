using AutoMapper;
using Onpoint.Store.Application.DTOs;
using Onpoint.Store.Application.DTOs.PosSales;
using Onpoint.Store.Application.DTOs.PosSession;
using Onpoint.Store.Domin.Entities;

namespace Onpoint.Store.Application.Mapping
{
    public class SalesReceiptProfile : Profile
    {
        public SalesReceiptProfile()
        {
            CreateMap<OrderItem, SalesOrderItemDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductName))
                .ForMember(dest => dest.ProductImageUrl, opt => opt.MapFrom(src => src.ProductImageUrl))
                .ForMember(dest => dest.ProductDescription, opt => opt.MapFrom(src => src.VariantDescription))
                .ForMember(dest => dest.Sku, opt => opt.MapFrom(src => src.ProductVariant != null ? src.ProductVariant.Sku : string.Empty))
                .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.UnitPrice))
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
                .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.TotalPrice));

            CreateMap<Order, SalesReceiptPreviewDto>()
                .ForMember(dest => dest.InvoiceNumber, opt => opt.MapFrom(src => src.InvoiceNumber ?? string.Empty))
                .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.CashierName, opt => opt.MapFrom(src =>
                    src.Cashier != null ? (src.Cashier.UserName ?? src.Cashier.Email ?? string.Empty) : string.Empty))
                .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : string.Empty))
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src =>
                    src.Customer != null ? (src.Customer.FName + " " + src.Customer.LName).Trim() : null))
                .ForMember(dest => dest.CustomerPhone, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.OrderItems))
                .ForMember(dest => dest.SubTotal, opt => opt.MapFrom(src => src.SubTotal))
                .ForMember(dest => dest.Discount, opt => opt.MapFrom(src => src.DiscountAmount))
                .ForMember(dest => dest.Tax, opt => opt.MapFrom(src => src.TaxAmount))
                .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.TotalAmount))
                .ForMember(dest => dest.AmountReceived, opt => opt.MapFrom(src => src.AmountReceived))
                .ForMember(dest => dest.Change, opt => opt.MapFrom(src => src.Change))
                .ForMember(dest => dest.Payments, opt => opt.MapFrom(src => new List<PaymentSummaryDto>
                {
                    new PaymentSummaryDto
                    {
                        Method = src.PaymentMethod.ToString(),
                        TotalPaid = src.TotalAmount,
                        AmountReceived = src.AmountReceived,
                        Change = src.Change
                    }
                }))
                .ForMember(dest => dest.QrCodeData, opt => opt.MapFrom(src => src.QRCode));
        }
    }
}