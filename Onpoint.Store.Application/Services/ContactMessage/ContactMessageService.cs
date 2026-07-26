using AutoMapper;
using BuildingBlocks.Results;
using FluentValidation;
using Onpoint.Store.Application.DTOs.Contactmessage;
using Onpoint.Store.Application.Services.ContactMessage;
using Onpoint.Store.Domin.Repositories;

namespace Onpoint.Store.Application.Services
{
    public class ContactMessageService : IContactMessageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ServiceResultHandler _serviceResultHandler;
        private readonly IValidator<ContactMessageCreateDto> _createContactMessageValidator;
        private readonly IValidator<ContactMessageUpdateDto> _updateContactMessageValidator;

        public ContactMessageService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ServiceResultHandler serviceResultHandler,
            IValidator<ContactMessageCreateDto> createContactMessageValidator,
            IValidator<ContactMessageUpdateDto> updateContactMessageValidator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _serviceResultHandler = serviceResultHandler;
            _createContactMessageValidator = createContactMessageValidator;
            _updateContactMessageValidator = updateContactMessageValidator;
        }

        public async Task<ServiceResult<IEnumerable<ContactMessageReadDto>>> GetAllAsync(CancellationToken ct = default)
        {
            var messages = await _unitOfWork.ContactMessages.GetAllAsync(ct);
            var dtos = _mapper.Map<IEnumerable<ContactMessageReadDto>>(messages);
            return _serviceResultHandler.Success(dtos);
        }

        public async Task<ServiceResult<ContactMessageReadDto>> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var message = await _unitOfWork.ContactMessages.GetByIdAsync(id, ct);
            if (message == null)
                return _serviceResultHandler.NotFound<ContactMessageReadDto>("Contact message not found.");

            var dto = _mapper.Map<ContactMessageReadDto>(message);
            return _serviceResultHandler.Success(dto);
        }

        public async Task<ServiceResult<IEnumerable<ContactMessageReadDto>>> GetUnresolvedAsync(CancellationToken ct = default)
        {
            var messages = await _unitOfWork.ContactMessages.GetUnresolvedAsync();
            var dtos = _mapper.Map<IEnumerable<ContactMessageReadDto>>(messages);
            return _serviceResultHandler.Success(dtos);
        }

        public async Task<ServiceResult<ContactMessageReadDto>> CreateAsync(ContactMessageCreateDto dto, CancellationToken ct = default)
        {
            await _createContactMessageValidator.ValidateAndThrowAsync(dto, cancellationToken: ct);

            var entity = _mapper.Map<Onpoint.Store.Domin.Entities.ContactMessage>(dto);
            await _unitOfWork.ContactMessages.AddAsync(entity, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            var resultDto = _mapper.Map<ContactMessageReadDto>(entity);
            return _serviceResultHandler.Created(resultDto);
        }

        public async Task<ServiceResult<bool>> UpdateAsync(int id, ContactMessageUpdateDto dto, CancellationToken ct = default)
        {
            await _updateContactMessageValidator.ValidateAndThrowAsync(dto, cancellationToken: ct);

            var existing = await _unitOfWork.ContactMessages.GetByIdAsync(id, ct);
            if (existing == null)
                return _serviceResultHandler.NotFound<bool>("Contact message not found.");

            _mapper.Map(dto, existing);
            _unitOfWork.ContactMessages.Update(existing);
            await _unitOfWork.SaveChangesAsync(ct);

            return _serviceResultHandler.Success(true);
        }

        public async Task<ServiceResult<bool>> DeleteAsync(int id, CancellationToken ct = default)
        {
            var existing = await _unitOfWork.ContactMessages.GetByIdAsync(id, ct);
            if (existing == null)
                return _serviceResultHandler.NotFound<bool>("Contact message not found.");

            _unitOfWork.ContactMessages.Remove(existing);
            await _unitOfWork.SaveChangesAsync(ct);

            return _serviceResultHandler.Success(true);
        }
    }
}