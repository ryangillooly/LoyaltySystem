namespace LoyaltySystem.Data;

using LinqToDB.Mapping;
using System;

[Table("members")]  // Matches the 'Members' table
public class Member
{
    [PrimaryKey, Identity]
    [Column("memberid")]
    public int MemberId { get; set; }

    [Column("businessid"), NotNull]
    public int BusinessId { get; set; }

    [Column("name"), NotNull]
    public string Name { get; set; }

    [Column("email"), NotNull]
    public string Email { get; set; }

    [Column("joinedat")]
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}