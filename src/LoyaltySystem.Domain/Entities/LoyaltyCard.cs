using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;
using LoyaltySystem.Domain.ValueObjects;
using LoyaltySystem.Domain.Events;

namespace LoyaltySystem.Domain.Entities;

/// <summary>
/// Represents a customer's membership in a loyalty program.
/// This is an Aggregate Root.
/// </summary>
public class LoyaltyCard : Entity<LoyaltyCardId>
{
    private readonly List<Transaction> _transactions;
        
    public LoyaltyProgramId ProgramId { get; set; }
    public CustomerId CustomerId { get; set; }
    public LoyaltyProgramType Type { get; set; }
    public int StampsCollected { get; set; }
    public decimal PointsBalance { get; set; }
    public CardStatus Status { get; set; }
    public string QrCode { get; set; }
    public DateTime? ExpiresAt { get; set; }
   
    public virtual IReadOnlyCollection<Transaction> Transactions
    {
        get => _transactions.AsReadOnly();
        set => throw new NotImplementedException();
    }
        
    public LoyaltyCard
    (
        LoyaltyProgramId programId,
        CustomerId customerId,
        LoyaltyProgramType type,
        DateTime? expiresAt = null
    )
    : base (new LoyaltyCardId())
    {
        ProgramId = programId ?? throw new ArgumentNullException(nameof(programId));
        CustomerId = customerId ?? throw new ArgumentNullException(nameof(customerId));
        Type = type;
        StampsCollected = 0;
        PointsBalance = 0;
        Status = CardStatus.Active;
        QrCode = GenerateQrCode();
        ExpiresAt = expiresAt;
            
        _transactions = new List<Transaction>();
        
        // Enforce business rules on creation
        EnforceBusinessRules();
    }
        
    public Transaction IssueStamps
    (
        int quantity,
        StoreId storeId,
        string programName,
        string storeName,
        int? stampThreshold = null,
        FraudPolicy? fraudPolicy = null,
        DailyLimits? dailyLimits = null,
        StaffId? staffId = null,
        string? staffName = null,
        string? posTransactionId = null
    )
    {
        // Enhanced validation with business rules
        EnforceBusinessRules();
        
        // Capture state before validation
        var stampsBefore = StampsCollected;
        var dailyStampsBefore = GetStampsIssuedToday();
        
        // Validate fraud policy
        var fraudCheckPassed = ValidateFraudPolicy(fraudPolicy, "StampIssuance", quantity);
        
        // Validate daily limits
        var dailyLimitCheckPassed = ValidateDailyLimits(dailyLimits, quantity, 0, 0);
        
        // If fraud or limits failed, raise fraud event and throw exception
        if (!fraudCheckPassed || !dailyLimitCheckPassed)
        {
            RaiseFraudDetectedEvent(
                "StampIssuance", 
                storeId, 
                programName, 
                storeName,
                fraudPolicy,
                dailyLimits,
                staffId,
                staffName,
                posTransactionId,
                attemptedStamps: quantity,
                fraudCheckPassed: fraudCheckPassed,
                dailyLimitCheckPassed: dailyLimitCheckPassed);
            
            var reason = !fraudCheckPassed ? "Fraud policy violation" : "Daily limit exceeded";
            throw new DomainException($"Stamp issuance blocked: {reason}");
        }
        
        ValidateStampIssuance(quantity, storeId);
            
        var transaction = new Transaction
        (
            Id,
            TransactionType.StampIssuance,
            quantity: quantity,
            storeId: storeId,
            staffId: staffId,
            posTransactionId: posTransactionId
        );
            
        StampsCollected += quantity;
        _transactions.Add(transaction);
        MarkAsUpdated();

        // Raise domain event
        var stampsIssuedEvent = new StampsIssuedEvent(
            cardId: Id.Value,
            customerId: CustomerId.Value,
            programId: ProgramId.Value,
            storeId: storeId.Value,
            stampsIssued: quantity,
            totalStampsBefore: stampsBefore,
            programName: programName,
            storeName: storeName,
            staffId: staffId?.Value,
            staffName: staffName,
            posTransactionId: posTransactionId,
            fraudCheckPassed: fraudCheckPassed,
            dailyLimitCheckPassed: dailyLimitCheckPassed,
            dailyStampsBeforeTransaction: dailyStampsBefore,
            stampThreshold: stampThreshold);
        
        AddDomainEvent(stampsIssuedEvent);

        return transaction;
    }
        
