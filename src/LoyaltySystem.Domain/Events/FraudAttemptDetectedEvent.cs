using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.ValueObjects;

namespace LoyaltySystem.Domain.Events;

/// <summary>
/// Domain event raised when a potential fraud attempt is detected
/// Contains detailed information about the fraud indicators
/// </summary>
public sealed record FraudAttemptDetectedEvent : DomainEventBase
{
    public override Guid AggregateId { get; init; }
    public override string AggregateType { get; init; } = "LoyaltyCard";
    
    // Core Transaction Data
    public Guid CardId { get; init; }
    public Guid CustomerId { get; init; }
    public Guid ProgramId { get; init; }
    public Guid StoreId { get; init; }
    public Guid? StaffId { get; init; }
    public string? PosTransactionId { get; init; }
    
    // Attempted Transaction Details
    public string TransactionType { get; init; } // "StampIssuance", "PointsIssuance", "RewardRedemption"
    public int? AttemptedStamps { get; init; }
    public decimal? AttemptedPoints { get; init; }
    public Guid? AttemptedRewardId { get; init; }
    public decimal? TransactionAmount { get; init; }
    
    // Fraud Indicators
    public List<string> FraudReasons { get; init; } = new();
    public string PrimaryFraudReason { get; init; }
    public FraudSeverity Severity { get; init; }
    
    // Timing Analysis
    public DateTime? LastTransactionTime { get; init; }
    public TimeSpan? TimeSinceLastTransaction { get; init; }
    public TimeSpan? RequiredCooldown { get; init; }
    public bool CooldownViolation { get; init; }
    
    // Velocity Analysis
    public int DailyTransactionCount { get; init; }
    public int DailyRedemptionCount { get; init; }
    public decimal DailyPointsEarned { get; init; }
    public decimal DailyCurrencyRedeemed { get; init; }
    public bool VelocityLimitExceeded { get; init; }
    
    // Location Analysis
    public int? DistanceFromLastTransaction { get; init; }
    public int? MaxAllowedDistance { get; init; }
    public bool LocationSuspicious { get; init; }
    
    // Business Context
    public string ProgramName { get; init; }
    public string StoreName { get; init; }
    public string? StaffName { get; init; }
    
    // Risk Assessment
    public decimal RiskScore { get; init; }
    public string RiskLevel { get; init; } // "Low", "Medium", "High", "Critical"
    public bool TransactionBlocked { get; init; }
    public string? RecommendedAction { get; init; }
    
    public FraudAttemptDetectedEvent(
        Guid cardId,
        Guid customerId,
        Guid programId,
        Guid storeId,
        string transactionType,
        string primaryFraudReason,
        FraudSeverity severity,
        string programName,
        string storeName,
        List<string>? fraudReasons = null,
        Guid? staffId = null,
        string? staffName = null,
        string? posTransactionId = null,
        int? attemptedStamps = null,
        decimal? attemptedPoints = null,
        Guid? attemptedRewardId = null,
        decimal? transactionAmount = null,
        DateTime? lastTransactionTime = null,
        TimeSpan? requiredCooldown = null,
        int dailyTransactionCount = 0,
        int dailyRedemptionCount = 0,
        decimal dailyPointsEarned = 0,
        decimal dailyCurrencyRedeemed = 0,
        int? distanceFromLastTransaction = null,
        int? maxAllowedDistance = null,
        decimal riskScore = 0,
        bool transactionBlocked = true,
        string? recommendedAction = null)
    {
        AggregateId = cardId;
        CardId = cardId;
        CustomerId = customerId;
        ProgramId = programId;
        StoreId = storeId;
        StaffId = staffId;
        PosTransactionId = posTransactionId;
        
        TransactionType = transactionType;
        AttemptedStamps = attemptedStamps;
        AttemptedPoints = attemptedPoints;
        AttemptedRewardId = attemptedRewardId;
        TransactionAmount = transactionAmount;
        
        FraudReasons = fraudReasons ?? new List<string>();
        PrimaryFraudReason = primaryFraudReason;
        Severity = severity;
        
        LastTransactionTime = lastTransactionTime;
        TimeSinceLastTransaction = lastTransactionTime.HasValue ? DateTime.UtcNow - lastTransactionTime.Value : null;
        RequiredCooldown = requiredCooldown;
        CooldownViolation = requiredCooldown.HasValue && TimeSinceLastTransaction.HasValue && 
                           TimeSinceLastTransaction.Value < requiredCooldown.Value;
        
        DailyTransactionCount = dailyTransactionCount;
        DailyRedemptionCount = dailyRedemptionCount;
        DailyPointsEarned = dailyPointsEarned;
        DailyCurrencyRedeemed = dailyCurrencyRedeemed;
        VelocityLimitExceeded = FraudReasons.Any(r => r.Contains("velocity") || r.Contains("limit"));
        
        DistanceFromLastTransaction = distanceFromLastTransaction;
        MaxAllowedDistance = maxAllowedDistance;
        LocationSuspicious = maxAllowedDistance.HasValue && distanceFromLastTransaction.HasValue && 
                            distanceFromLastTransaction.Value > maxAllowedDistance.Value;
        
        ProgramName = programName;
        StoreName = storeName;
        StaffName = staffName;
        
        RiskScore = riskScore;
        RiskLevel = CalculateRiskLevel(riskScore, severity);
        TransactionBlocked = transactionBlocked;
        RecommendedAction = recommendedAction ?? GetDefaultRecommendedAction(severity);
    }
    
    private static string CalculateRiskLevel(decimal riskScore, FraudSeverity severity)
    {
        return severity switch
        {
            FraudSeverity.Low => "Low",
            FraudSeverity.Medium => "Medium", 
            FraudSeverity.High => "High",
            FraudSeverity.Critical => "Critical",
            _ => riskScore switch
            {
                < 0.3m => "Low",
                < 0.6m => "Medium",
                < 0.8m => "High",
                _ => "Critical"
            }
        };
    }
    
    private static string GetDefaultRecommendedAction(FraudSeverity severity)
    {
        return severity switch
        {
            FraudSeverity.Low => "Monitor future transactions",
            FraudSeverity.Medium => "Require additional verification",
            FraudSeverity.High => "Block transaction and review account",
            FraudSeverity.Critical => "Suspend account and investigate",
            _ => "Review transaction"
        };
    }
}

/// <summary>
/// Severity levels for fraud detection
/// </summary>
public enum FraudSeverity
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
} 