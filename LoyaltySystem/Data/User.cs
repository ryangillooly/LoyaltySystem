using LinqToDB.Mapping;
using System;

namespace LoyaltySystem.Data;

[Table("users")] // or whatever your table is named
public class User
{
    [PrimaryKey, Identity]
    [Column("userid")]
    public int UserId { get; set; }

    [Column("email"), NotNull]
    public string Email { get; set; }

    [Column("passwordhash"), NotNull]
    public string PasswordHash { get; set; }

    [Column("firstname")]
    public string FirstName { get; set; }

    [Column("lastname")]
    public string LastName { get; set; }

    [Column("createdat")]
    public DateTime CreatedAt { get; set; }
}