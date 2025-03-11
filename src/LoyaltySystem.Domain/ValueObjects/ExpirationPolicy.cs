using System;

namespace LoyaltySystem.Domain.Entities
{
    /// <summary>
    /// Value object representing the policy for how loyalty (stamps/points) expires.
    /// </summary>
    public class ExpirationPolicy
    {
        /// <summary>
        /// Whether loyalty expires at all.
        /// </summary>
        public bool HasExpiration { get; private set; }
        
        /// <summary>
        /// Type of expiration period (days, months, years).
        /// </summary>
        public ExpirationType ExpirationType { get; private set; }
        
        /// <summary>
        /// Value for the expiration period.
        /// </summary>
        public int ExpirationValue { get; private set; }
        
        /// <summary>
        /// Whether expiration occurs on a specific day.
        /// </summary>
        public bool ExpiresOnSpecificDate { get; private set; }
        
        /// <summary>
        /// Specific day of month for expiration (if applicable).
        /// </summary>
        public int? ExpirationDay { get; private set; }
        
        /// <summary>
        /// Specific month for expiration (if applicable).
        /// </summary>
        public int? ExpirationMonth { get; private set; }

        /// <summary>
        /// Create a policy with no expiration.
        /// </summary>
        public ExpirationPolicy()
        {
            HasExpiration = false;
            ExpirationType = ExpirationType.Days;
            ExpirationValue = 0;
            ExpiresOnSpecificDate = false;
        }

        /// <summary>
        /// Create a policy with expiration after a certain period.
        /// </summary>
        public ExpirationPolicy(ExpirationType expirationType, int expirationValue)
        {
            if (expirationValue <= 0)
                throw new ArgumentException("Expiration value must be greater than zero", nameof(expirationValue));

            HasExpiration = true;
            ExpirationType = expirationType;
            ExpirationValue = expirationValue;
            ExpiresOnSpecificDate = false;
        }

        /// <summary>
        /// Create a policy with expiration on a specific date.
        /// </summary>
        public ExpirationPolicy(int expirationDay, int? expirationMonth = null)
        {
            if (expirationDay <= 0 || expirationDay > 31)
                throw new ArgumentOutOfRangeException(nameof(expirationDay), "Day must be between 1 and 31");

            if (expirationMonth.HasValue && (expirationMonth.Value <= 0 || expirationMonth.Value > 12))
                throw new ArgumentOutOfRangeException(nameof(expirationMonth), "Month must be between 1 and 12");

            HasExpiration = true;
            ExpirationType = ExpirationType.Years;
            ExpirationValue = 1;
            ExpiresOnSpecificDate = true;
            ExpirationDay = expirationDay;
            ExpirationMonth = expirationMonth;
        }

        /// <summary>
        /// Calculate the expiration date based on a reference date.
        /// </summary>
        public DateTime CalculateExpirationDate(DateTime referenceDate)
        {
            if (!HasExpiration)
                return DateTime.MaxValue;

            if (!ExpiresOnSpecificDate)
            {
                return ExpirationType switch
                {
                    ExpirationType.Days => referenceDate.AddDays(ExpirationValue),
                    ExpirationType.Months => referenceDate.AddMonths(ExpirationValue),
                    ExpirationType.Years => referenceDate.AddYears(ExpirationValue),
                    _ => throw new InvalidOperationException("Unknown expiration type")
                };
            }
            else
            {
                // Handle expiration on specific date
                var month = ExpirationMonth ?? referenceDate.Month;
                var day = ExpirationDay.Value;
                
                // Ensure valid day for month
                day = Math.Min(day, DateTime.DaysInMonth(referenceDate.Year, month));
                
                // Start with same year
                var expirationDate = new DateTime(referenceDate.Year, month, day);
                
                // If the date has already passed this year, move to next year
                if (expirationDate <= referenceDate)
                    expirationDate = expirationDate.AddYears(1);
                    
                return expirationDate;
            }
        }

        /// <summary>
        /// Check if two policies are equal.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var other = (ExpirationPolicy)obj;
            
            if (!HasExpiration && !other.HasExpiration)
                return true;  // Both don't have expiration
                
            if (HasExpiration != other.HasExpiration)
                return false;  // One has expiration, the other doesn't
                
            if (!ExpiresOnSpecificDate && !other.ExpiresOnSpecificDate)
                return ExpirationType == other.ExpirationType && 
                       ExpirationValue == other.ExpirationValue;
                       
            if (ExpiresOnSpecificDate != other.ExpiresOnSpecificDate)
                return false;  // Different expiration mechanisms
                
            // Both expire on specific date
            return ExpirationDay == other.ExpirationDay && 
                   ExpirationMonth == other.ExpirationMonth;
        }

        /// <summary>
        /// Get hash code for the policy.
        /// </summary>
        public override int GetHashCode()
        {
            if (!HasExpiration)
                return 0;
                
            if (!ExpiresOnSpecificDate)
                return HashCode.Combine(HasExpiration, ExpirationType, ExpirationValue);
                
            return HashCode.Combine(HasExpiration, ExpiresOnSpecificDate, ExpirationDay, ExpirationMonth);
        }
    }

    /// <summary>
    /// Defines the type of expiration period.
    /// </summary>
    public enum ExpirationType
    {
        Days = 1,
        Months = 2,
        Years = 3
    }
} 