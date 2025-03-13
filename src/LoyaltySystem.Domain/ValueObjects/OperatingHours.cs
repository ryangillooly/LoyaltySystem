using System;
using System.Collections.Generic;
using System.Linq;

namespace LoyaltySystem.Domain.Entities
{
    /// <summary>
    /// Value object representing business operating hours.
    /// </summary>
    public class OperatingHours
    {
        private readonly Dictionary<DayOfWeek, TimeRange> _hours;

        public IReadOnlyDictionary<DayOfWeek, TimeRange> Hours
        {
            get => _hours;
            set => throw new NotImplementedException();
        }

        // Private constructor for EF Core
        public OperatingHours() =>
            _hours = new Dictionary<DayOfWeek, TimeRange>();

        public OperatingHours(Dictionary<DayOfWeek, TimeRange> hours) =>
            _hours = hours ?? new Dictionary<DayOfWeek, TimeRange>();

        /// <summary>
        /// Sets the operating hours for a specific day of the week.
        /// </summary>
        public void SetHoursForDay(DayOfWeek day, TimeSpan openTime, TimeSpan closeTime)
        {
            _hours[day] = new TimeRange(openTime, closeTime);
        }

        /// <summary>
        /// Checks if the store is open at the specified time.
        /// </summary>
        public bool IsOpenAt(DateTime time)
        {
            var localTime = time.ToLocalTime();
            var dayOfWeek = localTime.DayOfWeek;
            
            if (!_hours.TryGetValue(dayOfWeek, out var timeRange))
                return false; // No hours defined for this day
                
            return timeRange.Includes(localTime.TimeOfDay);
        }

        /// <summary>
        /// Gets the opening hours for a specific day, or null if closed.
        /// </summary>
        public TimeRange GetHoursForDay(DayOfWeek day)
        {
            _hours.TryGetValue(day, out var timeRange);
            return timeRange;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var other = (OperatingHours)obj;
            
            if (_hours.Count != other._hours.Count)
                return false;
                
            return _hours.All(kvp => 
                other._hours.TryGetValue(kvp.Key, out var otherValue) && 
                Equals(kvp.Value, otherValue));
        }

        public override int GetHashCode()
        {
            return _hours.Aggregate(0, (hash, kvp) => 
                hash ^ kvp.Key.GetHashCode() ^ (kvp.Value?.GetHashCode() ?? 0));
        }
    }

    /// <summary>
    /// Represents a time range from open to close within a day.
    /// </summary>
    public class TimeRange
    {
        public TimeSpan OpenTime { get; private set; }
        public TimeSpan CloseTime { get; private set; }

        // Private constructor for EF Core
        private TimeRange() { }

        public TimeRange(TimeSpan openTime, TimeSpan closeTime)
        {
            // Ensure the times are valid
            if (openTime.TotalHours < 0 || openTime.TotalHours >= 24)
                throw new ArgumentOutOfRangeException(nameof(openTime), "Open time must be between 0 and 24 hours.");

            if (closeTime.TotalHours < 0 || closeTime.TotalHours > 24)
                throw new ArgumentOutOfRangeException(nameof(closeTime), "Close time must be between 0 and 24 hours.");

            OpenTime = openTime;
            CloseTime = closeTime;
        }

        /// <summary>
        /// Checks if the time range includes the specified time.
        /// Handles both regular ranges and overnight ranges (where close time is earlier than open time).
        /// </summary>
        public bool Includes(TimeSpan time)
        {
            // Regular case (e.g. 9:00 - 17:00)
            if (OpenTime < CloseTime)
                return time >= OpenTime && time < CloseTime;
                
            // Overnight case (e.g. 22:00 - 02:00)
            return time >= OpenTime || time < CloseTime;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var other = (TimeRange)obj;
            return OpenTime == other.OpenTime && CloseTime == other.CloseTime;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(OpenTime, CloseTime);
        }

        public override string ToString()
        {
            return $"{OpenTime.ToString(@"hh\:mm")} - {CloseTime.ToString(@"hh\:mm")}";
        }
    }
} 