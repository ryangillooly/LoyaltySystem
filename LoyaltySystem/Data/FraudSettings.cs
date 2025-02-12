namespace LoyaltySystem.Data;

using LinqToDB.Mapping;
using System;

[Table("fraudsettings")]
public class FraudSettings
{
    [PrimaryKey, Identity]
    [Column("fraudsettingid")]
    public int FraudSettingId { get; set; }

    [Column("storeid"), NotNull]
    public int StoreId { get; set; }

    [Column("allowedlatitude"), NotNull]
    public decimal AllowedLatitude { get; set; }

    [Column("allowedlongitude"), NotNull]
    public decimal AllowedLongitude { get; set; }

    [Column("allowedradius"), NotNull]
    public int AllowedRadius { get; set; }

    [Column("operationalstart"), NotNull]
    public TimeSpan OperationalStart { get; set; }  // or DateTime if you prefer
    [Column("operationalend"), NotNull]
    public TimeSpan OperationalEnd { get; set; }

    [Column("minintervalminutes"), NotNull]
    public int MinIntervalMinutes { get; set; } = 240;

    [Column("createdat")]
    public DateTime CreatedAt { get; set; }

    [Column("updatedat")]
    public DateTime UpdatedAt { get; set; }
}