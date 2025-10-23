using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Futopia.UserService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Futopia User Service is running...");
        }
    }
}
