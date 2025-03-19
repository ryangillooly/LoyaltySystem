using System;
using System.Collections.Generic;

namespace LoyaltySystem.Application.DTOs;

public class ProgramAnalyticsDto
{
    public int TotalPrograms { get; set; }
    public int ActivePrograms { get; set; }
    public int StampPrograms { get; set; }
    public int PointsPrograms { get; set; }
    public int TotalRewards { get; set; }
    public int ActiveRewards { get; set; }
    public Dictionary<string, int> ProgramsByBrand { get; set; } = new Dictionary<string, int>();
}

public class ProgramDetailedAnalyticsDto
{
    public string ProgramId { get; set; } = string.Empty;
    public string ProgramName { get; set; } = string.Empty;
    public string ProgramType { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int TotalCards { get; set; }
    public int ActiveCards { get; set; }
    public int SuspendedCards { get; set; }
    public int ExpiredCards { get; set; }
    public int TotalRewards { get; set; }
    public int ActiveRewards { get; set; }
    public int TotalTransactions { get; set; }
    public decimal TotalPointsIssued { get; set; }
    public decimal TotalPointsRedeemed { get; set; }
    public int TotalStampsIssued { get; set; }
    public int TotalStampsRedeemed { get; set; }
    public int TotalRedemptions { get; set; }
} 