using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using EmployeeApi.Models;
using EmployeeApi.Services;
using Serilog;

namespace EmployeeApi.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthenticationController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Логін: Повертає сесійний токен.
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var token = await _authService.LoginAsync(request);
                return Ok(new { Token = token });
            }
            catch (ArgumentException ex)
            {
                Log.Warning("Неуспешная попытка логина: {Message}", ex.Message);
                return Unauthorized(new { Message = "Неверный логин или пароль" });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при логине");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Логаут: видаляє сесію з токену.
        /// </summary>
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            if (!Request.Headers.TryGetValue("X-Session-Token", out var token))
            {
                return BadRequest(new { Message = "Token is missing from X-Session-Token header" });
            }

            await _authService.LogoutAsync(token!);
            return Ok(new { Message = "You have successfully logged out." });
        }
    }
}
