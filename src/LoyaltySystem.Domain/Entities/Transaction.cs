using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;

namespace LoyaltySystem.Domain.Entities;

public class Transaction : Entity<TransactionId> 
{
    internal Dictionary<string, string> _metadata;
    
    public LoyaltyCardId CardId { get; set; }
    public TransactionType Type { get; set; }
    public RewardId? RewardId { get; set; }
    public int? Quantity { get; set; }
    public decimal? PointsAmount { get; set; }
    public decimal? TransactionAmount { get; set; }
    public StoreId StoreId { get; set; }
    public StaffId? StaffId { get; set; }
    public string PosTransactionId { get; set; }
    public DateTime Timestamp { get; set; }

    public IReadOnlyDictionary<string, string> Metadata => _metadata;

    public Transaction
    (
        LoyaltyCardId cardId,
        TransactionType type,
        RewardId? rewardId = null,
        int? quantity = null,
        decimal? pointsAmount = null,
        decimal? transactionAmount = null,
        StoreId storeId = default,
        StaffId? staffId = null,
        string posTransactionId = null,
        Dictionary<string, string> metadata = null
    )
    : base(new TransactionId())
    {
        ValidateTransactionType(type, rewardId, quantity, pointsAmount);
        
        Type = type;
        CardId = cardId ?? throw new ArgumentNullException(nameof(cardId));
        StoreId = storeId ?? throw new ArgumentNullException(nameof(storeId));
        StaffId = staffId ?? throw new ArgumentNullException(nameof(staffId));
        RewardId = rewardId ?? throw new ArgumentNullException(nameof(rewardId));
        Quantity = quantity;
        Timestamp = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;
        PointsAmount = pointsAmount;
        PosTransactionId = posTransactionId;
        TransactionAmount = transactionAmount;
        
        _metadata = metadata;
    }

    public void AddMetadata(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Metadata key cannot be empty", nameof(key));

        _metadata[key] = value;
    }

    private void ValidateTransactionType
    (
        TransactionType type,
        Guid? rewardId,
        int? quantity,
        decimal? pointsAmount
    )
    {
        switch (type)
        {
            case TransactionType.StampIssuance:
            case TransactionType.StampVoid:
                if (!quantity.HasValue || quantity.Value <= 0)
                    throw new ArgumentException("Stamp transactions require a positive quantity", nameof(quantity));
                break;
                    
            case TransactionType.PointsIssuance:
            case TransactionType.PointsVoid:
                if (!pointsAmount.HasValue || pointsAmount.Value <= 0)
                    throw new ArgumentException("Points transactions require a positive points amount", nameof(pointsAmount));
                break;
                    
            case TransactionType.RewardRedemption:
                if (!rewardId.HasValue || rewardId.Value == Guid.Empty)
                    throw new ArgumentException("Reward redemption requires a valid reward ID", nameof(rewardId));
                break;
                    
            default:
                throw new ArgumentException("Invalid transaction type", nameof(type));
        }
    }
}