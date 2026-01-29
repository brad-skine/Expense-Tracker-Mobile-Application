using expense_tracker.Models;
using expense_tracker.Services.Interfaces;
using expense_tracker.Utils.CustomExceptions;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace expense_tracker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        private readonly IAuthService _authService = authService;

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            try
            {
                var token= await _authService.RegisterAsync(request.Email, request.Password);
                
                return Ok( new {token});
            }
            catch (UserAlreadyExistsException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var token = await _authService.LoginAsync(request.Email, request.Password);
                return Ok(new { token });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized("Invalid email or password");
            }
        }
    }
}