    public Transaction AddPoints
    (
        decimal pointsAmount,
        decimal transactionAmount,
        StoreId storeId,
        string programName,
        string storeName,
        decimal conversionRate = 1.0m,
        decimal tierMultiplier = 1.0m,
        string? currentTierName = null,
        string? newTierName = null,
        int? nextTierThreshold = null,
        FraudPolicy? fraudPolicy = null,
        DailyLimits? dailyLimits = null,
        StaffId? staffId = null,
        string? staffName = null,
        string? posTransactionId = null,
        decimal currencyValue = 0,
        int minimumRedemptionPoints = 0
    )
    {
        // Enhanced validation with business rules
        EnforceBusinessRules();
        
        // Capture state before validation
        var pointsBefore = PointsBalance;
        var dailyPointsBefore = GetTodaysTransactions()
            .Where(t => t.Type == TransactionType.PointsIssuance)
            .Sum(t => t.PointsAmount ?? 0);
        
        // Validate fraud policy
        var fraudCheckPassed = ValidateFraudPolicy(fraudPolicy, "PointsIssuance", pointsAmount: pointsAmount);
        
        // Validate daily limits
        var dailyLimitCheckPassed = ValidateDailyLimits(dailyLimits, 0, pointsAmount, 0);
        
        // If fraud or limits failed, raise fraud event and throw exception
        if (!fraudCheckPassed || !dailyLimitCheckPassed)
        {
            RaiseFraudDetectedEvent(
                "PointsIssuance", 
                storeId, 
                programName, 
                storeName,
                fraudPolicy,
                dailyLimits,
                staffId,
                staffName,
                posTransactionId,
                attemptedPoints: pointsAmount,
                transactionAmount: transactionAmount,
                fraudCheckPassed: fraudCheckPassed,
                dailyLimitCheckPassed: dailyLimitCheckPassed);
            
            var reason = !fraudCheckPassed ? "Fraud policy violation" : "Daily limit exceeded";
            throw new DomainException($"Points issuance blocked: {reason}");
        }

        ValidatePointsIssuance(pointsAmount, transactionAmount, storeId);

        // Add the transaction
        var transaction = new Transaction(
            Id,
            TransactionType.PointsIssuance,
            pointsAmount: pointsAmount,
            transactionAmount: transactionAmount,
            storeId: storeId,
            staffId: staffId,
            posTransactionId: posTransactionId);

        // Update points
        PointsBalance += pointsAmount;
        _transactions.Add(transaction);
        MarkAsUpdated();

        // Raise domain event
        var pointsAddedEvent = new PointsAddedEvent(
            cardId: Id.Value,
            customerId: CustomerId.Value,
            programId: ProgramId.Value,
            storeId: storeId.Value,
            pointsAdded: pointsAmount,
            totalPointsBefore: pointsBefore,
            transactionAmount: transactionAmount,
            conversionRate: conversionRate,
            programName: programName,
            storeName: storeName,
            staffId: staffId?.Value,
            staffName: staffName,
            posTransactionId: posTransactionId,
            tierMultiplier: tierMultiplier,
            currentTierName: currentTierName,
            newTierName: newTierName,
            nextTierThreshold: nextTierThreshold,
            fraudCheckPassed: fraudCheckPassed,
            dailyLimitCheckPassed: dailyLimitCheckPassed,
            dailyPointsBeforeTransaction: dailyPointsBefore,
            currencyValue: currencyValue,
            minimumRedemptionPoints: minimumRedemptionPoints);
        
        AddDomainEvent(pointsAddedEvent);

        return transaction;
    }
        
