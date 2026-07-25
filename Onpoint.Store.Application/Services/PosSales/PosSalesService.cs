using AutoMapper;
using BuildingBlocks.Results;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Onpoint.Store.Application.DTOs;
using Onpoint.Store.Application.DTOs.Order;
using Onpoint.Store.Application.DTOs.PosSales;
using Onpoint.Store.Application.DTOs.PosSession;
using Onpoint.Store.Application.DTOs.Refund;
using Onpoint.Store.Application.Interfaces;
using Onpoint.Store.Application.Services.CodeGeneration.QrCodeGeneration;
using Onpoint.Store.Domin.Repositories;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDFDocument = QuestPDF.Fluent.Document;
namespace Onpoint.Store.Application.Services.PosSales
{
    public class PosSalesService : IPosSalesService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IRefundService _refundService;
        private readonly IQrCodeService _qrCodeService;
        private readonly IImageStorageService _imageStorageService;
        private readonly ServiceResultHandler _resultHandler;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<PosSalesService> _logger;
        private readonly IConfiguration _configuration;

        public PosSalesService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IRefundService refundService,
            IQrCodeService qrCodeService,
            IImageStorageService imageStorageService,
            ServiceResultHandler resultHandler,
            IHttpClientFactory httpClientFactory,
            ILogger<PosSalesService> logger,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _refundService = refundService;
            _qrCodeService = qrCodeService;
            _imageStorageService = imageStorageService;
            _resultHandler = resultHandler;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<ServiceResult<PosSalesListResponse>> GetPosSalesPagedAsync(
            PosSalesFilterRequest filter, int? forcedBranchId = null, CancellationToken ct = default)
        {
            var query = _unitOfWork.Orders.QueryNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.Cashier)
                .Include(o => o.Branch)
                .AsQueryable();


