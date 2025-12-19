using Futopia.UserService.Application.Abstractions.Service;
using Futopia.UserService.Application.DTOs.Auth;
using Futopia.UserService.Application.ResponceObject.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Futopia.UserService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticationService _authService;

        public AuthController(IAuthenticationService authService)
        {
            _authService = authService;
        }

        // REGISTER
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var response = await _authService.RegisterAsync(registerDto);

            if (response.ResponseStatusCode == ResponseStatusCode.Error)
                return BadRequest(response);

            return Ok(response);
        }

        // LOGIN
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var response = await _authService.LoginAsync(loginDto);

            if (response.ResponseStatusCode == ResponseStatusCode.Error)
                return BadRequest(response);

            return Ok(response);
        }

        // REFRESH TOKEN
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken()
        {
            var response = await _authService.RefreshTokenAsync();

            if (response.ResponseStatusCode == ResponseStatusCode.Error)
                return Unauthorized(response);

            return Ok(response);
        }

        // LOGOUT
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var response = await _authService.LogoutAsync();

            if (response.ResponseStatusCode == ResponseStatusCode.Error)
                return BadRequest(response);

            return Ok(response);
        }

        // EMAIL CONFIRM
        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(
            [FromQuery] string email,
            [FromQuery] string code)
        {
            var response = await _authService.ConfirmEmailAsync(email, code);

            if (response.ResponseStatusCode == ResponseStatusCode.Error)
                return BadRequest(response);

            return Ok(response);
        }

        // VERIFY OTP
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyNumberDto verifyDto)
        {
            var response = await _authService.VerifyMobileAsync(verifyDto);

            if (response.ResponseStatusCode == ResponseStatusCode.Error)
                return BadRequest(response);

            return Ok(response);
        }
    }
}
