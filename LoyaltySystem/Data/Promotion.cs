namespace LoyaltySystem.Data;

using LinqToDB.Mapping;
using System;

[Table("promotions")]
public class Promotion
{
    [PrimaryKey, Identity]
    [Column("promotionid")]
    public int PromotionId { get; set; }

    [Column("businessid"), NotNull]
    public int BusinessId { get; set; }

    [Column("title"), NotNull]
    public string Title { get; set; }

    [Column("description")]
    public string Description { get; set; }

    [Column("validfrom")]
    public DateTime ValidFrom { get; set; }

    [Column("validuntil")]
    public DateTime ValidUntil { get; set; }

    [Column("createdat")]
    public DateTime CreatedAt { get; set; }

    [Column("updatedat")]
    public DateTime UpdatedAt { get; set; }
}