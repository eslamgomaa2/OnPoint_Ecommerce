using BuildingBlocks.Results;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Onpoint.Store.Application.DTOs.Auth;
using Onpoint.Store.Application.DTOs.Branch;
using Onpoint.Store.Application.Services.BranchServ;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Enums;
using Onpoint.Store.Domin.Repositories;

namespace Onpoint.Store.Application.Services.BranchServices
{
    public class BranchService : IBranchService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<ApplicationUser, int> _userRepo;

        private readonly IValidator<CreateBranchDto> _createBranchValidator;
        private readonly IValidator<UpdateBranchDto> _UpdateBranchValidator;
        private readonly IValidator<CreateManagerDto> _createManagerValidator;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly ServiceResultHandler _resultHandler;

        public BranchService(

            IGenericRepository<ApplicationUser, int> userRepo,

            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<int>> roleManager,
            ServiceResultHandler resultHandler,
            IUnitOfWork unitOfWork,
            IValidator<CreateBranchDto> createBranchValidator,
            IValidator<CreateManagerDto> createManagerValidator,
            IValidator<UpdateBranchDto> updateBranchValidator)
        {

            _userRepo = userRepo;

            _userManager = userManager;
            _roleManager = roleManager;
            _resultHandler = resultHandler;
            _unitOfWork = unitOfWork;
            _createBranchValidator = createBranchValidator;

            _createManagerValidator = createManagerValidator;
            _UpdateBranchValidator = updateBranchValidator;
        }

        public async Task<ServiceResult<PagedResult<BranchDto>>> GetPagedAsync(BranchPagedRequestDto request, CancellationToken ct = default)
        {
            var query = _unitOfWork.Branches.QueryNoTracking()
        .Include(b => b.Manager)
        .Include(b => b.Cashiers.Where(c => !c.IsDeleted && c.BranchRole == UserBranchRole.Cashier))
        .Where(b => !b.IsDeleted);

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.Trim().ToLower();
                query = query.Where(b =>
                    (b.Name != null && b.Name.ToLower().Contains(term)) ||
                    (b.Address != null && b.Address.ToLower().Contains(term)) ||
                    (b.Phone != null && b.Phone.ToLower().Contains(term)) ||
                    (b.Manager != null && (
                        (b.Manager.FirstName != null && b.Manager.FirstName.ToLower().Contains(term)) ||
                        (b.Manager.LastName != null && b.Manager.LastName.ToLower().Contains(term))
                    ))
                );
            }

            // Status Filter
            if (request.StatusFilter.HasValue)
            {
                query = request.StatusFilter.Value switch
                {
                    StatusFilter.Active => query.Where(b => b.IsActive),
                    StatusFilter.Inactive => query.Where(b => !b.IsActive),
                    _ => query
                };
            }

            // Default Filter
            if (request.IsDefaultFilter.HasValue)
            {
                query = query.Where(b => b.IsDefault == request.IsDefaultFilter.Value);
            }

            var totalCount = await query.CountAsync(ct);


            query = request.SortBy switch
            {
                SortColumn.CashierCount => request.SortDirection == SortDirection.Desc
                    ? query.OrderByDescending(b => b.Cashiers.Count)
                    : query.OrderBy(b => b.Cashiers.Count),
                SortColumn.Status => request.SortDirection == SortDirection.Desc
                    ? query.OrderByDescending(b => b.IsActive)
                    : query.OrderBy(b => b.IsActive),
                _ => request.SortDirection == SortDirection.Desc
                    ? query.OrderByDescending(b => b.Name)
                    : query.OrderBy(b => b.Name)
            };

            var items = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(ct);

            var dtos = items.Select(MapToDto).ToList();
            var pagedResult = PagedResult<BranchDto>.Create(dtos, totalCount, request.PageNumber, request.PageSize);

