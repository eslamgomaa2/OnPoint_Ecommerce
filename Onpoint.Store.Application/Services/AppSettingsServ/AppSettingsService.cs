using AutoMapper;
using BuildingBlocks.Results;
using Microsoft.AspNetCore.Http;
using Onpoint.Store.Application.DTOs.AppSettings;
using Onpoint.Store.Application.DTOs.Media;
using Onpoint.Store.Application.Services.MedioServices;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Repositories;

namespace Onpoint.Store.Application.Services.AppSettingsServ
{


    public class AppSettingsService : IAppSettingsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ServiceResultHandler _resultHandler;
        private readonly IMediaService _mediaService;


        public AppSettingsService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ServiceResultHandler resultHandler,
            IMediaService mediaService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _resultHandler = resultHandler;
            _mediaService = mediaService;
        }

        public async Task<ServiceResult<AppSettingsDto>> GetAsync(CancellationToken ct = default)
        {
            var settings = await _unitOfWork.AppSettings.GetCurrentAsync(ct);

            if (settings == null)
            {

                return _resultHandler.BadRequest<AppSettingsDto>("App settings not found.");
            }

            var dto = _mapper.Map<AppSettingsDto>(settings);

            if (string.IsNullOrEmpty(dto.AppLogoUrl))
            {
                dto.AppLogoUrl = string.Empty;
            }

            return _resultHandler.Success(dto);
        }

        public async Task<ServiceResult<AppSettingsDto>> CreateAsync(CreateAppSettingsDto dto, CancellationToken ct = default)
        {

            var existing = await _unitOfWork.AppSettings.GetCurrentAsync(ct);
            if (existing != null)
                return _resultHandler.BadRequest<AppSettingsDto>("App settings already exist. Use Update instead.");

            var settings = new AppSettingsInfo
            {
                AppName = dto.AppName,
                PrimaryColor = dto.PrimaryColor,
                SecondaryColor = dto.SecondaryColor,
                AccentColor = dto.AccentColor,
                DefaultLanguage = dto.DefaultLanguage,
                MaintenanceMode = dto.MaintenanceMode,
                MaintenanceMessage = dto.MaintenanceMessage
            };


            if (dto.LogoFile != null && dto.LogoFile.Length > 0)
            {
                var logoUrl = await UploadLogoAsync(dto.LogoFile, ct);
                if (!string.IsNullOrEmpty(logoUrl))
                    settings.AppLogoUrl = logoUrl;
            }
            else
            {
                settings.AppLogoUrl = string.Empty;
            }

            await _unitOfWork.AppSettings.AddAsync(settings, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return _resultHandler.Created(_mapper.Map<AppSettingsDto>(settings));
        }

        public async Task<ServiceResult<AppSettingsDto>> UpdateAsync(UpdateAppSettingsDto dto, CancellationToken ct = default)
        {
            var settings = await _unitOfWork.AppSettings.GetCurrentAsync(ct);

            if (settings == null)
            {

                var createDto = new CreateAppSettingsDto
                {
                    AppName = dto.AppName ?? "Onpoint Store",
                    LogoFile = dto.LogoFile,
                    PrimaryColor = dto.PrimaryColor ?? "#3B82F6",
                    SecondaryColor = dto.SecondaryColor,
                    AccentColor = dto.AccentColor,
                    DefaultLanguage = dto.DefaultLanguage ?? "ar",
                    MaintenanceMode = dto.MaintenanceMode ?? false,
                    MaintenanceMessage = dto.MaintenanceMessage
                };
                return await CreateAsync(createDto, ct);
            }


            if (!string.IsNullOrWhiteSpace(dto.AppName))
                settings.AppName = dto.AppName;

            if (!string.IsNullOrWhiteSpace(dto.PrimaryColor))
                settings.PrimaryColor = dto.PrimaryColor;

            if (dto.SecondaryColor != null)
                settings.SecondaryColor = dto.SecondaryColor;

            if (dto.AccentColor != null)
                settings.AccentColor = dto.AccentColor;

            if (!string.IsNullOrWhiteSpace(dto.DefaultLanguage))
                settings.DefaultLanguage = dto.DefaultLanguage;

            if (dto.MaintenanceMode.HasValue)
                settings.MaintenanceMode = dto.MaintenanceMode.Value;

            if (dto.MaintenanceMessage != null)
                settings.MaintenanceMessage = dto.MaintenanceMessage;

            // ⚠️ Upload new logo if provided
            if (dto.LogoFile != null && dto.LogoFile.Length > 0)
            {
                var logoUrl = await UploadLogoAsync(dto.LogoFile, ct);
                if (!string.IsNullOrEmpty(logoUrl))
                    settings.AppLogoUrl = logoUrl;
            }

            settings.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.AppSettings.Update(settings);
            await _unitOfWork.SaveChangesAsync(ct);

            return _resultHandler.Success(_mapper.Map<AppSettingsDto>(settings));
        }


        private async Task<string?> UploadLogoAsync(IFormFile logoFile, CancellationToken ct)
        {
            using var stream = logoFile.OpenReadStream();

            var uploadResult = await _mediaService.UploadProductImageAsync(new FileUploadDto
            {
                FileName = $"app-logo-{Guid.NewGuid()}.png",
                FileContent = stream
            }, ct);

            return uploadResult.Succeeded ? uploadResult.Data : null;
        }
    }
}