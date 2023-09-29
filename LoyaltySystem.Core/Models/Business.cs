using System.Diagnostics;
using LoyaltySystem.Core.Enums;

namespace LoyaltySystem.Core.Models;

public class Business
{
    public Business()
    {
        Id      = Guid.NewGuid();
        OwnerId = Guid.Empty;
    }

    public Business(Guid businessId)
    {
        Id = businessId;
    }

    public Business(Guid businessId, Guid ownerId)
    {
        Id      = businessId;
        OwnerId = ownerId;
    }
    
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Location Location { get; set; } = new ();
    public ContactInfo ContactInfo { get; set; } = new ();
    public OpeningHours OpeningHours { get; set; } = new ();
    public BusinessStatus Status { get; set; } = BusinessStatus.Active;

    public static Business Merge(Business current, Business updated) =>
        new ()
        {
            Id           = current.Id,
            OwnerId      = updated.OwnerId == Guid.Empty ? current.OwnerId : updated.OwnerId,
            Name         = string.IsNullOrEmpty(updated.Name) ? current.Name : updated.Name,
            Description  = updated.Description ?? current.Description,
            OpeningHours = updated.OpeningHours.Hours.Count == 0 ? current.OpeningHours : updated.OpeningHours,
            Status       = updated.Status == BusinessStatus.Active && updated.Status != current.Status ? updated.Status : current.Status,
            ContactInfo  = new ContactInfo
            {
                PhoneNumber = string.IsNullOrEmpty(updated.ContactInfo.PhoneNumber) ? current.ContactInfo.PhoneNumber : updated.ContactInfo.PhoneNumber,
                Email       = string.IsNullOrEmpty(updated.ContactInfo.Email) ? current.ContactInfo.Email : updated.ContactInfo.Email
            },
            Location     = new Location
            {
                Address   = updated.Location.Address == string.Empty      ? current.Location.Address : updated.Location.Address,
                Latitude  = updated.Location.Latitude == double.MinValue  ? current.Location.Latitude : updated.Location.Latitude,
                Longitude = updated.Location.Longitude == double.MinValue ? current.Location.Longitude : updated.Location.Longitude 
            }
        };
}