namespace LoyaltySystem.Data;

using LinqToDB.Mapping;
using System;

[Table("redemptiontransactions")]
public class RedemptionTransaction
{
    [PrimaryKey, Identity]
    [Column("redemptiontransactionid")]
    public int RedemptionTransactionId { get; set; }

    [Column("userloyaltycardid"), NotNull]
    public int UserLoyaltyCardId { get; set; }

    [Column("rewardid"), NotNull]
    public int RewardId { get; set; }

    [Column("storeid"), NotNull]
    public int StoreId { get; set; }

    [Column("businessid"), NotNull]
    public int BusinessId { get; set; }

    [Column("timestamp"), NotNull]
    public DateTime Timestamp { get; set; }
}