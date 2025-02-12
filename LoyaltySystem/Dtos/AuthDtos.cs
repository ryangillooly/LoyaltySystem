namespace LoyaltySystem.Services;

public class RegisterDto
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}

public class RegisterResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public int? UserId { get; set; }  // add this
}


public class LoginDto
{
    public string Email { get; set; }
    public string Password { get; set; }
}

public class LoginResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public string Token { get; set; }
}

public class ForgotPasswordDto
{
    public string Email { get; set; }
}

public class ResetPasswordDto
{
    public string Token { get; set; }
    public string NewPassword { get; set; }
}