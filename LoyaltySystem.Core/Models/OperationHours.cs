namespace LoyaltySystem.Core.Models;

public class TimeRange
{
    public TimeSpan? OpenTime { get; set; }
    public TimeSpan? CloseTime { get; set; }
}

public class OpeningHours
{
    public Dictionary<DayOfWeek, TimeRange> Hours { get; set; }

    public OpeningHours()
    {
        Hours = new Dictionary<DayOfWeek, TimeRange>();
    }
}
