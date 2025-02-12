namespace LoyaltySystem.Data;

using LinqToDB.Mapping;
using System;

[Table("rewards")]
public class Reward
{
    [PrimaryKey, Identity]
    [Column("rewardid")]
    public int RewardId { get; set; }

    [Column("businessid"), NotNull]
    public int BusinessId { get; set; }

    [Column("rewardtitle"), NotNull]
    public string RewardTitle { get; set; }

    [Column("whatyouget"), NotNull]
    public string WhatYouGet { get; set; }

    [Column("fineprint")]
    public string FinePrint { get; set; }

    [Column("rewardimageurl")]
    public string RewardImageUrl { get; set; }

    [Column("isgift")]
    public bool IsGift { get; set; }

    [Column("rewardtype"), NotNull]
    public string RewardType { get; set; }  // e.g. "Regular", "Birthday", etc.

    [Column("validitystart")]
    public DateTime? ValidityStart { get; set; }

    [Column("validityend")]
    public DateTime? ValidityEnd { get; set; }

    [Column("createdat")]
    public DateTime CreatedAt { get; set; }

    [Column("updatedat")]
    public DateTime UpdatedAt { get; set; }
}