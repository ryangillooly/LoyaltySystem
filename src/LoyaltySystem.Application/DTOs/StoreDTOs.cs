using LoyaltySystem.Domain.Entities;
using System;

namespace LoyaltySystem.Application.DTOs;

public class StoreDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Address Address { get; set; }
    public ContactInfo ContactInfo { get; set; }
    public OperatingHours OperatingHours { get; private set; }
    public GeoLocation Location { get; set; }
    public string BrandId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateStoreDto
{
    public string Name { get; set; } = string.Empty;
    public Address Address { get; set; } 
    public ContactInfo ContactInfo { get; set; } 
    public GeoLocation Location { get; set; } 
    public OperatingHours OperatingHours { get; set; }
    public string BrandId { get; set; } = string.Empty;
}

public class UpdateStoreDto
{
    public string Name { get; set; } = string.Empty;
    public Address Address { get; set; } 
    public ContactInfo ContactInfo { get; set; }
    public GeoLocation Location { get; set; }
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
    public DayHours Monday { get; set; } = new ();
    public DayHours Tuesday { get; set; } = new ();
    public DayHours Wednesday { get; set; } = new ();
    public DayHours Thursday { get; set; } = new ();
    public DayHours Friday { get; set; } = new ();
    public DayHours Saturday { get; set; } = new ();
    public DayHours Sunday { get; set; } = new ();
}

/// <summary>
/// DTO for a day's operating hours.
/// </summary>
public class DayHours
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
