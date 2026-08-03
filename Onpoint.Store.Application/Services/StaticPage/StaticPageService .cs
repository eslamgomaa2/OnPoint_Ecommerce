using AutoMapper;
using BuildingBlocks.Results;
using FluentValidation;
using Ganss.Xss;
using Onpoint.Store.Application.DTOs.StaticPage;
using Onpoint.Store.Application.DTOs.StaticPages;
using Onpoint.Store.Domin.Enums;
using Onpoint.Store.Domin.Repositories;

namespace Onpoint.Store.Application.Services.StaticPage
{
    public class StaticPageService : IStaticPageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ServiceResultHandler _resultHandler;
        private readonly IValidator<CreateStaticPageDto> _createValidator;
        private readonly IValidator<UpdateStaticPageDto> _updateValidator;

        public StaticPageService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ServiceResultHandler resultHandler,
            IValidator<CreateStaticPageDto> createValidator,
            IValidator<UpdateStaticPageDto> updateValidator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _resultHandler = resultHandler;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        public async Task<ServiceResult<StaticPageReadDto>> GetByTypeAsync(PageType type, CancellationToken ct = default)
        {
            var page = await _unitOfWork.StaticPages.GetByTypeAsync(type, ct);
            if (page == null)
                return _resultHandler.NotFound<StaticPageReadDto>($"'{type}' page not found.");

            var dto = _mapper.Map<StaticPageReadDto>(page);
            return _resultHandler.Success(dto);
        }

        public async Task<ServiceResult<IEnumerable<StaticPageReadDto>>> GetAllAsync(CancellationToken ct = default)
        {
            var pages = await _unitOfWork.StaticPages.GetAllAsync(ct);
            var dtos = _mapper.Map<IEnumerable<StaticPageReadDto>>(pages);
            return _resultHandler.Success(dtos);
        }

        public async Task<ServiceResult<StaticPageReadDto>> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var page = await _unitOfWork.StaticPages.GetByIdAsync(id, ct);
            if (page == null)
                return _resultHandler.NotFound<StaticPageReadDto>("Page not found.");

            var dto = _mapper.Map<StaticPageReadDto>(page);
            return _resultHandler.Success(dto);
        }



        public async Task<ServiceResult<StaticPageReadDto>> CreateAsync(CreateStaticPageDto dto, CancellationToken ct = default)
        {
            await _createValidator.ValidateAndThrowAsync(dto, cancellationToken: ct);

            var existing = await _unitOfWork.StaticPages.GetByTypeAsync(dto.Type, ct);
            if (existing != null)
                return _resultHandler.BadRequest<StaticPageReadDto>($"A page with type '{dto.Type}' already exists.");


            var sanitizer = new HtmlSanitizer();
            dto.Content = sanitizer.Sanitize(dto.Content);

            var entity = _mapper.Map<Onpoint.Store.Domin.Entities.StaticPage>(dto);
            await _unitOfWork.StaticPages.AddAsync(entity, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            var resultDto = _mapper.Map<StaticPageReadDto>(entity);
            return _resultHandler.Created(resultDto);
        }

        public async Task<ServiceResult<bool>> UpdateAsync(int id, UpdateStaticPageDto dto, CancellationToken ct = default)
        {
            await _updateValidator.ValidateAndThrowAsync(dto, cancellationToken: ct);

            var existing = await _unitOfWork.StaticPages.GetByIdAsync(id, ct);
            if (existing == null)
                return _resultHandler.NotFound<bool>("Page not found.");

            _mapper.Map(dto, existing);
            _unitOfWork.StaticPages.Update(existing);
            await _unitOfWork.SaveChangesAsync(ct);

            return _resultHandler.Success(true);
        }

        public async Task<ServiceResult<bool>> DeleteAsync(int id, CancellationToken ct = default)
        {
            var existing = await _unitOfWork.StaticPages.GetByIdAsync(id, ct);
            if (existing == null)
                return _resultHandler.NotFound<bool>("Page not found.");

            _unitOfWork.StaticPages.Remove(existing);
            await _unitOfWork.SaveChangesAsync(ct);

            return _resultHandler.Success(true);
        }
    }
}