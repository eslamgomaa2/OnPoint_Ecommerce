using AutoMapper;
using BuildingBlocks.Results;
using FluentValidation;
using Onpoint.Store.Application.DTOs.Address;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Repositories;

namespace Onpoint.Store.Application.Services.AddressServ
{
    public class AddressService : IAddressService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ServiceResultHandler _resultHandler;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateAddressDto> _createValidator;
        private readonly IValidator<UpdateAddressDto> _updateValidator;

        public AddressService(
            IUnitOfWork unitOfWork,
            ServiceResultHandler resultHandler,
            IMapper mapper,
            IValidator<CreateAddressDto> createValidator,
            IValidator<UpdateAddressDto> updateValidator)
        {
            _unitOfWork = unitOfWork;
            _resultHandler = resultHandler;
            _mapper = mapper;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        public async Task<ServiceResult<IReadOnlyList<AddressDto>>> GetUserAddressesAsync(int userId, CancellationToken ct = default)
        {
            var addresses = await _unitOfWork.Addresses.GetUserAddressesAsync(userId, ct);
            return _resultHandler.Success(_mapper.Map<IReadOnlyList<AddressDto>>(addresses));
        }

        public async Task<ServiceResult<AddressDto>> CreateAsync(int userId, CreateAddressDto dto, CancellationToken ct = default)
        {
            var validation = await _createValidator.ValidateAsync(dto, ct);
            if (!validation.IsValid) throw new ValidationException(validation.Errors);

            if (dto.IsDefault)
            {
                await RemoveDefaultFlagAsync(userId, ct);
            }

            var address = _mapper.Map<Address>(dto);
            address.UserId = userId;

            await _unitOfWork.Addresses.AddAsync(address, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return _resultHandler.Created(_mapper.Map<AddressDto>(address));
        }

        public async Task<ServiceResult<AddressDto>> UpdateAsync(int userId, int addressId, UpdateAddressDto dto, CancellationToken ct = default)
        {
            var validation = await _updateValidator.ValidateAsync(dto, ct);
            if (!validation.IsValid) throw new ValidationException(validation.Errors);

            var existingAddress = await _unitOfWork.Addresses.GetByIdAsync(addressId, ct);

            if (existingAddress == null || existingAddress.UserId != userId)
                return _resultHandler.NotFound<AddressDto>("Address not found.");

            bool wasDefault = existingAddress.IsDefault;

            if (dto.IsDefault && !wasDefault)
            {
                await RemoveDefaultFlagAsync(userId, ct);
            }

            _mapper.Map(dto, existingAddress);

            if (wasDefault && !dto.IsDefault)
            {
                existingAddress.IsDefault = true;
            }

            _unitOfWork.Addresses.Update(existingAddress);
            await _unitOfWork.SaveChangesAsync(ct);

            return _resultHandler.Success(_mapper.Map<AddressDto>(existingAddress));
        }

        public async Task<ServiceResult<string>> DeleteAsync(int userId, int addressId, CancellationToken ct = default)
        {
            var address = await _unitOfWork.Addresses.GetByIdAsync(addressId, ct);
            if (address == null || address.UserId != userId)
                return _resultHandler.NotFound<string>("Address not found.");

            bool wasDefault = address.IsDefault;

            address.IsDeleted = true;
            _unitOfWork.Addresses.Update(address);
            await _unitOfWork.SaveChangesAsync(ct);

            if (wasDefault)
            {
                await AssignNewDefaultAsync(userId, excludeAddressId: addressId, ct);
            }

            return _resultHandler.Deleted<string>();
        }

        private async Task RemoveDefaultFlagAsync(int userId, CancellationToken ct = default)
        {
            var currentDefault = await _unitOfWork.Addresses.GetUserDefaultAddressAsync(userId, ct);
            if (currentDefault != null)
            {
                currentDefault.IsDefault = false;
                _unitOfWork.Addresses.Update(currentDefault);
            }
        }

        private async Task AssignNewDefaultAsync(int userId, int excludeAddressId, CancellationToken ct = default)
        {
            var remaining = await _unitOfWork.Addresses.GetUserAddressesAsync(userId, ct);
            var nextDefault = remaining
                .Where(a => a.Id != excludeAddressId && !a.IsDeleted)
                .OrderBy(a => a.Id)
                .FirstOrDefault();

            if (nextDefault != null)
            {
                nextDefault.IsDefault = true;
                _unitOfWork.Addresses.Update(nextDefault);
                await _unitOfWork.SaveChangesAsync(ct);
            }
        }
    }
}