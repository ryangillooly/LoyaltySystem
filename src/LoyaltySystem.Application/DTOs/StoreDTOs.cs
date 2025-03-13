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

    public partial class AddressDto
    {
        // ... existing code ...
    }

    public partial class ContactInfoDto
    {
        // ... existing code ...
    }

    public partial class GeoLocationDto
    {
        // ... existing code ...
    }
} 