            return _resultHandler.Success(pagedResult);
        }

        public async Task<ServiceResult<BranchDto>> GetByIdAsync(int id, CancellationToken ct = default)
        {

            var branch = await _unitOfWork.Branches.QueryNoTracking()
        .Include(b => b.Manager)
        .Include(b => b.Cashiers.Where(c => !c.IsDeleted && c.BranchRole == UserBranchRole.Cashier))
        .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted, ct);

            if (branch == null)
                return _resultHandler.NotFound<BranchDto>("Branch not found.");

            return _resultHandler.Success(MapToDto(branch));
        }

        public async Task<ServiceResult<BranchDto>> CreateAsync(CreateBranchDto dto, CancellationToken ct = default)
        {
            await _createBranchValidator.ValidateAndThrowAsync(dto, cancellationToken: ct);

            var branch = new Branch
            {
                Name = dto.Name,
                Address = dto.Address,
                Phone = dto.Phone,
                Email = dto.Email,
                GoogleMapLocation = dto.GoogleMapLocation,
                WorkingHours = dto.WorkingHours,
                IsActive = dto.IsActive,
                IsDefault = dto.IsDefault,

            };

            await _unitOfWork.Branches.AddAsync(branch, ct);

            if (dto.IsDefault)
            {
                await UnsetOtherDefaultsAsync(branch.Id, ct);
            }
            await _unitOfWork.SaveChangesAsync();

            return _resultHandler.Created(MapToDto(branch));
        }

        public async Task<ServiceResult<BranchDto>> UpdateAsync(int id, UpdateBranchDto dto, CancellationToken ct = default)
        {
            await _UpdateBranchValidator.ValidateAndThrowAsync(dto, ct);
            var branch = await _unitOfWork.Branches.Query()
        .Include(b => b.Manager)
        .Include(b => b.Cashiers.Where(c => !c.IsDeleted && c.BranchRole == UserBranchRole.Cashier))
        .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted, ct);

            if (branch == null)
                return _resultHandler.NotFound<BranchDto>("Branch not found.");



            branch.Name = dto.Name;
            branch.Address = dto.Address;
            branch.Phone = dto.Phone;
            branch.Email = dto.Email;
            branch.GoogleMapLocation = dto.GoogleMapLocation;
            branch.WorkingHours = dto.WorkingHours;
            branch.IsActive = dto.IsActive;
            branch.UpdatedAt = DateTime.UtcNow;

            if (dto.IsDefault && !branch.IsDefault)
            {
                branch.IsDefault = true;
                await UnsetOtherDefaultsAsync(branch.Id, ct);
            }
            else
            {
                branch.IsDefault = dto.IsDefault;
            }

            _unitOfWork.Branches.Update(branch);
            await _unitOfWork.SaveChangesAsync();
            return _resultHandler.Success(MapToDto(branch));
        }

        public async Task<ServiceResult<bool>> DeleteAsync(int id, CancellationToken ct = default)
        {
            var branch = await _unitOfWork.Branches.QueryNoTracking()
         .Include(b => b.Manager)
         .Include(b => b.Cashiers.Where(c => !c.IsDeleted && c.BranchRole == UserBranchRole.Cashier))
         .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted, ct);

            if (branch == null)
                return _resultHandler.NotFound<bool>("Branch not found.");

            if (branch.Cashiers.Any())
                return _resultHandler.BadRequest<bool>("Cannot delete branch with active cashiers. Please remove or reassign them first.");

            var hasOrders = await _unitOfWork.Orders.AnyAsync(o => o.BranchId == id, ct);
            if (hasOrders)
                return _resultHandler.BadRequest<bool>("Cannot delete branch with existing orders.");

            branch.IsDeleted = true;
            branch.IsActive = true;
            branch.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Branches.Update(branch);

            await _unitOfWork.SaveChangesAsync();

            return _resultHandler.Deleted<bool>("Branch deleted successfully.");
        }

        public async Task<ServiceResult<bool>> ToggleActiveAsync(int id, CancellationToken ct = default)
        {
            var branch = await _unitOfWork.Branches.Query()
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted, ct);

            if (branch == null)
                return _resultHandler.NotFound<bool>("Branch not found.");

            branch.IsActive = !branch.IsActive;
            branch.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Branches.Update(branch);

            await _unitOfWork.SaveChangesAsync();

            return _resultHandler.Success(true);
        }

        public async Task<ServiceResult<bool>> SetDefaultAsync(int id, CancellationToken ct = default)
        {
            var branch = await _unitOfWork.Branches.Query()
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted, ct);

            if (branch == null)
                return _resultHandler.NotFound<bool>("Branch not found.");

            if (!branch.IsActive)
                return _resultHandler.BadRequest<bool>("Cannot set an inactive branch as default.");

            branch.IsDefault = true;
            branch.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Branches.Update(branch);

            await UnsetOtherDefaultsAsync(branch.Id, ct);

            await _unitOfWork.SaveChangesAsync();

            return _resultHandler.Success(true);
        }

        public async Task<ServiceResult<BranchDto>> AssignManagerAsync(int branchId, CreateManagerDto dto, CancellationToken ct = default)
        {
            await _createManagerValidator.ValidateAndThrowAsync(dto, cancellationToken: ct);
            var branch = await _unitOfWork.Branches.Query()
                  .Include(b => b.Manager)
                   .Include(b => b.Cashiers.Where(c => !c.IsDeleted))
                    .FirstOrDefaultAsync(b => b.Id == branchId && !b.IsDeleted, ct);

            if (branch == null)
                return _resultHandler.NotFound<BranchDto>("Branch not found.");

            var emailExists = await _userManager.FindByEmailAsync(dto.Email);
            if (emailExists != null)
                return _resultHandler.BadRequest<BranchDto>("Email is already registered.");

            if (!await _roleManager.RoleExistsAsync("BranchManager"))
                await _roleManager.CreateAsync(new IdentityRole<int>("BranchManager"));
            var user = new ApplicationUser
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                UserName = dto.Email,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                BranchId = branchId,
                BranchRole = UserBranchRole.Manager,
                EmailConfirmed = true,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return _resultHandler.BadRequest<BranchDto>($"Failed to create manager: {errors}");
            }

            await _userManager.AddToRoleAsync(user, "BranchManager");

            branch.ManagerId = user.Id;
            branch.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Branches.Update(branch);

            await _unitOfWork.SaveChangesAsync();

            return _resultHandler.Success(MapToDto(branch));
        }

        // ===================== REMOVE MANAGER =====================
        public async Task<ServiceResult<bool>> RemoveManagerAsync(int branchId, CancellationToken ct = default)
        {
            var branch = await _unitOfWork.Branches.Query()
                .Include(b => b.Manager)
                .FirstOrDefaultAsync(b => b.Id == branchId && !b.IsDeleted, ct);

            if (branch == null)
                return _resultHandler.NotFound<bool>("Branch not found.");

            if (!branch.ManagerId.HasValue)
                return _resultHandler.BadRequest<bool>("This branch has no manager assigned.");

            var manager = await _userRepo.GetByIdAsync(branch.ManagerId.Value, ct);
            if (manager == null)
                return _resultHandler.NotFound<bool>("Manager not found.");



            _userRepo.Remove(manager);

            branch.ManagerId = null;
            branch.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Branches.Update(branch);

            await _unitOfWork.SaveChangesAsync();
            return _resultHandler.Success(true);
        }

        // ===================== HELPERS =====================
        private async Task UnsetOtherDefaultsAsync(int exceptBranchId, CancellationToken ct)
        {
            var otherDefaults = await _unitOfWork.Branches.FindAsync(
                b => b.IsDefault && b.Id != exceptBranchId && !b.IsDeleted,
                ct: ct);

            foreach (var b in otherDefaults)
            {
                b.IsDefault = false;
                b.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Branches.Update(b);
            }
        }

        private static BranchDto MapToDto(Branch branch)
        {
            return new BranchDto
            {
                Id = branch.Id,
                Name = branch.Name,
                Address = branch.Address,
                Phone = branch.Phone,
                Email = branch.Email,
                GoogleMapLocation = branch.GoogleMapLocation,
                WorkingHours = branch.WorkingHours,
                IsActive = branch.IsActive,
                IsDefault = branch.IsDefault,
                IsDeleted = branch.IsDeleted,
                ManagerId = branch.ManagerId,
                ManagerName = branch.Manager != null
                    ? $"{branch.Manager.FirstName} {branch.Manager.LastName}".Trim()
                    : null,
                CashierCount = branch.Cashiers?.Count ?? 0
            };
        }
    }
}