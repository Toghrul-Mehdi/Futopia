using Futopia.UserService.Application.DTOs.Auth;
namespace Futopia.UserService.Application.Abstractions.Service;
public interface IAuthenticationService
{
    Task<Response> RegisterAsync(RegisterDto registerDto);
    Task<Response> LoginAsync(LoginDto loginDto);
    Task<Response> ConfirmEmailAsync(string email, string code);
    Task<Response> SendOtpCodeAsync(string phoneNumber);
}
