namespace LoyaltySystem.Data;

using LinqToDB.Mapping;
using System;

[Table("stamptransaction")]
public class StampTransaction
{
    [PrimaryKey, Identity]
    [Column("stamptransactionid")]
    public int StampTransactionId { get; set; }

    [Column("userloyaltycardid"), NotNull]
    public int UserLoyaltyCardId { get; set; }

    [Column("storeid"), NotNull]
    public int StoreId { get; set; }

    [Column("businessid"), NotNull]
    public int BusinessId { get; set; }

    [Column("timestamp"), NotNull]
    public DateTime Timestamp { get; set; }

    [Column("geolocation")]
    public string GeoLocation { get; set; }
}