using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.ValueObjects;

namespace LoyaltySystem.Domain.Events;

/// <summary>
/// Domain event raised when stamps are issued to a loyalty card
/// Enhanced with full business context and transaction details
/// </summary>
public sealed record StampsIssuedEvent : DomainEventBase
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
    
    // Stamp Details
    public int StampsIssued { get; init; }
    public int TotalStampsAfter { get; init; }
    public int TotalStampsBefore { get; init; }
    
    // Business Context
    public string ProgramName { get; init; }
    public string StoreName { get; init; }
    public string? StaffName { get; init; }
    
    // Fraud & Validation Context
    public bool FraudCheckPassed { get; init; }
    public bool DailyLimitCheckPassed { get; init; }
    public int DailyStampsBeforeTransaction { get; init; }
    public int DailyStampsAfterTransaction { get; init; }
    
    // Threshold Progress
    public int? StampThreshold { get; init; }
    public bool ThresholdReached { get; init; }
    public int StampsToThreshold { get; init; }
    
    public StampsIssuedEvent(
        Guid cardId,
        Guid customerId,
        Guid programId,
        Guid storeId,
        int stampsIssued,
        int totalStampsBefore,
        string programName,
        string storeName,
        Guid? staffId = null,
        string? staffName = null,
        string? posTransactionId = null,
        bool fraudCheckPassed = true,
        bool dailyLimitCheckPassed = true,
        int dailyStampsBeforeTransaction = 0,
        int? stampThreshold = null)
    {
        AggregateId = cardId;
        CardId = cardId;
        CustomerId = customerId;
        ProgramId = programId;
        StoreId = storeId;
        StaffId = staffId;
        PosTransactionId = posTransactionId;
        
        StampsIssued = stampsIssued;
        TotalStampsBefore = totalStampsBefore;
        TotalStampsAfter = totalStampsBefore + stampsIssued;
        
        ProgramName = programName;
        StoreName = storeName;
        StaffName = staffName;
        
        FraudCheckPassed = fraudCheckPassed;
        DailyLimitCheckPassed = dailyLimitCheckPassed;
        DailyStampsBeforeTransaction = dailyStampsBeforeTransaction;
        DailyStampsAfterTransaction = dailyStampsBeforeTransaction + stampsIssued;
        
        StampThreshold = stampThreshold;
        ThresholdReached = stampThreshold.HasValue && TotalStampsAfter >= stampThreshold.Value;
        StampsToThreshold = stampThreshold.HasValue ? Math.Max(0, stampThreshold.Value - TotalStampsAfter) : 0;
    }
} 