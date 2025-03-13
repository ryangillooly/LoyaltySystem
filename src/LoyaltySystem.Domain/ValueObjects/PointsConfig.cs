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
        public decimal PointsPerDollar { get; private set; }

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
            PointsPerDollar = 1;
            MinimumPointsForRedemption = 100;
            RoundingRule = PointsRoundingRule.RoundDown;
            EnrollmentBonusPoints = 0;
        }

        /// <summary>
        /// Create a new points configuration.
        /// </summary>
        public PointsConfig(
            decimal pointsPerDollar,
            int minimumPointsForRedemption, 
            PointsRoundingRule roundingRule = PointsRoundingRule.RoundDown,
            int enrollmentBonusPoints = 0)
        {
            if (pointsPerDollar <= 0)
                throw new ArgumentException("Points per dollar must be greater than zero", nameof(pointsPerDollar));

            if (minimumPointsForRedemption < 0)
                throw new ArgumentException("Minimum points for redemption cannot be negative", nameof(minimumPointsForRedemption));

            if (enrollmentBonusPoints < 0)
                throw new ArgumentException("Enrollment bonus points cannot be negative", nameof(enrollmentBonusPoints));

            PointsPerDollar = pointsPerDollar;
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

            decimal rawPoints = transactionAmount * PointsPerDollar * tierMultiplier;

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
            return PointsPerDollar == other.PointsPerDollar
                && MinimumPointsForRedemption == other.MinimumPointsForRedemption
                && RoundingRule == other.RoundingRule
                && EnrollmentBonusPoints == other.EnrollmentBonusPoints;
        }

        /// <summary>
        /// Get hash code for this object.
        /// </summary>
        public override int GetHashCode()
        {
            return HashCode.Combine(PointsPerDollar, MinimumPointsForRedemption, RoundingRule, EnrollmentBonusPoints);
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