    public Transaction RedeemReward
    (
        Reward reward,
        StoreId storeId,
        string programName,
        string storeName,
        FraudPolicy? fraudPolicy = null,
        DailyLimits? dailyLimits = null,
        StaffId? staffId = null,
        string? staffName = null,
        string? currentTierName = null,
        string? newTierName = null,
        int? nextRewardThreshold = null,
        decimal currencyValue = 0
    )
    {
        ArgumentNullException.ThrowIfNull(reward);
        ArgumentNullException.ThrowIfNull(storeId);
        
        // Enhanced validation with business rules
        EnforceBusinessRules();
        
        // Capture state before validation
        var stampsBefore = StampsCollected;
        var pointsBefore = PointsBalance;
        var dailyRedemptionsBefore = GetTodaysTransactions()
            .Count(t => t.Type == TransactionType.RewardRedemption);
        var dailyCurrencyBefore = GetTodaysTransactions()
            .Where(t => t.Type == TransactionType.RewardRedemption)
            .Sum(t => t.TransactionAmount ?? 0);
        
        // Validate fraud policy
        var fraudCheckPassed = ValidateFraudPolicy(fraudPolicy, "RewardRedemption");
        
        // Validate daily limits
        var dailyLimitCheckPassed = ValidateDailyLimits(dailyLimits, 0, 0, currencyValue);
        
        // If fraud or limits failed, raise fraud event and throw exception
        if (!fraudCheckPassed || !dailyLimitCheckPassed)
        {
            RaiseFraudDetectedEvent(
                "RewardRedemption", 
                storeId, 
                programName, 
                storeName,
                fraudPolicy,
                dailyLimits,
                staffId,
                staffName,
                attemptedRewardId: reward.Id.Value,
                fraudCheckPassed: fraudCheckPassed,
                dailyLimitCheckPassed: dailyLimitCheckPassed);
            
            var reason = !fraudCheckPassed ? "Fraud policy violation" : "Daily limit exceeded";
            throw new DomainException($"Reward redemption blocked: {reason}");
        }
        
        ValidateRewardRedemption(reward, storeId);
            
        var transaction = new Transaction
        (
            Id,
            TransactionType.RewardRedemption,
            rewardId: reward.Id,
            storeId: storeId,
            staffId: staffId
        );

        // Deduct balance
        if (Type == LoyaltyProgramType.Stamp)
            StampsCollected -= reward.RequiredValue;
        else
            PointsBalance -= reward.RequiredValue;

        _transactions.Add(transaction);
        MarkAsUpdated();

        // Raise domain event
        var rewardRedeemedEvent = new RewardRedeemedEvent(
            cardId: Id.Value,
            customerId: CustomerId.Value,
            programId: ProgramId.Value,
            rewardId: reward.Id.Value,
            storeId: storeId.Value,
            rewardTitle: reward.Title,
            rewardDescription: reward.Description ?? "",
            requiredValue: reward.RequiredValue,
            rewardType: Type == LoyaltyProgramType.Stamp ? "Stamps" : "Points",
            stampsBefore: stampsBefore,
            pointsBefore: pointsBefore,
            programName: programName,
            storeName: storeName,
            staffId: staffId?.Value,
            staffName: staffName,
            fraudCheckPassed: fraudCheckPassed,
            dailyLimitCheckPassed: dailyLimitCheckPassed,
            dailyRedemptionsBeforeTransaction: dailyRedemptionsBefore,
            dailyCurrencyValueBefore: dailyCurrencyBefore,
            currencyValue: currencyValue,
            currentTierName: currentTierName,
            newTierName: newTierName,
            nextRewardThreshold: nextRewardThreshold);
        
        AddDomainEvent(rewardRedeemedEvent);

        return transaction;
    }

