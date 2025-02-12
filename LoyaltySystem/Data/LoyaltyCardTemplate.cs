namespace LoyaltySystem.Data;

using LinqToDB.Mapping;
using System;

[Table("loyaltycardtemplates")]
public class LoyaltyCardTemplate
{
    [PrimaryKey, Identity]
    [Column("loyaltycardtemplateid")]
    public int LoyaltyCardTemplateId { get; set; }

    [Column("businessid"), NotNull]
    public int BusinessId { get; set; }

    [Column("cardtype"), NotNull]
    public string CardType { get; set; }  // e.g. "RegularStampCard", "DigitalStampPassport"

    [Column("cardname"), NotNull]
    public string CardName { get; set; }

    [Column("requiredstamps")]
    public int RequiredStamps { get; set; }

    [Column("minimumspendcondition")]
    public decimal? MinimumSpendCondition { get; set; }

    [Column("description")]
    public string Description { get; set; }

    [Column("resetaftercompletion")]
    public bool ResetAfterCompletion { get; set; }

    [Column("createdat")]
    public DateTime CreatedAt { get; set; }

    [Column("updatedat")]
    public DateTime UpdatedAt { get; set; }
}