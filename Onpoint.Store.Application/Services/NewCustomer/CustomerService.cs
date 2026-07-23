using AutoMapper;
using BuildingBlocks.Results;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Onpoint.Store.Application.DTOs.Customer;
using Onpoint.Store.Domin.Enums;
using Onpoint.Store.Domin.Repositories;
using CustomerEntity = Onpoint.Store.Domin.Entities.Identity.Customer;

namespace Onpoint.Store.Application.Services.Customer
{
    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateCustomerDto> _createValidator;
        private readonly IValidator<UpdateCustomerDto> _updateValidator;
        private readonly ServiceResultHandler _resultHandler;

        public CustomerService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<CreateCustomerDto> createValidator,
            IValidator<UpdateCustomerDto> updateValidator,
            ServiceResultHandler resultHandler)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _resultHandler = resultHandler;
        }

        public async Task<ServiceResult<PagedResult<CustomerListItemDto>>> GetAllAsync(
            int branchId,
            string? search,
            bool? isActive,
            int pageNumber,
            int pageSize,
            CancellationToken ct = default)
        {
            var (items, totalCount) = await _unitOfWork.Customers.GetPagedAsync(branchId, search, isActive, pageNumber, pageSize, ct);

            var dtos = _mapper.Map<IReadOnlyList<CustomerListItemDto>>(items);
            var result = PagedResult<CustomerListItemDto>.Create(dtos, totalCount, pageNumber, pageSize);

            return _resultHandler.Success(result);
        }

        public async Task<ServiceResult<CustomerDetailsDto>> GetByIdAsync(int id, int branchId, CancellationToken ct = default)
        {
            var customer = await _unitOfWork.Customers.GetByIdWithOrdersAsync(id, branchId, ct);
            if (customer is null)
                return _resultHandler.NotFound<CustomerDetailsDto>("Customer not found.");

            return _resultHandler.Success(_mapper.Map<CustomerDetailsDto>(customer));
        }

        public async Task<ServiceResult<CustomerStatsDto>> GetStatsAsync(int branchId, CancellationToken ct = default)
        {
            var (totalCustomers, active, newThisMonth) = await _unitOfWork.Customers.GetCustomerCountsAsync(branchId, ct);

            var totalRevenue = await _unitOfWork.Orders.QueryNoTracking()
                .Where(o => o.BranchId == branchId && o.Status == OrderStatus.Completed)
                .SumAsync(o => (decimal?)o.TotalAmount, ct) ?? 0;

            var dto = new CustomerStatsDto
            {
                TotalCustomers = totalCustomers,
                Active = active,
                NewThisMonth = newThisMonth,
                TotalRevenue = totalRevenue
            };

            return _resultHandler.Success(dto);
        }

        public async Task<ServiceResult<CustomerDetailsDto>> CreateAsync(CreateCustomerDto dto, int branchId, CancellationToken ct = default)
        {
            await _createValidator.ValidateAndThrowAsync(dto, cancellationToken: ct);

            var customer = _mapper.Map<CustomerEntity>(dto);
            customer.BranchId = branchId;

            await _unitOfWork.Customers.AddAsync(customer, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            var result = _mapper.Map<CustomerDetailsDto>(customer);
            return _resultHandler.Created(result);
        }

        public async Task<ServiceResult<CustomerDetailsDto>> UpdateAsync(int id, UpdateCustomerDto dto, int branchId, CancellationToken ct = default)
        {
            await _updateValidator.ValidateAndThrowAsync(dto, cancellationToken: ct);

            var customer = await _unitOfWork.Customers.GetByIdAsync(id, ct);
            if (customer is null || customer.BranchId != branchId || customer.IsDeleted)
                return _resultHandler.NotFound<CustomerDetailsDto>("Customer not found.");

            _mapper.Map(dto, customer);
            customer.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Customers.Update(customer);
            await _unitOfWork.SaveChangesAsync(ct);

            var updated = await _unitOfWork.Customers.GetByIdWithOrdersAsync(id, branchId, ct);
            return _resultHandler.Success(_mapper.Map<CustomerDetailsDto>(updated));
        }

        public async Task<ServiceResult<bool>> DeleteAsync(int id, int branchId, CancellationToken ct = default)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(id, ct);
            if (customer is null || customer.BranchId != branchId)
                return _resultHandler.NotFound<bool>("Customer not found.");

            _unitOfWork.Customers.Remove(customer);
            await _unitOfWork.SaveChangesAsync(ct);

            return _resultHandler.Deleted<bool>();
        }
    }
}