    /// <summary>
    /// Validates if a transaction is allowed based on fraud policy
    /// </summary>
    public bool ValidateAgainstFraudPolicy(FraudPolicy fraudPolicy)
    {
        if (fraudPolicy == null) return true;

        var lastTransaction = _transactions.OrderByDescending(t => t.Timestamp).FirstOrDefault();
        if (lastTransaction == null) return true;

        var dailyTransactions = GetTodaysTransactions();
        var dailyRedemptions = dailyTransactions.Count(t => t.Type == TransactionType.RewardRedemption);
        var dailyPoints = dailyTransactions.Where(t => t.Type == TransactionType.PointsIssuance)
            .Sum(t => t.PointsAmount ?? 0);

        // Check cooldown period
        if (fraudPolicy.CooldownBetweenScans.HasValue)
        {
            var timeSinceLastTransaction = DateTime.UtcNow - lastTransaction.Timestamp;
            if (timeSinceLastTransaction < fraudPolicy.CooldownBetweenScans.Value)
                return false;
        }

        // Check daily redemption limits
        if (fraudPolicy.MaxRedemptionsPerDay.HasValue && 
            dailyRedemptions >= fraudPolicy.MaxRedemptionsPerDay.Value)
            return false;

        // Check velocity window points
        if (fraudPolicy.MaxPointsPerVelocityWindow.HasValue && 
            dailyPoints >= fraudPolicy.MaxPointsPerVelocityWindow.Value)
            return false;

        return true;
    }

    /// <summary>
    /// Validates if a transaction is within daily limits
    /// </summary>
    public bool ValidateAgainstDailyLimits(DailyLimits dailyLimits, int newStamps = 0, decimal newPoints = 0, decimal newCurrencyValue = 0)
    {
        if (dailyLimits == null) return true;

        var todaysTransactions = GetTodaysTransactions();
        var currentDailyStamps = todaysTransactions.Where(t => t.Type == TransactionType.StampIssuance)
            .Sum(t => t.Quantity ?? 0);
        var currentDailyPoints = todaysTransactions.Where(t => t.Type == TransactionType.PointsIssuance)
            .Sum(t => t.PointsAmount ?? 0);
        var currentDailyRedemptions = todaysTransactions.Count(t => t.Type == TransactionType.RewardRedemption);

        if (newStamps > 0 && !dailyLimits.IsStampTransactionAllowed(currentDailyStamps, newStamps))
            return false;

        if (newPoints > 0 && !dailyLimits.IsPointsTransactionAllowed((int)currentDailyPoints, (int)newPoints))
            return false;

        if (newCurrencyValue > 0 && !dailyLimits.IsRedemptionAllowed(currentDailyRedemptions))
            return false;

        return true;
    }

    /// <summary>
    /// Enforces all business rules and invariants
    /// </summary>
    public void EnforceBusinessRules()
    {
        // Card must not be expired
        if (ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value)
        {
            if (Status != CardStatus.Expired)
            {
                Status = CardStatus.Expired;
                UpdatedAt = DateTime.UtcNow;
            }
            throw new DomainException("Loyalty card has expired");
        }

        // Balances cannot be negative
        if (StampsCollected < 0)
            throw new DomainException("Stamps collected cannot be negative");

        if (PointsBalance < 0)
            throw new DomainException("Points balance cannot be negative");

        // QR Code must be present
        if (string.IsNullOrWhiteSpace(QrCode))
            throw new DomainException("Loyalty card must have a QR code");
    }

