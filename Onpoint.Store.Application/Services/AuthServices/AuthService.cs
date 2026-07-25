using BuildingBlocks.Results;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Onpoint.Store.Application.DTOs.Auth;
using Onpoint.Store.Application.Services.AuthServices.Email;
using Onpoint.Store.Application.Services.AuthServices.Otp;
using Onpoint.Store.Application.Services.AuthServices.Token;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Enums;
using System.Net;

namespace Onpoint.Store.Application.Services.AuthServices
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AuthService> _logger;
        private readonly ITokenService _tokenService;
        private readonly IValidator<RegisterDto> _registerValidator;
        private readonly IValidator<LoginDto> _loginValidator;
        private readonly IValidator<ForgotPasswordDto> _forgetpasswordvalidator;
        private readonly IValidator<ResendOtpDto> _resendotpvalidator;
        private readonly IValidator<ResetPasswordDto> _resetpasswordvalidator;
        private readonly IValidator<ConfirmEmailDto> _confirmEmailValidator;
        private readonly ServiceResultHandler _resultHandler;
        private readonly IOtpService _otpService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IValidator<RegisterDto> registerValidator,
            IValidator<LoginDto> loginValidator,
            ServiceResultHandler resultHandler,
            ITokenService tokenService,
            RoleManager<IdentityRole<int>> roleManager,
            ILogger<AuthService> logger,
            IOtpService otpService,
            IEmailService emailService,
            IValidator<ForgotPasswordDto> forgetpasswordvalidator,
            IValidator<ResendOtpDto> resendotpvalidator,
            IValidator<ResetPasswordDto> resetpasswordvalidator,
            IConfiguration configuration,
            IValidator<ConfirmEmailDto> confirmEmailValidator)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _registerValidator = registerValidator;
            _loginValidator = loginValidator;
            _resultHandler = resultHandler;
            _tokenService = tokenService;
            _roleManager = roleManager;
            _logger = logger;
            _otpService = otpService;
            _emailService = emailService;
            _forgetpasswordvalidator = forgetpasswordvalidator;
            _resendotpvalidator = resendotpvalidator;
            _resetpasswordvalidator = resetpasswordvalidator;
            _configuration = configuration;
            _confirmEmailValidator = confirmEmailValidator;
        }

        public async Task<ServiceResult<string>> RegisterAsync(RegisterDto dto, CancellationToken ct = default)
        {
            var validation = await _registerValidator.ValidateAsync(dto, ct);
            if (!validation.IsValid) throw new ValidationException(validation.Errors);

            var existingUser = await _userManager.Users
    .IgnoreQueryFilters()
    .FirstOrDefaultAsync(u => u.NormalizedEmail == dto.Email.ToUpperInvariant(), ct);
            if (existingUser != null)
                return _resultHandler.BadRequest<string>("Email is already registered.");

            await EnsureRoleExistsAsync("Customer");

            var user = new ApplicationUser
            {
                FirstName = dto.FName,
                LastName = dto.LName,
                UserName = $"{dto.FName}_{dto.LName}",
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                EmailConfirmed = false
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return _resultHandler.BadRequest<string>(errors);
            }

            await _userManager.AddToRoleAsync(user, "Customer");

            await GenerateOtpAndSendEmailAsync(user, "Onpoint Store - Verify Your Email", OtpPurpose.EmailVerification);

            return _resultHandler.Success<string>("Registration successful. Please check your email to verify your account.");
        }

        public async Task<ServiceResult<AuthResponseDto>> LoginAsync(LoginDto dto, CancellationToken ct = default)
        {
            try
            {
                // ─── 500: Check cancellation/timeout أول حاجة ───
                ct.ThrowIfCancellationRequested();

                // ─── 400: Validation errors ───
                var validation = await _loginValidator.ValidateAsync(dto, ct);
                if (!validation.IsValid)
                {
                    var errors = validation.Errors.Select(e => e.ErrorMessage).ToList();
                    return _resultHandler.BadRequest<AuthResponseDto>("Validation failed", errors);
                }


                var user = await _userManager.FindByEmailAsync(dto.Email);
                if (user == null)
                    return _resultHandler.BadRequest<AuthResponseDto>("Incorrect email address or password");


                if (user.IsDeleted)
                    return _resultHandler.Unauthorized<AuthResponseDto>("This account no longer exists.");


                if (user.BranchId.HasValue && user.BranchRole == null)
                    return _resultHandler.Forbidden<AuthResponseDto>("Access denied. No branch role assigned.");


                if (!user.EmailConfirmed)
                    return _resultHandler.AccountInactive<AuthResponseDto>("You must verify your account before logging in.");


                if (!user.IsActive)
                    return _resultHandler.AccountInactive<AuthResponseDto>("This account is inactive. Please contact support.");


                if (await _userManager.IsLockedOutAsync(user))
                {
                    var message = user.LockoutEscalationLevel >= 1
                        ? "The account is temporarily locked for 24 hours due to repeated incorrect login attempts. Please try again later."
                        : "The account is temporarily locked for 1 hour due to repeated incorrect login attempts. Please try again later.";

                    return _resultHandler.BadRequest<AuthResponseDto>(message);
                }


                var passwordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
                if (!passwordValid)
                {
                    await _userManager.AccessFailedAsync(user);

                    if (await _userManager.IsLockedOutAsync(user))
                    {
                        TimeSpan lockoutDuration;

                        if (user.LockoutEscalationLevel == 0)
                        {
                            lockoutDuration = TimeSpan.FromHours(1);
                            user.LockoutEscalationLevel = 1;
                        }
                        else
                        {
                            lockoutDuration = TimeSpan.FromHours(24);
                        }

                        await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.Add(lockoutDuration));
                        await _userManager.UpdateAsync(user);

                        var msg = lockoutDuration == TimeSpan.FromHours(1)
                            ? "The account has been locked for 1 hour due to repeated incorrect login attempts."
                            : "The account has been locked for 24 hours due to repeated incorrect login attempts.";

                        return _resultHandler.BadRequest<AuthResponseDto>(msg);
                    }

                    return _resultHandler.BadRequest<AuthResponseDto>("Incorrect email address or password");
                }


                await _userManager.ResetAccessFailedCountAsync(user);

                if (user.LockoutEscalationLevel != 0)
                {
                    user.LockoutEscalationLevel = 0;
                    await _userManager.UpdateAsync(user);
                }


                var roles = await _userManager.GetRolesAsync(user);
                var roleList = roles.Count > 0 ? roles.ToList() : new List<string> { "Customer" };
                var primaryRole = roleList.First();
                var tokenData = _tokenService.GenerateToken(user, roleList);

                return _resultHandler.Success(new AuthResponseDto
                {
                    Token = tokenData.token,
                    Expiration = tokenData.expiresAt,
                    UserId = user.Id,
                    Email = user.Email!,
                    FullName = $"{user.FirstName} {user.LastName}",
                    Role = primaryRole
                });
            }
            catch (OperationCanceledException)
            {

                return _resultHandler.InternalServerError<AuthResponseDto>("Request timed out due to weak network connection.");
            }
        }

        public async Task<ServiceResult<string>> ConfirmEmailAsync(ConfirmEmailDto dto, CancellationToken ct = default)
        {
            var validation = await _confirmEmailValidator.ValidateAsync(dto, ct);
            if (!validation.IsValid) throw new ValidationException(validation.Errors);

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return _resultHandler.NotFound<string>("User not found.");

            if (user.IsDeleted)
                return _resultHandler.Unauthorized<string>("This account no longer exists.");

            if (user.EmailConfirmed)
                return _resultHandler.BadRequest<string>("Email is already confirmed.");

            var decodedToken = WebUtility.UrlDecode(dto.OTP);

            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

            if (result.Succeeded)
            {
                return _resultHandler.Success<string>("Email confirmed successfully. You can now log in.");
            }

            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return _resultHandler.BadRequest<string>(errors);
        }

        public async Task<ServiceResult<string>> ResendOtpAsync(ResendOtpDto dto, CancellationToken ct = default)
        {
            var validation = await _resendotpvalidator.ValidateAsync(dto, ct);
            if (!validation.IsValid) throw new ValidationException(validation.Errors);

            var email = dto.Email.Trim().ToLowerInvariant();
            var user = await _userManager.FindByEmailAsync(email);

            const string genericMessage = "If the email is registered and unverified, a new verification code will be sent shortly.";


            if (user is null || user.IsDeleted)
            {
                _logger.LogWarning("Resend OTP attempt for unregistered email: {Email}", email);
                return _resultHandler.Success<string>(genericMessage);
            }

            if (!user.EmailConfirmed)
                return _resultHandler.Success<string>(genericMessage);

            if (!user.IsActive)
                return _resultHandler.AccountInactive<string>("This account is inactive. Please contact support.");

            bool isEmailSent = await GenerateOtpAndSendEmailAsync(user, "Forget Password", OtpPurpose.PasswordReset);

            if (!isEmailSent)
                return _resultHandler.BadRequest<string>("An error occurred while sending the email. Please try again later.");

            _logger.LogInformation("otp code resent to user {UserId}", user.Id);
            return _resultHandler.Success<string>("A new otp code has been sent to your email.");
        }

        public async Task<ServiceResult<string>> ForgotPasswordAsync(ForgotPasswordDto dto, CancellationToken ct = default)
        {
            var validation = await _forgetpasswordvalidator.ValidateAsync(dto, ct);
            if (!validation.IsValid) throw new ValidationException(validation.Errors);

            var email = dto.Email.Trim().ToLowerInvariant();
            var user = await _userManager.FindByEmailAsync(email);

            const string genericMessage = "If the email is registered and active, a password reset code will be sent shortly.";

            if (user is null || user.IsDeleted)
            {
                _logger.LogWarning("Forgot password attempt for unregistered email: {Email}", email);
                return _resultHandler.Success<string>(null, genericMessage);
            }

            if (!user.EmailConfirmed || !user.IsActive)
                return _resultHandler.Success<string>(null, genericMessage);

            bool isEmailSent = await GenerateOtpAndSendEmailAsync(user, "Password Reset", OtpPurpose.PasswordReset);

            if (!isEmailSent)
                return _resultHandler.BadRequest<string>("An error occurred while sending the email. Please try again later.");

            _logger.LogInformation("Password reset code sent to user {UserId}", user.Id);
            return _resultHandler.Success<string>(null, genericMessage);
        }

        public async Task<ServiceResult<string>> VerifyOtpAsync(string email, string otpCode)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
                return _resultHandler.NotFound<string>("User was not found.");

            if (user.IsDeleted)
                return _resultHandler.Unauthorized<string>("This account no longer exists.");

            var otpResult = await _otpService.VerifyOtpAsync(
                user.Id,
                otpCode,
                OtpPurpose.EmailVerification);

            if (!otpResult.Succeeded)
                return _resultHandler.BadRequest<string>(otpResult.Message, otpResult.Errors);

            user.EmailConfirmed = true;
            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                var errors = updateResult.Errors.Select(e => e.Description).ToList();
                return _resultHandler.BadRequest<string>("Failed to update user account.", errors);
            }

            return _resultHandler.Success<string>(null,
                "Email verified successfully. Your account is now pending admin trade license review.");
        }

        public async Task<ServiceResult<string>> ResetPasswordAsync(ResetPasswordDto dto, CancellationToken ct = default)
        {
            var validation = await _resetpasswordvalidator.ValidateAsync(dto, ct);
            if (!validation.IsValid) throw new ValidationException(validation.Errors);

            var email = dto.Email.Trim().ToLowerInvariant();

            var user = await _userManager.Users
                  .Include(u => u.RefreshTokens)
                  .FirstOrDefaultAsync(u => u.Email == email, ct);

            if (user is null)
                return _resultHandler.BadRequest<string>("Invalid reset data.");

            if (user.IsDeleted)
                return _resultHandler.Unauthorized<string>("This account no longer exists.");

            if (!user.IsActive || !user.EmailConfirmed)
                return _resultHandler.AccountInactive<string>("This account is inactive or not verified.");

            var otpResult = await _otpService.VerifyOtpAsync(user.Id, dto.Otp.Trim(), OtpPurpose.PasswordReset);
            if (!otpResult.Succeeded)
            {
                _logger.LogWarning("Failed to verify reset OTP for user {UserId}: {Message}", user.Id, otpResult.Message);
                return _resultHandler.BadRequest<string>("Invalid email, incorrect code, or code has expired.");
            }

            string resetToken;
            try
            {
                resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while generating password reset token for user {UserId}", user.Id);
                return _resultHandler.BadRequest<string>("An error occurred while resetting the password. Please try again.");
            }

            var resetResult = await _userManager.ResetPasswordAsync(user, resetToken, dto.NewPassword);
            if (!resetResult.Succeeded)
            {
                _logger.LogWarning("Password reset failed for user {UserId}: {Errors}", user.Id, string.Join(", ", resetResult.Errors.Select(e => e.Description)));
                return _resultHandler.BadRequest<string>("Password reset failed. Please try again.");
            }

            if (user.RefreshTokens != null)
            {
                foreach (var token in user.RefreshTokens.Where(t => t.IsActive))
                    token.RevokedAt = DateTime.UtcNow;
            }

            await _userManager.UpdateSecurityStampAsync(user);
            await _userManager.UpdateAsync(user);

            try
            {
                var body = $@"
<p>Hello {user.FirstName},</p>
<p>Your account password has been successfully changed.</p>
<p style='color:#d9534f'>
    If you did not make this change, please contact support immediately.
</p>";
                await _emailService.SendEmailAsync(user.Email!, "Password Changed Successfully", body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password change notification to {Email}", user.Email);
            }

            _logger.LogInformation("Password reset successfully for user {UserId}", user.Id);
            return _resultHandler.Success<string>("Password has been changed successfully. Please login with your new password.");
        }

        public async Task<ServiceResult<bool>> DeleteMyAccountAsync(int userId, CancellationToken ct = default)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user is null || user.IsDeleted)
                return _resultHandler.Unauthorized<bool>("Account not found.");

            user.IsDeleted = true;
            user.DeletedAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
            return _resultHandler.Deleted<bool>();
        }
        // ==========================================================
        // ===================  Private Helpers  ===================
        // ==========================================================

        private async Task<bool> GenerateOtpAndSendEmailAsync(ApplicationUser user, string subject, OtpPurpose purpose)
        {
            var otpResult = await _otpService.GenerateAndStoreOtpAsync(user.Id, purpose);
            if (!otpResult.Succeeded) return false;

            string plainOtp = otpResult.Data!;

            var body = purpose switch
            {
                OtpPurpose.EmailVerification => BuildEmailVerificationOtpBody(user.FirstName, plainOtp),
                OtpPurpose.PasswordReset => BuildPasswordResetOtpBody(user.FirstName, plainOtp),
                _ => throw new ArgumentOutOfRangeException(nameof(purpose))
            };

            var res = await ExecuteEmailSendingAsync(user.Email!, subject, body, "otp");
            return res;
        }


        private static string BuildEmailVerificationOtpBody(string firstName, string otp)
        {
            return $@"
    <p>Hello {firstName},</p>
    <p>Thank you for registering with Onpoint Store!</p>
    <p>Your email verification code is: 
        <strong style='font-size:22px;letter-spacing:4px'>{otp}</strong>
    </p>
    <p>This code expires in 10 minutes.</p>
    <p style='color:#d9534f'>
        If you didn't create an account, you can safely ignore this email.
    </p>";
        }


        private static string BuildPasswordResetOtpBody(string firstName, string otp)
        {
            return $@"
    <p>Hello {firstName},</p>
    <p>We received a request to reset your password.</p>
    <p>Your reset code is: 
        <strong style='font-size:22px;letter-spacing:4px'>{otp}</strong>
    </p>
    <p>This code expires in 10 minutes.</p>
    <p style='color:#d9534f'>
        If you didn't request this, you can safely ignore this email.
    </p>";
        }


        /*  private static string BuildEmailBody(string firstName, string payload, EmailTemplateType templateType)
          {
              // احتفظنا بهذا الميثود فقط للينك (لو لسه بتستخدمه في مكان تاني)
              return templateType switch
              {
                  EmailTemplateType.EmailVerification => $@"
          <p>Hello {firstName},</p>
          <p>Thank you for registering with Onpoint Store!</p>
          <p>Please click the button below to verify your email address:</p>
          <div style='margin: 30px 0;'>
              <a href='{payload}' 
                 style='padding: 12px 24px; background-color: #007bff; color: white; text-decoration: none; border-radius: 5px; font-weight: bold;'>
                  Verify Email Address
              </a>
          </div>
          <p style='word-break: break-all; color: #495057; font-size: 13px;'>
              If the button doesn't work, paste this link in your browser: {payload}
          </p>",

                  _ => throw new ArgumentOutOfRangeException(nameof(templateType), $"Unexpected template type: {templateType}")
              };
          }

          private async Task<bool> GenerateVerificationLinkAndSendEmailAsync(ApplicationUser user, string subject)
          {
              var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

              var encodedToken = WebUtility.UrlDecode(token);

              var frontendUrl = _configuration["Frontend:BaseUrl"];
              var verificationLink = $"{frontendUrl}/verify-email?token={encodedToken}";

              var body = BuildEmailBody(user.FirstName, verificationLink, EmailTemplateType.EmailVerification);

              var res = await ExecuteEmailSendingAsync(user.Email, subject, body, "verification");
              return res;
          }*/

        private async Task<bool> ExecuteEmailSendingAsync(string? email, string subject, string body, string logType)
        {
            try
            {
                await _emailService.SendEmailAsync(email!, subject, body);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send {LogType} email to {Email}", logType, email);
                return false;
            }
        }

        private static string BuildEmailBody(string firstName, string payload, EmailTemplateType templateType)
        {
            return templateType switch
            {
                EmailTemplateType.OtpVerification => $@"
            <p>Hello {firstName},</p>
            <p>We received a request to reset your password.</p>
            <p>Your reset code is: 
                <strong style='font-size:22px;letter-spacing:4px'>{payload}</strong>
            </p>
            <p>This code expires in 10 minutes.</p>
            <p style='color:#d9534f'>
                If you didn't request this, you can safely ignore this email.
            </p>",

                EmailTemplateType.EmailVerification => $@"
            <p>Hello {firstName},</p>
            <p>Thank you for registering with Onpoint Store!</p>
            <p>Please click the button below to verify your email address:</p>
            <div style='margin: 30px 0;'>
                <a href='{payload}' 
                   style='padding: 12px 24px; background-color: #007bff; color: white; text-decoration: none; border-radius: 5px; font-weight: bold;'>
                    Verify Email Address
                </a>
            </div>
            <p style='word-break: break-all; color: #495057; font-size: 13px;'>
                If the button doesn't work, paste this link in your browser: {payload}
            </p>",

                _ => throw new ArgumentOutOfRangeException(nameof(templateType), $"Unexpected template type: {templateType}")
            };
        }

        private async Task EnsureRoleExistsAsync(string roleName)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
                await _roleManager.CreateAsync(new IdentityRole<int>(roleName));
        }
    }
}