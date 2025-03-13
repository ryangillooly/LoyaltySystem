using System;
using LoyaltySystem.Domain.ValueObjects;

namespace LoyaltySystem.Application.DTOs.LoyaltyPrograms
{
    /// <summary>
    /// DTO representing the configuration for a points-based loyalty program.
    /// </summary>
    public class PointsConfigDto
    {
        /// <summary>
        /// Number of points awarded per pound spent.
        /// </summary>
        public decimal PointsPerPound { get; set; } = 1;

        /// <summary>
        /// Minimum number of points required for redemption.
        /// </summary>
        public int MinimumPointsForRedemption { get; set; } = 100;

        /// <summary>
        /// How points should be rounded when calculated.
        /// </summary>
        public PointsRoundingRule RoundingRule { get; set; } = PointsRoundingRule.RoundDown;

        /// <summary>
        /// Enrollment bonus points awarded when a customer joins the program.
        /// </summary>
        public int EnrollmentBonusPoints { get; set; } = 0;

        public PointsConfigDto()
        {
            // Default constructor for serialization
        }

        public PointsConfigDto(PointsConfig pointsConfig)
        {
            if (pointsConfig == null)
                return;
                
            PointsPerPound = pointsConfig.PointsPerDollar;
            MinimumPointsForRedemption = pointsConfig.MinimumPointsForRedemption;
            RoundingRule = pointsConfig.RoundingRule;
            EnrollmentBonusPoints = pointsConfig.EnrollmentBonusPoints;
        }

        public PointsConfig ToPointsConfig()
        {
            return new PointsConfig(
                PointsPerPound,
                MinimumPointsForRedemption,
                RoundingRule,
                EnrollmentBonusPoints);
        }
    }
} 