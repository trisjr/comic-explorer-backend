using System.Security.Claims;

namespace ApplicationCore.Interfaces;

public interface ITokenClaimsService
{
    Task<string> GetTokenAsync(string userName);

    Task<string> GetUserNameFromTokenAsync(string token);

    Task<string> GenerateRefreshTokenAsync(string userName);

    ClaimsPrincipal GetPrincipalFromExpiredToken(string expiredToken);

    Task<bool> CheckRefreshTokenAsync(string? userName, string refreshToken);
}