    /// <summary>
    /// Validates stamp issuance business rules
    /// </summary>
    private void ValidateStampIssuance(int quantity, StoreId storeId)
    {
        if (Type is not LoyaltyProgramType.Stamp)
            throw new DomainException("Cannot issue stamps to a points-based card");

        if (Status is not CardStatus.Active)
            throw new DomainException("Cannot issue stamps to an inactive card");

        if (quantity <= 0)
            throw new DomainException("Stamp quantity must be greater than zero");

        if (quantity > 100) // Business rule: max 100 stamps per transaction
            throw new DomainException("Cannot issue more than 100 stamps in a single transaction");

        if (storeId == Guid.Empty)
            throw new DomainException("Store ID cannot be empty");
    }

    /// <summary>
    /// Validates points issuance business rules
    /// </summary>
    private void ValidatePointsIssuance(decimal pointsAmount, decimal transactionAmount, StoreId storeId)
    {
        if (Type is not LoyaltyProgramType.Points)
            throw new DomainException("Cannot add points to a stamp-based card");

        if (Status is not CardStatus.Active)
            throw new DomainException("Cannot add points to an inactive card");

        if (pointsAmount <= 0)
            throw new DomainException("Points amount must be greater than zero");

        if (pointsAmount > 10000) // Business rule: max 10000 points per transaction
            throw new DomainException("Cannot issue more than 10000 points in a single transaction");

        if (transactionAmount < 0)
            throw new DomainException("Transaction amount cannot be negative");

        if (storeId == Guid.Empty)
            throw new DomainException("Store ID cannot be empty");
    }

    /// <summary>
    /// Validates reward redemption business rules
    /// </summary>
    private void ValidateRewardRedemption(Reward reward, StoreId storeId)
    {
        if (Status is not CardStatus.Active)
            throw new DomainException("Cannot redeem rewards with an inactive card");
            
        if (reward.ProgramId != ProgramId)
            throw new DomainException("Cannot redeem a reward from a different program");

        if (!reward.IsActive)
            throw new DomainException("Cannot redeem an inactive reward");

        if (!reward.IsValidAt(DateTime.UtcNow))
            throw new DomainException("Reward is not valid at this time");

        if (storeId == Guid.Empty)
            throw new DomainException("Store ID cannot be empty");

        // Validate sufficient balance
        switch (Type)
        {
            case LoyaltyProgramType.Stamp when StampsCollected < reward.RequiredValue:
                throw new DomainException("Insufficient stamps for reward redemption");
                
            case LoyaltyProgramType.Points when PointsBalance < reward.RequiredValue:
                throw new DomainException("Insufficient points for reward redemption");
        }
    }

    /// <summary>
    /// Gets today's transactions for fraud and limit validation
    /// </summary>
    private List<Transaction> GetTodaysTransactions()
    {
        var today = DateTime.UtcNow.Date;
        return _transactions.Where(t => t.Timestamp.Date == today).ToList();
    }
        
    public int GetStampsIssuedToday()
    {
        if (Type != LoyaltyProgramType.Stamp)
            return 0;

        return GetTodaysTransactions()
            .Where(t => t.Type == TransactionType.StampIssuance)
            .Sum(t => t.Quantity ?? 0);
    }

    public void Expire()
    {
        if (Status is CardStatus.Expired)
            return;

        Status = CardStatus.Expired;
        UpdatedAt = DateTime.UtcNow;
    }
        
    public void Suspend()
    {
        if (Status is CardStatus.Suspended)
            return;

        Status = CardStatus.Suspended;
        UpdatedAt = DateTime.UtcNow;
    }
        
    public void Reactivate()
    { 
        ArgumentNullException.ThrowIfNull(Status);
        Status = CardStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }
        
