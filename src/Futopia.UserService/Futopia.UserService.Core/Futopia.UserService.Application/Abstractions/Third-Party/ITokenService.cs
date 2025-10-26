using System.Security.Claims;
namespace Futopia.UserService.Application.Abstractions.Third_Party;
public interface ITokenService
{
    string GenerateAccessToken(IEnumerable<Claim> claims);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    Task<bool> ValidateTokenAsync(string token);
}
