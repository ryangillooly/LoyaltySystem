namespace LoyaltySystem.Data;

using LinqToDB.Mapping;
using System;

[Table("userloyaltycards")]
public class UserLoyaltyCard
{
    [PrimaryKey, Identity]
    [Column("userloyaltycardid")]
    public int UserLoyaltyCardId { get; set; }

    [Column("userid"), NotNull]
    public int UserId { get; set; }

    [Column("loyaltycardtemplateid"), NotNull]
    public int LoyaltyCardTemplateId { get; set; }

    [Column("businessid"), NotNull]
    public int BusinessId { get; set; }

    [Column("currentstampcount")]
    public int CurrentStampCount { get; set; }

    [Column("status"), NotNull]
    public string Status { get; set; } // e.g. "Active", "Completed"

    [Column("createdat")]
    public DateTime CreatedAt { get; set; }

    [Column("updatedat")]
    public DateTime UpdatedAt { get; set; }
}