    public void SetExpirationDate(DateTime expirationDate)
    {
        ArgumentNullException.ThrowIfNull(expirationDate);
        ExpiresAt = expirationDate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateQrCode(string qrCode)
    {
        ArgumentNullException.ThrowIfNull(qrCode);
        QrCode = qrCode;
        UpdatedAt = DateTime.UtcNow;
    }
        
    public void AddTransaction(Transaction transaction) => 
        _transactions.Add(transaction);

    private string GenerateQrCode()
    {
        // In a real implementation, this would generate a unique QR code
        // For now, just use the loyalty card ID as the content
        return Id.ToString();
    }

    /// <summary>
    /// Validates fraud policy and returns whether the transaction passes
    /// </summary>
    private bool ValidateFraudPolicy(
        FraudPolicy? fraudPolicy, 
        string transactionType, 
        int? stamps = null, 
        decimal? pointsAmount = null)
    {
        if (fraudPolicy == null) return true;

        try
        {
            return ValidateAgainstFraudPolicy(fraudPolicy);
        }
        catch (DomainException)
        {
            return false;
        }
    }

    /// <summary>
    /// Validates daily limits and returns whether the transaction passes
    /// </summary>
    private bool ValidateDailyLimits(
        DailyLimits? dailyLimits, 
        int newStamps = 0, 
        decimal newPoints = 0, 
        decimal newCurrencyValue = 0)
    {
        if (dailyLimits == null) return true;

        try
        {
            return ValidateAgainstDailyLimits(dailyLimits, newStamps, newPoints, newCurrencyValue);
        }
        catch (DomainException)
        {
            return false;
        }
    }

    /// <summary>
    /// Raises a fraud detection event with detailed context
    /// </summary>
    private void RaiseFraudDetectedEvent(
        string transactionType,
        StoreId storeId,
        string programName,
        string storeName,
        FraudPolicy? fraudPolicy,
        DailyLimits? dailyLimits,
        StaffId? staffId = null,
        string? staffName = null,
        string? posTransactionId = null,
        int? attemptedStamps = null,
        decimal? attemptedPoints = null,
        Guid? attemptedRewardId = null,
        decimal? transactionAmount = null,
        bool fraudCheckPassed = true,
        bool dailyLimitCheckPassed = true)
    {
        var fraudReasons = new List<string>();
        var primaryReason = "";
        var severity = FraudSeverity.Low;

        if (!fraudCheckPassed)
        {
            fraudReasons.Add("Fraud policy violation");
            primaryReason = "Fraud policy violation";
            severity = FraudSeverity.High;
        }

        if (!dailyLimitCheckPassed)
        {
            fraudReasons.Add("Daily limit exceeded");
            if (string.IsNullOrEmpty(primaryReason))
                primaryReason = "Daily limit exceeded";
            severity = FraudSeverity.Medium;
        }

        var lastTransaction = _transactions.OrderByDescending(t => t.Timestamp).FirstOrDefault();
        var todaysTransactions = GetTodaysTransactions();

        var fraudEvent = new FraudAttemptDetectedEvent(
            cardId: Id.Value,
            customerId: CustomerId.Value,
            programId: ProgramId.Value,
            storeId: storeId.Value,
            transactionType: transactionType,
            primaryFraudReason: primaryReason,
            severity: severity,
            programName: programName,
            storeName: storeName,
            fraudReasons: fraudReasons,
            staffId: staffId?.Value,
            staffName: staffName,
            posTransactionId: posTransactionId,
            attemptedStamps: attemptedStamps,
            attemptedPoints: attemptedPoints,
            attemptedRewardId: attemptedRewardId,
            transactionAmount: transactionAmount,
            lastTransactionTime: lastTransaction?.Timestamp,
            requiredCooldown: fraudPolicy?.CooldownBetweenScans,
            dailyTransactionCount: todaysTransactions.Count,
            dailyRedemptionCount: todaysTransactions.Count(t => t.Type == TransactionType.RewardRedemption),
            dailyPointsEarned: todaysTransactions.Where(t => t.Type == TransactionType.PointsIssuance)
                .Sum(t => t.PointsAmount ?? 0),
            dailyCurrencyRedeemed: todaysTransactions.Where(t => t.Type == TransactionType.RewardRedemption)
                .Sum(t => t.TransactionAmount ?? 0));

        AddDomainEvent(fraudEvent);
    }
}