            if (forcedBranchId.HasValue)
                query = query.Where(o => o.BranchId == forcedBranchId.Value);
            else if (filter.BranchId.HasValue)
                query = query.Where(o => o.BranchId == filter.BranchId.Value);


            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.Trim().ToLower();
                query = query.Where(o =>
                    (o.InvoiceNumber != null && o.InvoiceNumber.ToLower().Contains(search)) ||
                    (o.Customer != null && (o.Customer.FName + " " + o.Customer.LName).ToLower().Contains(search)));
            }


            if (filter.Status.HasValue)
                query = query.Where(o => o.Status == filter.Status.Value);

            if (filter.PaymentMethod.HasValue)
                query = query.Where(o => o.PaymentMethod == filter.PaymentMethod.Value);

            if (filter.DateFrom.HasValue)
                query = query.Where(o => o.CreatedAt >= filter.DateFrom.Value);

            if (filter.DateTo.HasValue)
                query = query.Where(o => o.CreatedAt <= filter.DateTo.Value);

            var totalCount = await query.CountAsync(ct);


            query = filter.SortBy switch
            {
                OrderSortBy.OrderNumber => filter.Descending
                    ? query.OrderByDescending(o => o.InvoiceNumber)
                    : query.OrderBy(o => o.InvoiceNumber),
                OrderSortBy.TotalAmount => filter.Descending
                    ? query.OrderByDescending(o => o.TotalAmount)
                    : query.OrderBy(o => o.TotalAmount),
                OrderSortBy.Status => filter.Descending
                    ? query.OrderByDescending(o => o.Status)
                    : query.OrderBy(o => o.Status),
                _ => filter.Descending
                    ? query.OrderByDescending(o => o.CreatedAt)
                    : query.OrderBy(o => o.CreatedAt)
            };


            var items = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(o => new PosSalesListItemDto
                {
                    Id = o.Id,
                    InvoiceNumber = o.InvoiceNumber ?? " there is no invoice number ",
                    CustomerName = o.Customer != null ? $"{o.Customer.FName} {o.Customer.LName}".Trim() : string.Empty,
                    CashierName = o.Cashier != null ? o.Cashier.UserName ?? o.Cashier.Email ?? string.Empty : string.Empty,
                    BranchName = o.Branch != null ? o.Branch.Name : string.Empty,
                    PaymentMethod = o.PaymentMethod.ToString(),
                    Status = o.Status.ToString(),
                    Amount = o.TotalAmount,
                    Date = o.CreatedAt
                })
                .ToListAsync(ct);

            var response = new PosSalesListResponse
            {
                Items = items,
                TotalCount = totalCount
            };

            return _resultHandler.Success(response);
        }

        public async Task<ServiceResult<PosOrderDetailsDto>> GetOrderDetailsAsync(int orderId, CancellationToken ct = default)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId,
                include: q => q.Include(o => o.OrderItems)
                               .ThenInclude(oi => oi.Product)
                               .Include(o => o.Customer)
                               .Include(o => o.Cashier)
                               .Include(o => o.Branch)
                               .Include(o => o.StatusHistory)
                               .Include(o => o.Refunds)
                               .ThenInclude(r => r.RefundItems)
                               .ThenInclude(ri => ri.OrderItem),
                ct: ct);

            if (order == null)
                return _resultHandler.NotFound<PosOrderDetailsDto>("Order not found.");

            var dto = new PosOrderDetailsDto
            {
                Id = order.Id,
                InvoiceNumber = order.InvoiceNumber,
                Date = order.CreatedAt,
                Status = order.Status,
                Customer = new CustomerSummaryDto
                {
                    Id = order.CustomerId.Value,
                    Name = order.Customer != null ? $"{order.Customer.FName} {order.Customer.LName}".Trim() : string.Empty
                },
                Cashier = new CashierSummaryDto
                {
                    Id = order.CashierId ?? 0,
                    Name = order.Cashier != null ? order.Cashier.UserName ?? order.Cashier.Email ?? string.Empty : string.Empty
                },
                Branch = new BranchSummaryDto
                {
                    Id = order.BranchId,
                    Name = order.Branch?.Name ?? string.Empty
                },
                Items = order.OrderItems.Select(oi => new SalesOrderItemDto
                {
                    Id = oi.Id,
                    ProductName = oi.ProductName,
                    ProductImageUrl = oi.ProductImageUrl,
                    VariantDescription = oi.VariantDescription,
                    Sku = oi.Product?.Sku ?? string.Empty,
                    UnitPrice = oi.UnitPrice,
                    Quantity = oi.Quantity,
                    Total = oi.TotalPrice
                }).ToList(),
                Summary = new OrderSummaryDto
                {
                    Subtotal = order.SubTotal,
                    Discount = order.DiscountAmount,
                    Tax = order.TaxAmount,
                    GrandTotal = order.TotalAmount
                },
                Payment = new PaymentSummaryDto
                {
                    Method = order.PaymentMethod.ToString(),
                    TotalPaid = order.TotalAmount,
                    AmountReceived = order.AmountReceived,
                    Change = order.Change
                },
                Timeline = order.StatusHistory.Select(sh => new TimelineEventDto
                {
                    Event = sh.EventName.ToString(),
                    Time = sh.EventTime.ToString("hh:mm tt"),
                    Timestamp = sh.EventTime
                }).ToList(),
                Refunds = _mapper.Map<List<RefundDto>>(order.Refunds)
            };

            return _resultHandler.Success(dto);
        }

        public async Task<ServiceResult<RefundDto>> FullRefundAsync(int orderId, FullRefundRequestDto dto, int? processedByUserId, CancellationToken ct = default)
        {
            if (!processedByUserId.HasValue)
                return _resultHandler.BadRequest<RefundDto>("User ID is required.");

            var order = await _unitOfWork.Orders.GetByIdAsync(orderId, ct: ct);
            if (order == null)
                return _resultHandler.NotFound<RefundDto>("Order not found.");

            try
            {
                var result = await _refundService.CreateFullRefundAsync(processedByUserId.Value, order.BranchId, orderId, dto, ct);
                return result;
            }
            catch (InvalidOperationException ex)
            {
                return _resultHandler.BadRequest<RefundDto>(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return _resultHandler.Unauthorized<RefundDto>(ex.Message);
            }
        }

        public async Task<ServiceResult<RefundDto>> PartialRefundAsync(
            int orderId, PartialRefundRequestDto dto, int? processedByUserId, CancellationToken ct = default)
        {
            if (!processedByUserId.HasValue)
                return _resultHandler.BadRequest<RefundDto>("User ID is required.");

            var order = await _unitOfWork.Orders.GetByIdAsync(orderId, ct: ct);
            if (order == null)
                return _resultHandler.NotFound<RefundDto>("Order not found.");

            try
            {
                var result = await _refundService.CreatePartialRefundAsync(processedByUserId.Value, order.BranchId, orderId, dto, ct);
                return result;
            }
            catch (InvalidOperationException ex)
            {
                return _resultHandler.BadRequest<RefundDto>(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return _resultHandler.Unauthorized<RefundDto>(ex.Message);
            }
        }

        public async Task<ServiceResult<SalesReceiptPreviewDto>> GetReceiptAsync(int orderId, CancellationToken ct = default)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId,
                include: q => q.Include(o => o.OrderItems)
                               .ThenInclude(oi => oi.Product)
                               .Include(o => o.Customer)
                               .Include(o => o.Cashier)
                               .Include(o => o.Branch),
                ct: ct);

            if (order == null)
                return _resultHandler.NotFound<SalesReceiptPreviewDto>("Order not found.");

            var receipt = new SalesReceiptPreviewDto
            {
                InvoiceNumber = order.InvoiceNumber ?? string.Empty,
                Date = order.CreatedAt,
                CashierName = order.Cashier?.UserName ?? order.Cashier?.Email ?? string.Empty,
                BranchName = order.Branch?.Name ?? string.Empty,
                CustomerName = order.Customer != null ? $"{order.Customer.FName} {order.Customer.LName}".Trim() : null,
                CustomerPhone = order.PhoneNumber,
                Items = order.OrderItems.Select(oi => new SalesOrderItemDto
                {
                    ProductName = oi.ProductName,
                    Sku = oi.Product?.Sku ?? string.Empty,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    Total = oi.TotalPrice
                }).ToList(),
                SubTotal = order.SubTotal,
                Discount = order.DiscountAmount,
                Tax = order.TaxAmount,
                Total = order.TotalAmount,
                AmountReceived = order.AmountReceived,
                Change = order.Change,
                Payments = new List<PaymentSummaryDto>
                {
                    new PaymentSummaryDto
                    {
                        Method = order.PaymentMethod.ToString(),
                        TotalPaid = order.TotalAmount,
                        AmountReceived = order.AmountReceived,
                        Change = order.Change
                    }
                },
                QrCodeData = order.QRCode
            };

            return _resultHandler.Success(receipt);
        }

        public async Task<ServiceResult<byte[]>> GeneratePdfAsync(int orderId, CancellationToken ct = default)
        {
            var receiptResult = await GetReceiptAsync(orderId, ct);
            if (!receiptResult.Succeeded || receiptResult.Data == null)
                return _resultHandler.BadRequest<byte[]>(receiptResult.Message ?? "Failed to get receipt data.");

            var receipt = receiptResult.Data;

            byte[]? qrImageBytes = null;
            if (!string.IsNullOrEmpty(receipt.QrCodeData))
            {
                try
                {
                    using var httpClient = _httpClientFactory.CreateClient();
                    qrImageBytes = await httpClient.GetByteArrayAsync(receipt.QrCodeData, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to download QR code image for order {OrderId}", orderId);
                }
            }

            var document = QuestPDFDocument.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));


                    page.Header().Column(col =>
                    {
                        col.Item().Text("Company Name").Bold().FontSize(20).AlignCenter();
                        col.Item().Text("123 Main St, New York, NY 10001").FontSize(10).AlignCenter();
                        col.Item().Text("Tel: +1 555-0100").FontSize(10).AlignCenter();
                        col.Item().PaddingTop(8).LineHorizontal(1);
                    });


                    page.Content().PaddingTop(10).Column(column =>
                    {
                        column.Spacing(6);


                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text($"Invoice: {receipt.InvoiceNumber}").SemiBold();
                            row.RelativeItem().AlignRight().Text($"Date: {receipt.Date:dd/MM/yyyy}");
                        });
                        column.Item().Text($"Cashier: {receipt.CashierName}");
                        column.Item().Text($"Branch: {receipt.BranchName}");
                        column.Item().Text($"Customer: {receipt.CustomerName}");

                        column.Item().PaddingTop(5).LineHorizontal(1);

                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3); // product
                                columns.RelativeColumn(1); // qty
                                columns.RelativeColumn(1); // unit price
                                columns.RelativeColumn(1); // total
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Item").Bold();
                                header.Cell().AlignCenter().Text("Qty").Bold();
                                header.Cell().AlignRight().Text("Price").Bold();
                                header.Cell().AlignRight().Text("Total").Bold();
                                header.Cell().ColumnSpan(4).PaddingTop(3).LineHorizontal(0.5f);
                            });

                            foreach (var item in receipt.Items)
                            {
                                table.Cell().PaddingVertical(3).Text(item.ProductName);
                                table.Cell().PaddingVertical(3).AlignCenter().Text(item.Quantity.ToString());
                                table.Cell().PaddingVertical(3).AlignRight().Text($"${item.UnitPrice:F2}");
                                table.Cell().PaddingVertical(3).AlignRight().Text($"${item.Total:F2}");
                            }
                        });

                        column.Item().PaddingTop(5).LineHorizontal(1);


                        column.Item().AlignRight().Width(220).Column(totals =>
                        {
                            totals.Spacing(3);

                            totals.Item().Row(row =>
                            {
                                row.RelativeItem().Text("Subtotal");
                                row.RelativeItem().AlignRight().Text($"${receipt.SubTotal:F2}");
                            });
                            totals.Item().Row(row =>
                            {
                                row.RelativeItem().Text("Discount");
                                row.RelativeItem().AlignRight().Text($"-${receipt.Discount:F2}");
                            });
                            totals.Item().Row(row =>
                            {
                                row.RelativeItem().Text("Tax");
                                row.RelativeItem().AlignRight().Text($"${receipt.Tax:F2}");
                            });
                            totals.Item().PaddingTop(3).LineHorizontal(1);
                            totals.Item().Row(row =>
                            {
                                row.RelativeItem().Text("TOTAL").Bold().FontSize(13);
                                row.RelativeItem().AlignRight().Text($"${receipt.Total:F2}").Bold().FontSize(13);
                            });
                            totals.Item().PaddingTop(5).Row(row =>
                            {
                                row.RelativeItem().Text("Cash");
                                row.RelativeItem().AlignRight().Text($"${receipt.AmountReceived:F2}");
                            });
                            totals.Item().Row(row =>
                            {
                                row.RelativeItem().Text("Change");
                                row.RelativeItem().AlignRight().Text($"${receipt.Change:F2}");
                            });
                        });


                        column.Item().PaddingTop(15).AlignCenter().Column(qrCol =>
                        {
                            if (qrImageBytes != null)
                            {
                                qrCol.Item().AlignCenter().Width(100).Image(qrImageBytes);
                            }
                            else
                            {
                                qrCol.Item().AlignCenter().Text("[QR Code]").FontSize(9).FontColor(Colors.Grey.Medium);
                            }
                        });

                        column.Item().PaddingTop(10).AlignCenter()
                            .Text("Thank you for your purchase!").Italic();
                    });


                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.CurrentPageNumber();
                        x.Span(" / ");
                        x.TotalPages();
                    });
                });
            });

            var pdfBytes = document.GeneratePdf();
            return _resultHandler.Success(pdfBytes);
        }

        public async Task<ServiceResult<byte[]>> ExportToExcelAsync(
            ExportPosSalesRequestDto filter, int? forcedBranchId = null, CancellationToken ct = default)
        {
            var query = _unitOfWork.Orders.QueryNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.Cashier)
                .Include(o => o.Branch)
                .AsQueryable();

            if (forcedBranchId.HasValue)
                query = query.Where(o => o.BranchId == forcedBranchId.Value);
            else if (filter.BranchId.HasValue)
                query = query.Where(o => o.BranchId == filter.BranchId.Value);

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.Trim().ToLower();
                query = query.Where(o =>
                    (o.InvoiceNumber != null && o.InvoiceNumber.ToLower().Contains(search)) ||
                    (o.Customer != null && (o.Customer.FName + " " + o.Customer.LName).ToLower().Contains(search)));
            }

            if (filter.Status.HasValue)
                query = query.Where(o => o.Status == filter.Status.Value);

            if (filter.PaymentMethod.HasValue)
                query = query.Where(o => o.PaymentMethod == filter.PaymentMethod.Value);

            if (filter.DateFrom.HasValue)
                query = query.Where(o => o.CreatedAt >= filter.DateFrom.Value);

            if (filter.DateTo.HasValue)
                query = query.Where(o => o.CreatedAt <= filter.DateTo.Value);

            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new
                {
                    o.InvoiceNumber,
                    CustomerName = o.Customer != null ? $"{o.Customer.FName} {o.Customer.LName}".Trim() : string.Empty,
                    CashierName = o.Cashier != null ? o.Cashier.UserName ?? o.Cashier.Email ?? string.Empty : string.Empty,
                    BranchName = o.Branch != null ? o.Branch.Name : string.Empty,
                    PaymentMethod = o.PaymentMethod.ToString(),
                    Status = o.Status.ToString(),
                    o.TotalAmount,
                    o.CreatedAt
                })
                .ToListAsync(ct);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("POS Sales");

            // Headers
            worksheet.Cell(1, 1).Value = "Invoice #";
            worksheet.Cell(1, 2).Value = "Customer";
            worksheet.Cell(1, 3).Value = "Cashier";
            worksheet.Cell(1, 4).Value = "Branch";
            worksheet.Cell(1, 5).Value = "Payment";
            worksheet.Cell(1, 6).Value = "Status";
            worksheet.Cell(1, 7).Value = "Amount";
            worksheet.Cell(1, 8).Value = "Date";

            var headerRange = worksheet.Range(1, 1, 1, 8);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

            // Data
            for (int i = 0; i < orders.Count; i++)
            {
                var row = i + 2;
                worksheet.Cell(row, 1).Value = orders[i].InvoiceNumber;
                worksheet.Cell(row, 2).Value = orders[i].CustomerName;
                worksheet.Cell(row, 3).Value = orders[i].CashierName;
                worksheet.Cell(row, 4).Value = orders[i].BranchName;
                worksheet.Cell(row, 5).Value = orders[i].PaymentMethod;
                worksheet.Cell(row, 6).Value = orders[i].Status;
                worksheet.Cell(row, 7).Value = orders[i].TotalAmount;
                worksheet.Cell(row, 8).Value = orders[i].CreatedAt;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var bytes = stream.ToArray();

            return _resultHandler.Success(bytes);
        }

        public async Task<ServiceResult<string>> GenerateAndUploadQrCodeAsync(int orderId, CancellationToken ct = default)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId, ct: ct);
            if (order == null)
                return _resultHandler.NotFound<string>("Order not found.");


            var frontendBaseUrl = _configuration["FrontendBaseUrl"];
            var qrValue = $"{frontendBaseUrl}/invoice/{order.Id}";

            var qrBytes = _qrCodeService.GenerateImage(qrValue);
            var fileName = $"invoice-qr-{order.Id}-{Guid.NewGuid()}.png";

            using var qrStream = new MemoryStream(qrBytes);
            var uploadResult = await _imageStorageService.UploadImageAsync(qrStream, fileName, ct);

            if (!uploadResult.Succeeded)
                return _resultHandler.BadRequest<string>(uploadResult.Message ?? "Failed to upload QR code.");

            order.QRCode = uploadResult.Data;
            _unitOfWork.Orders.Update(order);
            await _unitOfWork.SaveChangesAsync(ct);

            return _resultHandler.Success<string>(uploadResult.Data!);
        }
    }
}