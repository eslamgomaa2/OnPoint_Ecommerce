using BuildingBlocks.Results;
using Onpoint.Store.Application.DTOs.StoreSettings;
using Onpoint.Store.Domain.Repositories;

namespace Onpoint.Store.Application.Services.StoreSettings
{
    public class StoreSettingsService : IStoreSettingsService
    {
        private readonly IStoreSettingsRepository _repository;
        private readonly ServiceResultHandler _resultHandler;

        public StoreSettingsService(IStoreSettingsRepository repository, ServiceResultHandler resultHandler)
        {
            _repository = repository;
            _resultHandler = resultHandler;
        }

        public async Task<ServiceResult<List<StoreSettingDto>>> GetAsync(CancellationToken ct = default)
        {
            var settingsList = await _repository.GetAllAsync(ct);

            var response = settingsList?.Select(s => new StoreSettingDto
            {
                Id = s.Id,
                Key = s.Key,
                Value = s.Value
            }).ToList() ?? new List<StoreSettingDto>();

            return _resultHandler.Success(response);
        }

        public async Task<ServiceResult<StoreSettingsUpdateDto>> CreateAsync(StoreSettingsUpdateDto dto, CancellationToken ct = default)
        {
            return await SaveSettingsAsync(dto, ct, "Store settings created successfully.");
        }

        // ── UPDATE BY ID ─────────────────────────────────────────────
        public async Task<ServiceResult<StoreSettingDto>> UpdateAsync(int id, StoreSettingItemDto dto, CancellationToken ct = default)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity is null)
            {
                return _resultHandler.NotFound<StoreSettingDto>("Store setting not found.");
            }

            if (dto is null || (string.IsNullOrWhiteSpace(dto.Key) && string.IsNullOrWhiteSpace(dto.Value)))
            {
                return _resultHandler.BadRequest<StoreSettingDto>("No data provided to update.");
            }

            if (!string.IsNullOrWhiteSpace(dto.Key))
            {
                entity.Key = dto.Key;
            }

            entity.Value = dto.Value ?? string.Empty;

            _repository.Update(entity);
            await _repository.SaveChangesAsync(ct);

            var response = new StoreSettingDto
            {
                Id = entity.Id,
                Key = entity.Key,
                Value = entity.Value
            };

            return _resultHandler.Success(response, "Store setting updated successfully.");
        }

        // ── DELETE BY ID ─────────────────────────────────────────────
        public async Task<ServiceResult<bool>> DeleteAsync(int id, CancellationToken ct = default)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity is null)
            {
                return _resultHandler.NotFound<bool>("Store setting not found.");
            }

            _repository.Remove(entity);
            await _repository.SaveChangesAsync(ct);

            return _resultHandler.Success(true, "Store setting deleted successfully.");
        }

        // ── BULK CREATE / UPSERT (unchanged) ──────────────────────────
        private async Task<ServiceResult<StoreSettingsUpdateDto>> SaveSettingsAsync(
            StoreSettingsUpdateDto dto, CancellationToken ct, string successMessage)
        {
            if (dto.Settings is null || !dto.Settings.Any())
            {
                return _resultHandler.Success(dto, successMessage);
            }

            var existingList = await _repository.GetAllAsync(ct);
            var existingDict = existingList.ToDictionary(s => s.Key, s => s, StringComparer.OrdinalIgnoreCase);

            var entitiesToSave = new List<global::Onpoint.Store.Domin.Entities.StoreSettings>();
            var entitiesToUpdate = new List<global::Onpoint.Store.Domin.Entities.StoreSettings>();

            foreach (var kvp in dto.Settings)
            {
                var key = kvp.Key;
                var value = kvp.Value ?? string.Empty;

                if (existingDict.TryGetValue(key, out var existingEntity))
                {
                    existingEntity.Value = value;
                    entitiesToUpdate.Add(existingEntity);
                }
                else
                {
                    entitiesToSave.Add(new global::Onpoint.Store.Domin.Entities.StoreSettings
                    {
                        Key = key,
                        Value = value
                    });
                }
            }

            if (entitiesToSave.Any())
                await _repository.AddRangeAsync(entitiesToSave, ct);

            if (entitiesToUpdate.Any())
                await _repository.UpdateRangeAsync(entitiesToUpdate, ct);

            return _resultHandler.Success(dto, successMessage);
        }
    }
}