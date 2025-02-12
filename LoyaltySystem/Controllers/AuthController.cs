using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using LoyaltySystem.Services;

namespace LoyaltySystem.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // POST /api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var result = await _authService.RegisterUserAsync(dto);
        if (!result.Success)
        {
            return BadRequest(result.ErrorMessage);
        }
        return Ok(new { userId = result.UserId});
    }

    // POST /api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto);
        if (!result.Success)
        {
            return Unauthorized(result.ErrorMessage);
        }
        return Ok(new { token = result.Token });
    }

    // POST /api/auth/logout
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        // Typically for JWT-based auth, there's no server-based logout
        _authService.Logout();
        return Ok("Logged out successfully");
    }

    // POST /api/auth/forgot-password
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        var success = await _authService.ForgotPasswordAsync(dto);
        // Often we return 200 to avoid email enumeration
        return Ok("If the email is registered, a reset link was sent");
    }

    // POST /api/auth/reset-password
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        var success = await _authService.ResetPasswordAsync(dto);
        if (!success)
        {
            return BadRequest("Invalid token or unable to reset password.");
        }
        return Ok("Password reset successful.");
    }
}
