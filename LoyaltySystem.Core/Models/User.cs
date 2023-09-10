using LoyaltySystem.Core.Enums;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltySystem.Core.Models;

public class User
{
    // public Guid Id { get; set; }
    public ContactInfo ContactInfo { get; set; } = new ();
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public List<UserPermission> Permissions { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Active;
    
    public string GetFullName => $"{FirstName} {LastName}";
}