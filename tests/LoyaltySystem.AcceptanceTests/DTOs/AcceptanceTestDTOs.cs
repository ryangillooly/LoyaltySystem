using System.Text.Json.Serialization;

namespace LoyaltySystem.AcceptanceTests.DTOs;

/// <summary>
/// These DTOs are specifically for the acceptance tests and may duplicate
/// some application DTOs. We define them here to keep the tests isolated
/// from changes in the application DTOs.
/// </summary>

// Authentication DTOs
public class LoginDto
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class RegisterUserDto
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string ConfirmPassword { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
}

public class UserDto
{
    public string Id { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class AuthResponseDto
{
    public string Token { get; set; } = null!;
    public UserDto User { get; set; } = null!;
    public DateTime Expiration { get; set; }
}

// Loyalty Program DTOs
public class LoyaltyProgramDto
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string BusinessId { get; set; } = null!;
    public string BrandId { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public bool Active { get; set; }
    public List<RewardDto> Rewards { get; set; } = new List<RewardDto>();
}

public class LoyaltyCardDto
{
    public string Id { get; set; } = null!;
    public string CustomerId { get; set; } = null!;
    public string LoyaltyProgramId { get; set; } = null!;
    public string CardNumber { get; set; } = null!;
    public int CurrentPoints { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastUsed { get; set; }
    public string Status { get; set; } = null!;
}

public class RewardDto
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string LoyaltyProgramId { get; set; } = null!;
    public int PointsCost { get; set; }
    public bool Active { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

// Transaction DTOs
public class CreateTransactionDto
{
    public string LoyaltyCardId { get; set; } = null!;
    public decimal Amount { get; set; }
    public string Description { get; set; } = null!;
}

public class TransactionDto
{
    public string Id { get; set; } = null!;
    public string LoyaltyCardId { get; set; } = null!;
    public decimal Amount { get; set; }
    public int PointsEarned { get; set; }
    public string Description { get; set; } = null!;
    public DateTime Timestamp { get; set; }
}

// Reward Redemption DTOs
public class RedeemRewardDto
{
    public string LoyaltyCardId { get; set; } = null!;
    public string RewardId { get; set; } = null!;
}

public class RedemptionResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = null!;
    public string RedemptionCode { get; set; } = null!;
    public RewardDto Reward { get; set; } = null!;
}

// Card Request DTOs
public class CreateCardRequest
{
    public string LoyaltyProgramId { get; set; } = null!;
}

// You can add more DTOs as needed for the acceptance tests 