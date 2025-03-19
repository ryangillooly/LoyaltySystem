using System;

namespace LoyaltySystem.Domain.ValueObjects
{
    /// <summary>
    /// Value object representing the configuration for a points-based loyalty program.
    /// </summary>
    public class PointsConfig
    {
        /// <summary>
        /// Number of points awarded per dollar spent.
        /// </summary>
        public decimal PointsPerPound { get; private set; }

        /// <summary>
        /// Minimum number of points required for redemption.
        /// </summary>
        public int MinimumPointsForRedemption { get; private set; }

        /// <summary>
        /// How points should be rounded when calculated.
        /// </summary>
        public PointsRoundingRule RoundingRule { get; private set; }

        /// <summary>
        /// Enrollment bonus points awarded when a customer joins the program.
        /// </summary>
        public int EnrollmentBonusPoints { get; private set; }

        /// <summary>
        /// Default constructor with reasonable defaults.
        /// </summary>
        public PointsConfig()
        {
            PointsPerPound = 1;
            MinimumPointsForRedemption = 100;
            RoundingRule = PointsRoundingRule.RoundDown;
            EnrollmentBonusPoints = 0;
        }

        /// <summary>
        /// Create a new points configuration.
        /// </summary>
        public PointsConfig(
            decimal pointsPerPound,
            int minimumPointsForRedemption, 
            PointsRoundingRule roundingRule = PointsRoundingRule.RoundDown,
            int enrollmentBonusPoints = 0)
        {
            if (pointsPerPound <= 0)
                throw new ArgumentException("Points per pound must be greater than zero", nameof(pointsPerPound));

            if (minimumPointsForRedemption < 0)
                throw new ArgumentException("Minimum points for redemption cannot be negative", nameof(minimumPointsForRedemption));

            if (enrollmentBonusPoints < 0)
                throw new ArgumentException("Enrollment bonus points cannot be negative", nameof(enrollmentBonusPoints));

            PointsPerPound = pointsPerPound;
            MinimumPointsForRedemption = minimumPointsForRedemption;
            RoundingRule = roundingRule;
            EnrollmentBonusPoints = enrollmentBonusPoints;
        }

        /// <summary>
        /// Calculate points for a given transaction amount.
        /// </summary>
        public decimal CalculatePoints(decimal transactionAmount, decimal tierMultiplier = 1.0m)
        {
            if (transactionAmount < 0)
                throw new ArgumentException("Transaction amount cannot be negative", nameof(transactionAmount));

            decimal rawPoints = transactionAmount * PointsPerPound * tierMultiplier;

            return RoundingRule switch
            {
                PointsRoundingRule.RoundDown => Math.Floor(rawPoints),
                PointsRoundingRule.RoundUp => Math.Ceiling(rawPoints),
                PointsRoundingRule.RoundToNearest => Math.Round(rawPoints),
                _ => Math.Floor(rawPoints) // Default to round down
            };
        }

        /// <summary>
        /// Check if two PointsConfig objects are equal.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var other = (PointsConfig)obj;
            return PointsPerPound == other.PointsPerPound
                && MinimumPointsForRedemption == other.MinimumPointsForRedemption
                && RoundingRule == other.RoundingRule
                && EnrollmentBonusPoints == other.EnrollmentBonusPoints;
        }

        /// <summary>
        /// Get hash code for this object.
        /// </summary>
        public override int GetHashCode()
        {
            return HashCode.Combine(PointsPerPound, MinimumPointsForRedemption, RoundingRule, EnrollmentBonusPoints);
        }
    }

    /// <summary>
    /// Defines how points should be rounded when calculated.
    /// </summary>
    public enum PointsRoundingRule
    {
        /// <summary>
        /// Always round down to the nearest whole point.
        /// </summary>
        RoundDown = 0,

        /// <summary>
        /// Always round up to the nearest whole point.
        /// </summary>
        RoundUp = 1,

        /// <summary>
        /// Round to the nearest whole point.
        /// </summary>
        RoundToNearest = 2
    }
} 