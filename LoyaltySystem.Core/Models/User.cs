using LoyaltySystem.Core.Convertors;
using LoyaltySystem.Core.Enums;
using Newtonsoft.Json;

namespace LoyaltySystem.Core.Models;

// [JsonConverter(typeof(UserConverter))]
public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public ContactInfo ContactInfo { get; set; } = new ();
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly? DateOfBirth { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Pending;
    public bool IsNotActive() => Status != UserStatus.Active;
    
    public static User Merge(User current, User updated) =>
        new ()
        {
            Id          = current.Id,
            FirstName   = updated.FirstName == string.Empty ? current.FirstName : updated.FirstName,
            LastName    = updated.LastName  == string.Empty ? current.LastName  : updated.LastName,
            DateOfBirth = updated.DateOfBirth ?? current.DateOfBirth,
            Status      = updated.Status != current.Status ? updated.Status : current.Status,
            ContactInfo  = new ContactInfo
            {
                PhoneNumber = string.IsNullOrEmpty(updated.ContactInfo.PhoneNumber) ? current.ContactInfo.PhoneNumber : updated.ContactInfo.PhoneNumber,
                Email       = string.IsNullOrEmpty(updated.ContactInfo.Email) ? current.ContactInfo.Email : updated.ContactInfo.Email
            }
        };
}