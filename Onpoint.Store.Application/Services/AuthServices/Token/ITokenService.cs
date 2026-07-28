

using Onpoint.Store.Domin.Entities;

namespace Onpoint.Store.Application.Services.AuthServices.Token
{
    public interface ITokenService
    {
        (string token, DateTime expiresAt) GenerateToken(ApplicationUser user, IList<string> roles);
        string GenerateRefreshToken();
        RefreshToken CreateRefreshTokenEntity(int userId, string tokenString, int expiryDays = 7);
    }
}
