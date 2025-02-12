using LoyaltySystem.Data;
using LoyaltySystem.Repositories;
using System;
using System.Threading.Tasks;

namespace LoyaltySystem.Services;

public interface IAuthService
{
    Task<RegisterResult> RegisterUserAsync(RegisterDto dto);
    Task<LoginResult> LoginAsync(LoginDto dto);
    void Logout();
    Task<bool> ForgotPasswordAsync(ForgotPasswordDto dto);
    Task<bool> ResetPasswordAsync(ResetPasswordDto dto);
}

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService; // For JWT creation, if needed

    public AuthService(IUserRepository userRepository, ITokenService tokenService)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
    }

    public async Task<RegisterResult> RegisterUserAsync(RegisterDto dto)
    {
        // Check if user already exists
        var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
        if (existingUser != null)
        {
            return new RegisterResult
            {
                Success = false,
                ErrorMessage = "Email already in use."
            };
        }

        var user = new User
        {
            Email = dto.Email.ToLowerInvariant(),
            PasswordHash = HashPassword(dto.Password),
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            CreatedAt = DateTime.UtcNow
        };

        var newUserId = await _userRepository.AddAsync(user); 
        return new RegisterResult
        {
            Success = true,
            UserId = newUserId
        };
    }

    public async Task<LoginResult> LoginAsync(LoginDto dto)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email);
        if (user == null)
        {
            return new LoginResult
            {
                Success = false,
                ErrorMessage = "Invalid credentials."
            };
        }

        bool passwordOk = VerifyPassword(dto.Password, user.PasswordHash);
        if (!passwordOk)
        {
            return new LoginResult
            {
                Success = false,
                ErrorMessage = "Invalid credentials."
            };
        }

        // Generate a JWT or token
        var token = _tokenService.GenerateToken(user);

        return new LoginResult
        {
            Success = true,
            Token = token
        };
    }

    public void Logout()
    {
        // If you store tokens somewhere for blacklisting, you'd revoke here.
    }

    public async Task<bool> ForgotPasswordAsync(ForgotPasswordDto dto)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email);
        if (user == null) return false;

        // Generate a reset token, email it, etc.
        return true;
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordDto dto)
    {
        // Validate token, find user from token claims or stored DB token
        var user = await _userRepository.GetByEmailAsync("someone@example.com");
        if (user == null) return false;

        user.PasswordHash = HashPassword(dto.NewPassword);
        await _userRepository.UpdateAsync(user);
        return true;
    }

    // Simple placeholders. Use a real hashing library in production
    private string HashPassword(string raw) => $"hashed-{raw}";
    private bool VerifyPassword(string raw, string stored) => stored == $"hashed-{raw}";
}
