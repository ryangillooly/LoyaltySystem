namespace LoyaltySystem.Data;

using LinqToDB.Mapping;
using System;

[Table("businesses")]
public class Business
{
    [PrimaryKey, Identity]
    [Column("businessid")]
    public int BusinessId { get; set; }

    [Column("businessname"), NotNull]
    public string BusinessName { get; set; }

    [Column("category")]
    public string Category { get; set; }

    [Column("websiteurl")]
    public string WebsiteUrl { get; set; }

    [Column("description")]
    public string Description { get; set; }

    [Column("logourl")]
    public string LogoUrl { get; set; }

    [Column("coverimageurl")]
    public string CoverImageUrl { get; set; }

    [Column("createdat")]
    public DateTime CreatedAt { get; set; }

    [Column("updatedat")]
    public DateTime UpdatedAt { get; set; }
}