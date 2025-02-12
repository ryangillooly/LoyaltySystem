namespace LoyaltySystem.Data;

using LinqToDB.Mapping;
using System;

[Table("businessusers")]
public class BusinessUser
{
    [PrimaryKey, Identity]
    [Column("businessuserid")]
    public int BusinessUserId { get; set; }

    [Column("businessid"), NotNull]
    public int BusinessId { get; set; }

    [Column("userid"), NotNull]
    public int UserId { get; set; } // or string if using GUID IDs

    [Column("role"), NotNull]
    public string Role { get; set; }

    [Column("createdat")]
    public DateTime CreatedAt { get; set; }
}