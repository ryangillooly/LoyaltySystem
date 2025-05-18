using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Enums;

namespace LoyaltySystem.Application.DTOs;

public class LoyaltyCardDto
{
    public LoyaltyCardId? Id { get; set; }
    public CustomerId? CustomerId { get; set; }
    public LoyaltyProgramId? ProgramId { get; set; }
    public string QrCode { get; set; }
    public CardStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public LoyaltyProgramType Type { get; set; }
    public int StampCount { get; set; }
    public decimal PointsBalance { get; set; }
    public int TotalTransactions { get; set; }
    public DateTime? LastActivityDate { get; set; }
        
    public IReadOnlyCollection<Transaction> Transactions { get; set; } 
}
    
public class CreateLoyaltyCardDto
{
    public string CustomerId { get; set; } = string.Empty;
    public string ProgramId { get; set; } = string.Empty;
    public string QrCode { get; set; } = string.Empty;
    public CardStatus Status { get; set; } = CardStatus.Active;
    public LoyaltyProgramType Type { get; set; } = LoyaltyProgramType.Stamp;
    public int? StampCount { get; set; } = 0;
    public decimal? PointsBalance { get; set; } = 0;
    public int TotalTransactions { get; set; } = 0;
}