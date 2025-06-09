using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.ValueObjects;

namespace LoyaltySystem.Domain.Events;

/// <summary>
/// Domain event raised when points are added to a loyalty card
/// Enhanced with full business context and transaction details
/// </summary>
public sealed record PointsAddedEvent : DomainEventBase
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
    
    // Points Details
    public decimal PointsAdded { get; init; }
    public decimal TotalPointsAfter { get; init; }
    public decimal TotalPointsBefore { get; init; }
    
    // Transaction Context
    public decimal TransactionAmount { get; init; }
    public decimal ConversionRate { get; init; }
    public decimal BasePointsEarned { get; init; }
    public decimal TierMultiplier { get; init; }
    
    // Business Context
    public string ProgramName { get; init; }
    public string StoreName { get; init; }
    public string? StaffName { get; init; }
    
    // Tier Information
    public string? CurrentTierName { get; init; }
    public string? NewTierName { get; init; }
    public bool TierUpgraded { get; init; }
    public int? NextTierThreshold { get; init; }
    public decimal PointsToNextTier { get; init; }
    
    // Fraud & Validation Context
    public bool FraudCheckPassed { get; init; }
    public bool DailyLimitCheckPassed { get; init; }
    public decimal DailyPointsBeforeTransaction { get; init; }
    public decimal DailyPointsAfterTransaction { get; init; }
    
    // Redemption Context
    public decimal CurrencyValue { get; init; }
    public int MinimumRedemptionPoints { get; init; }
    public bool EligibleForRedemption { get; init; }
    
    public PointsAddedEvent(
        Guid cardId,
        Guid customerId,
        Guid programId,
        Guid storeId,
        decimal pointsAdded,
        decimal totalPointsBefore,
        decimal transactionAmount,
        decimal conversionRate,
        string programName,
        string storeName,
        Guid? staffId = null,
        string? staffName = null,
        string? posTransactionId = null,
        decimal tierMultiplier = 1.0m,
        string? currentTierName = null,
        string? newTierName = null,
        int? nextTierThreshold = null,
        bool fraudCheckPassed = true,
        bool dailyLimitCheckPassed = true,
        decimal dailyPointsBeforeTransaction = 0,
        decimal currencyValue = 0,
        int minimumRedemptionPoints = 0)
    {
        AggregateId = cardId;
        CardId = cardId;
        CustomerId = customerId;
        ProgramId = programId;
        StoreId = storeId;
        StaffId = staffId;
        PosTransactionId = posTransactionId;
        
        PointsAdded = pointsAdded;
        TotalPointsBefore = totalPointsBefore;
        TotalPointsAfter = totalPointsBefore + pointsAdded;
        
        TransactionAmount = transactionAmount;
        ConversionRate = conversionRate;
        BasePointsEarned = transactionAmount * conversionRate;
        TierMultiplier = tierMultiplier;
        
        ProgramName = programName;
        StoreName = storeName;
        StaffName = staffName;
        
        CurrentTierName = currentTierName;
        NewTierName = newTierName;
        TierUpgraded = !string.IsNullOrEmpty(newTierName) && newTierName != currentTierName;
        NextTierThreshold = nextTierThreshold;
        PointsToNextTier = nextTierThreshold.HasValue ? Math.Max(0, nextTierThreshold.Value - (int)TotalPointsAfter) : 0;
        
        FraudCheckPassed = fraudCheckPassed;
        DailyLimitCheckPassed = dailyLimitCheckPassed;
        DailyPointsBeforeTransaction = dailyPointsBeforeTransaction;
        DailyPointsAfterTransaction = dailyPointsBeforeTransaction + pointsAdded;
        
        CurrencyValue = currencyValue;
        MinimumRedemptionPoints = minimumRedemptionPoints;
        EligibleForRedemption = TotalPointsAfter >= minimumRedemptionPoints;
    }
} 