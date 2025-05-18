using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;

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
    }
        
    public Transaction IssueStamps
    (
        int quantity,
        StoreId storeId,
        StaffId? staffId = null,
        string posTransactionId = null
    )
    {
        if (Type is not LoyaltyProgramType.Stamp)
            throw new InvalidOperationException("Cannot issue stamps to a points-based card");

        if (Status is not CardStatus.Active)
            throw new InvalidOperationException("Cannot issue stamps to an inactive card");

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

        if (storeId == Guid.Empty)
            throw new ArgumentException("Store ID cannot be empty", nameof(storeId));
            
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
        UpdatedAt = DateTime.UtcNow;

        return transaction;
    }
        
    public Transaction AddPoints
    (
        decimal pointsAmount,
        decimal transactionAmount,
        StoreId storeId,
        StaffId? staffId = null,
        string posTransactionId = null
    )
    {
        if (Type is not LoyaltyProgramType.Points)
            throw new InvalidOperationException("Cannot add points to a stamp-based card");

        if (Status is not CardStatus.Active)
            throw new InvalidOperationException("Cannot add points to an inactive card");

        if (pointsAmount <= 0)
            throw new ArgumentException("Points amount must be greater than zero", nameof(pointsAmount));

        if (transactionAmount < 0)
            throw new ArgumentException("Transaction amount cannot be negative", nameof(transactionAmount));

        if (storeId == Guid.Empty)
            throw new ArgumentException("Store ID cannot be empty", nameof(storeId));

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
        UpdatedAt = DateTime.UtcNow;

        return transaction;
    }
        
    public Transaction RedeemReward
    (
        Reward reward,
        StoreId storeId,
        StaffId? staffId = null
    )
    {
        ArgumentNullException.ThrowIfNull(reward);
        ArgumentNullException.ThrowIfNull(storeId);
            
        if (Status is not CardStatus.Active)
            throw new InvalidOperationException("Cannot redeem rewards with an inactive card");
            
        if (reward.ProgramId != ProgramId)
            throw new InvalidOperationException("Cannot redeem a reward from a different program");

        if (!reward.IsActive)
            throw new InvalidOperationException("Cannot redeem an inactive reward");

        if (!reward.IsValidAt(DateTime.UtcNow))
            throw new InvalidOperationException("Reward is not valid at this time");

        if (storeId == Guid.Empty)
            throw new ArgumentException("Store ID cannot be empty", nameof(storeId));

        switch (Type)
        {
            // Validate sufficient balance
            case LoyaltyProgramType.Stamp when StampsCollected < reward.RequiredValue:
                throw new InvalidOperationException("Insufficient stamps for reward redemption");
                
            case LoyaltyProgramType.Points when PointsBalance < reward.RequiredValue:
                throw new InvalidOperationException("Insufficient points for reward redemption");
        }
            
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
        UpdatedAt = DateTime.UtcNow;

        return transaction;
    }
        
    public int GetStampsIssuedToday()
    {
        if (Type != LoyaltyProgramType.Stamp)
            return 0;

        var today = DateTime.UtcNow.Date;
            
        return _transactions
            .Where(t => t.Type == TransactionType.StampIssuance && t.Timestamp.Date == today)
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
}