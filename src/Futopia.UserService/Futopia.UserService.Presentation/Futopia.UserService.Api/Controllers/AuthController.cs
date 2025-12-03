using Futopia.UserService.Application.Abstractions.Service;
using Futopia.UserService.Application.DTOs.Auth;
using Futopia.UserService.Application.ResponceObject.Enums;
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

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {           
            var response = await _authService.RegisterAsync(registerDto);

            if (response.ResponseStatusCode == ResponseStatusCode.Error)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var response = await _authService.LoginAsync(loginDto);
            if (response.ResponseStatusCode == ResponseStatusCode.Error)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string email, [FromQuery] string code)
        {
            var response = await _authService.ConfirmEmailAsync(email, code);
            if (response.ResponseStatusCode == ResponseStatusCode.Error)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
    }
}
