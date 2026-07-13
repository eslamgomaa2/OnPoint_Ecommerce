using BuildingBlocks.Results;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Onpoint.Store.Application.DTOs.Auth;
using Onpoint.Store.Application.Services.AuthServices.ExternalAuthService;
using Onpoint.Store.Application.Services.AuthServices.Token;
using Onpoint.Store.Domin.Entities;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;

namespace Onpoint.Store.Infrastructure.ExternalAuthServices
{

    public class ExternalAuthService : IExternalAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ServiceResultHandler _resultHandler;
        private readonly ILogger<ExternalAuthService> _logger;
        private readonly IConfiguration _configuration;

        public ExternalAuthService(
            UserManager<ApplicationUser> userManager,
            ITokenService tokenService,
            IHttpClientFactory httpClientFactory,
            ServiceResultHandler resultHandler,
            ILogger<ExternalAuthService> logger,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _httpClientFactory = httpClientFactory;
            _resultHandler = resultHandler;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<ServiceResult<AuthResponseDto>> ExternalLoginAsync(ExternalLoginDto dto, CancellationToken ct = default)
        {
            ExternalUserInfo? userInfo = null;

            if (dto.Provider.Equals("Google", StringComparison.OrdinalIgnoreCase))
                userInfo = await VerifyGoogleTokenAsync(dto.IdToken);
            else if (dto.Provider.Equals("Facebook", StringComparison.OrdinalIgnoreCase))
                userInfo = await VerifyFacebookTokenAsync(dto.IdToken);
            else if (dto.Provider.Equals("Apple", StringComparison.OrdinalIgnoreCase))
                throw new NotImplementedException("Apple login requires specific JWT validation logic.");


            if (userInfo == null || string.IsNullOrEmpty(userInfo.Email))
                throw new Exception("Failed to retrieve valid user info from the provider.");


            var user = await _userManager.FindByEmailAsync(userInfo.Email);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = userInfo.Email,
                    Email = userInfo.Email,
                    FirstName = userInfo.FirstName ?? "Unknown",
                    LastName = userInfo.LastName ?? "User",
                    EmailConfirmed = true
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    throw new Exception($"Failed to create external user: {errors}");
                }


                await _userManager.AddToRoleAsync(user, "Customer");
            }


            var roles = await _userManager.GetRolesAsync(user);
            var roleList = roles.Count > 0 ? roles.ToList() : new List<string> { "Customer" };
            var tokenData = _tokenService.GenerateToken(user, roleList);

            return _resultHandler.Success(new AuthResponseDto
            {
                Token = tokenData.token,
                Expiration = tokenData.expiresAt,
                UserId = user.Id,
                Email = user.Email!,
                FullName = $"{user.FirstName} {user.LastName}",
                Role = roleList.First()
            });
        }



        private async Task<ExternalUserInfo?> VerifyGoogleTokenAsync(string idToken)
        {
            var googleClientId = _configuration["GoogleAuth:ClientId"];
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new[] { googleClientId }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

                return new ExternalUserInfo
                {
                    Email = payload.Email,
                    FirstName = payload.GivenName,
                    LastName = payload.FamilyName,
                    ProviderKey = payload.Subject
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Invalid Google token.");
                throw new ValidationException("Invalid Google token.");
            }
        }

        private async Task<ExternalUserInfo?> VerifyFacebookTokenAsync(string accessToken)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetFromJsonAsync<FacebookTokenValidationResponse>(
                    $"https://graph.facebook.com/me?access_token={accessToken}&fields=email,first_name,last_name");

                if (response == null || string.IsNullOrEmpty(response.Id))
                    throw new ValidationException("Invalid Facebook token.");

                return new ExternalUserInfo
                {
                    Email = response.Email,
                    FirstName = response.FirstName,
                    LastName = response.LastName,
                    ProviderKey = response.Id
                };
            }
            catch (ValidationException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Invalid Facebook token.");
                throw new ValidationException("Invalid Facebook token.");
            }
        }
    }


    internal class ExternalUserInfo
    {
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string ProviderKey { get; set; } = string.Empty;
    }

    internal class FacebookTokenValidationResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }
}

