using AutoMapper;
using BuildingBlocks.Results;
using FluentValidation;
using Onpoint.Store.Application.DTOs.PaymentTransaction;
using Onpoint.Store.Domin.Repositories;

namespace Onpoint.Store.Application.Services.PaymentTransactionServ
{
    public class PaymentTransactionService : IPaymentTransactionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<UpdatePaymentStatusDto> _validator;
        private readonly ServiceResultHandler _serviceResultHandler;

        public PaymentTransactionService(IUnitOfWork unitOfWork, IMapper mapper, IValidator<UpdatePaymentStatusDto> validator, ServiceResultHandler serviceResultHandler)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _validator = validator;
            _serviceResultHandler = serviceResultHandler;
        }

        public async Task<ServiceResult<PaymentTransactionDto>> UpdateStatusAsync(int id, UpdatePaymentStatusDto dto)
        {
            await _validator.ValidateAndThrowAsync(dto);

            var transaction = await _unitOfWork.PaymentTransactions.GetByIdAsync(id);
            if (transaction == null)
                return _serviceResultHandler.NotFound<PaymentTransactionDto>("Transaction not found");

            transaction.Status = dto.Status;
            transaction.GatewayTransactionId = dto.GatewayTransactionId ?? transaction.GatewayTransactionId;
            transaction.ErrorCode = dto.ErrorCode;
            transaction.ErrorMessage = dto.ErrorMessage;
            transaction.PaidAt = dto.PaidAt;

            _unitOfWork.PaymentTransactions.Update(transaction);
            await _unitOfWork.SaveChangesAsync();

            return _serviceResultHandler.Success(_mapper.Map<PaymentTransactionDto>(transaction));
        }

        public async Task<ServiceResult<PaymentTransactionDto>> GetByOrderIdAsync(int orderId)
        {
            var transaction = await _unitOfWork.PaymentTransactions.GetByOrderIdAsync(orderId);
            if (transaction == null)
                return _serviceResultHandler.NotFound<PaymentTransactionDto>("No transaction found for this order");

            return _serviceResultHandler.Success(_mapper.Map<PaymentTransactionDto>(transaction));
        }
    }
}