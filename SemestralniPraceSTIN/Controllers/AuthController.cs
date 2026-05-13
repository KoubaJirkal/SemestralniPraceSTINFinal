using Microsoft.AspNetCore.Mvc;
using smestralka.DTOs;
using smestralka.Services;

namespace smestralka.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public IActionResult Login(LoginDto request)
        {
            var token = _authService.Login(request.Password);

            if (token == null)
                return Unauthorized();

            return Ok(new { token });
        }
    }
}
