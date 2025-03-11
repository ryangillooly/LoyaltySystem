using System;

namespace LoyaltySystem.Application.DTOs
{
    public class StoreDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public AddressDto Address { get; set; } = new AddressDto();
        public ContactInfoDto ContactInfo { get; set; } = new ContactInfoDto();
        public GeoLocationDto Location { get; set; } = new GeoLocationDto();
        public string BrandId { get; set; } = string.Empty;
        public string BrandName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class AddressDto
    {
        public string Line1 { get; set; } = string.Empty;
        public string Line2 { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
    }

    public class ContactInfoDto
    {
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
    }

    public class GeoLocationDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class CreateStoreDto
    {
        public string Name { get; set; } = string.Empty;
        public AddressDto Address { get; set; } = new AddressDto();
        public ContactInfoDto ContactInfo { get; set; } = new ContactInfoDto();
        public GeoLocationDto Location { get; set; } = new GeoLocationDto();
        public string BrandId { get; set; } = string.Empty;
    }

    public class UpdateStoreDto
    {
        public string Name { get; set; } = string.Empty;
        public AddressDto Address { get; set; } = new AddressDto();
        public ContactInfoDto ContactInfo { get; set; } = new ContactInfoDto();
        public GeoLocationDto Location { get; set; } = new GeoLocationDto();
        public string BrandId { get; set; } = string.Empty;
    }

    public class StoreStatsDto
    {
        public int TransactionCount { get; set; }
        public int StampsIssued { get; set; }
        public decimal PointsIssued { get; set; }
        public int RedemptionCount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class OperatingHoursDto
    {
        public DayHoursDto Monday { get; set; } = new DayHoursDto();
        public DayHoursDto Tuesday { get; set; } = new DayHoursDto();
        public DayHoursDto Wednesday { get; set; } = new DayHoursDto();
        public DayHoursDto Thursday { get; set; } = new DayHoursDto();
        public DayHoursDto Friday { get; set; } = new DayHoursDto();
        public DayHoursDto Saturday { get; set; } = new DayHoursDto();
        public DayHoursDto Sunday { get; set; } = new DayHoursDto();
    }

    /// <summary>
    /// DTO for a day's operating hours.
    /// </summary>
    public class DayHoursDto
    {
        /// <summary>
        /// Whether the store is open on this day.
        /// </summary>
        public bool IsOpen { get; set; }
        
        /// <summary>
        /// Opening time (if open).
        /// </summary>
        public TimeSpan? OpenTime { get; set; }
        
        /// <summary>
        /// Closing time (if open).
        /// </summary>
        public TimeSpan? CloseTime { get; set; }
    }
} 