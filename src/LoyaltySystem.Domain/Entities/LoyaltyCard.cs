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
        
    /// <summary>
    /// Initializes a new loyalty card for a customer in a specified loyalty program.
    /// </summary>
    /// <param name="programId">The identifier of the loyalty program.</param>
    /// <param name="customerId">The identifier of the customer.</param>
    /// <param name="type">The type of loyalty program (stamp-based or points-based).</param>
    /// <param name="expiresAt">Optional expiration date for the card.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="programId"/> or <paramref name="customerId"/> is null.</exception>
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
        
    /// <summary>
    /// Issues a specified number of stamps to a stamp-based loyalty card and records the transaction.
    /// </summary>
    /// <param name="quantity">The number of stamps to issue. Must be greater than zero.</param>
    /// <param name="storeId">The identifier of the store where the stamps are issued. Must not be empty.</param>
    /// <param name="staffId">Optional identifier of the staff member issuing the stamps.</param>
    /// <param name="posTransactionId">Optional POS transaction identifier associated with the issuance.</param>
    /// <returns>The transaction representing the stamp issuance.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the card is not stamp-based or is not active.</exception>
    /// <exception cref="ArgumentException">Thrown if the quantity is not positive or the store ID is empty.</exception>
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
        
    /// <summary>
    /// Adds points to a points-based loyalty card and records the issuance as a transaction.
    /// </summary>
    /// <param name="pointsAmount">The number of points to add. Must be greater than zero.</param>
    /// <param name="transactionAmount">The monetary amount associated with the transaction. Must not be negative.</param>
    /// <param name="storeId">The identifier of the store where the transaction occurred. Must not be empty.</param>
    /// <param name="staffId">Optional identifier of the staff member who processed the transaction.</param>
    /// <param name="posTransactionId">Optional POS transaction identifier.</param>
    /// <returns>The transaction representing the points issuance.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the card is not points-based or is not active.</exception>
    /// <exception cref="ArgumentException">Thrown if pointsAmount is not positive, transactionAmount is negative, or storeId is empty.</exception>
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
        
    /// <summary>
    /// Redeems a reward using the card's stamps or points, deducting the required value and recording the transaction.
    /// </summary>
    /// <param name="reward">The reward to redeem. Must belong to the same program and be active and valid.</param>
    /// <param name="storeId">The store where the redemption occurs. Must not be empty.</param>
    /// <param name="staffId">Optional staff member involved in the redemption.</param>
    /// <returns>The transaction representing the reward redemption.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="reward"/> or <paramref name="storeId"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="storeId"/> is empty.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the card is not active, the reward is from a different program, the reward is inactive or not valid,
    /// or if the card does not have sufficient stamps or points for redemption.
    /// </exception>
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
        
    /// <summary>
    /// Returns the total number of stamps issued to this card today, or zero if the card is not stamp-based.
    /// </summary>
    /// <returns>The number of stamps issued today.</returns>
    public int GetStampsIssuedToday()
    {
        if (Type != LoyaltyProgramType.Stamp)
            return 0;

        var today = DateTime.UtcNow.Date;
            
        return _transactions
            .Where(t => t.Type == TransactionType.StampIssuance && t.Timestamp.Date == today)
            .Sum(t => t.Quantity ?? 0);
    }

    /// <summary>
    /// Sets the card status to Expired if it is not already expired.
    /// </summary>
    public void Expire()
    {
        if (Status is CardStatus.Expired)
            return;

        Status = CardStatus.Expired;
        UpdatedAt = DateTime.UtcNow;
    }
        
    /// <summary>
    /// Suspends the loyalty card if it is not already suspended.
    /// </summary>
    public void Suspend()
    {
        if (Status is CardStatus.Suspended)
            return;

        Status = CardStatus.Suspended;
        UpdatedAt = DateTime.UtcNow;
    }
        
    /// <summary>
    /// Reactivates the loyalty card by setting its status to Active and updating the timestamp.
    /// </summary>
    public void Reactivate()
    { 
        ArgumentNullException.ThrowIfNull(Status);
        Status = CardStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }
        
    /// <summary>
    /// Sets the expiration date for the loyalty card.
    /// </summary>
    /// <param name="expirationDate">The date and time when the card will expire.</param>
    public void SetExpirationDate(DateTime expirationDate)
    {
        ArgumentNullException.ThrowIfNull(expirationDate);
        ExpiresAt = expirationDate;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the QR code associated with the loyalty card.
    /// </summary>
    /// <param name="qrCode">The new QR code string to assign to the card.</param>
    public void UpdateQrCode(string qrCode)
    {
        ArgumentNullException.ThrowIfNull(qrCode);
        QrCode = qrCode;
        UpdatedAt = DateTime.UtcNow;
    }
        
    /// <summary>
        /// Adds a transaction to the loyalty card's transaction history.
        /// </summary>
        /// <param name="transaction">The transaction to add.</param>
        public void AddTransaction(Transaction transaction) => 
        _transactions.Add(transaction);

    /// <summary>
    /// Generates a QR code string representing the loyalty card's unique identifier.
    /// </summary>
    /// <returns>The loyalty card ID as a string.</returns>
    private string GenerateQrCode()
    {
        // In a real implementation, this would generate a unique QR code
        // For now, just use the loyalty card ID as the content
        return Id.ToString();
    }
}