using AutoMapper;
using BuildingBlocks.Results;
using FluentValidation;
using Onpoint.Store.Application.DTOs.Branch;
using Onpoint.Store.Domin.Entities.Sales.Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Repositories;
using System.Data;

namespace Onpoint.Store.Application.Services.BranchServ
{
    public class BranchService : IBranchService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateBranchDto> _createValidator;
        private readonly IValidator<UpdateBranchDto> _updateValidator;
        private readonly ServiceResultHandler _resultHandler;

        public BranchService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<CreateBranchDto> createValidator,
            IValidator<UpdateBranchDto> updateValidator,
            ServiceResultHandler resultHandler)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _resultHandler = resultHandler;
        }

        public async Task<ServiceResult<IEnumerable<BranchDto>>> GetAllAsync()
        {
            var branches = await _unitOfWork.Branches.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<BranchDto>>(branches);
            return _resultHandler.Success(dtos);
        }

        public async Task<ServiceResult<BranchDto>> GetByIdAsync(int id)
        {
            var branch = await _unitOfWork.Branches.GetByIdAsync(id);
            if (branch == null)
                return _resultHandler.NotFound<BranchDto>("Branch not found");

            var dto = _mapper.Map<BranchDto>(branch);
            return _resultHandler.Success(dto);
        }

        public async Task<ServiceResult<BranchDto>> CreateAsync(CreateBranchDto dto)
        {
            await _createValidator.ValidateAndThrowAsync(dto);

            var branch = _mapper.Map<Branch>(dto);

            await _unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                if (branch.IsDefault)
                {
                    await UnsetOtherDefaultsAsync(branch.Id);
                }

                await _unitOfWork.Branches.AddAsync(branch);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                var resultDto = _mapper.Map<BranchDto>(branch);
                return _resultHandler.Created(resultDto);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<ServiceResult<BranchDto>> UpdateAsync(int Id, UpdateBranchDto dto)
        {
            await _updateValidator.ValidateAndThrowAsync(dto);

            var branch = await _unitOfWork.Branches.GetByIdAsync(Id);
            if (branch == null)
                return _resultHandler.NotFound<BranchDto>("Branch not found");

            await _unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                _mapper.Map(dto, branch);

                if (branch.IsDefault)
                {
                    await UnsetOtherDefaultsAsync(branch.Id);
                }

                _unitOfWork.Branches.Update(branch);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                var resultDto = _mapper.Map<BranchDto>(branch);
                return _resultHandler.Success(resultDto);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<ServiceResult<bool>> ToggleActiveAsync(int id)
        {
            var branch = await _unitOfWork.Branches.GetByIdAsync(id);
            if (branch == null)
                return _resultHandler.NotFound<bool>("Branch not found");

            if (branch.IsDefault && branch.IsActive)
            {
                return _resultHandler.BadRequest<bool>("Cannot deactivate the default branch. Set another branch as default first.");
            }

            branch.IsActive = !branch.IsActive;
            _unitOfWork.Branches.Update(branch);
            await _unitOfWork.SaveChangesAsync();

            return _resultHandler.Success(true);
        }

        public async Task<ServiceResult<bool>> SetDefaultAsync(int id)
        {
            var branch = await _unitOfWork.Branches.GetByIdAsync(id);
            if (branch == null)
                return _resultHandler.NotFound<bool>("Branch not found");

            if (!branch.IsActive)
                return _resultHandler.BadRequest<bool>("Cannot set an inactive branch as default");

            if (branch.IsDefault)
                return _resultHandler.Success(true);

            await _unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {

                await UnsetOtherDefaultsAsync(branch.Id);
                await _unitOfWork.SaveChangesAsync();


                branch.IsDefault = true;
                _unitOfWork.Branches.Update(branch);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();

                return _resultHandler.Success(true);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        private async Task UnsetOtherDefaultsAsync(int exceptBranchId)
        {
            var allBranches = await _unitOfWork.Branches.GetAllAsync();
            var otherDefaults = allBranches.Where(b => b.IsDefault && b.Id != exceptBranchId);

            foreach (var b in otherDefaults)
            {
                b.IsDefault = false;
                _unitOfWork.Branches.Update(b);
            }
        }
    }
}