using Futopia.UserService.Application.DTOs.Auth;

namespace Futopia.UserService.Application.Abstractions.Service;

public interface IAuthenticationService
{
    // Auth
    Task<Response> RegisterAsync(RegisterDto registerDto);
    Task<Response> LoginAsync(LoginDto loginDto);

    // Token lifecycle
    Task<Response> RefreshTokenAsync();
    Task<Response> LogoutAsync();

    // Verification
    Task<Response> ConfirmEmailAsync(string email, string code);
    Task<Response> VerifyMobileAsync(VerifyNumberDto verifyDto);
}
