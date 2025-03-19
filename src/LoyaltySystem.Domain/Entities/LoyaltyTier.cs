using System;
using System.Collections.Generic;
using LoyaltySystem.Domain.Common;

namespace LoyaltySystem.Domain.Entities
{
    /// <summary>
    /// Represents a tier level in a loyalty program with specific benefits.
    /// </summary>
    public class LoyaltyTier
    {
        private readonly List<string> _benefits;

        /// <summary>
        /// The unique identifier for this tier.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// The loyalty program this tier belongs to.
        /// </summary>
        public LoyaltyProgramId ProgramId { get; private set; }

        /// <summary>
        /// The name of the tier (e.g., "Bronze", "Silver", "Gold").
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The point threshold required to reach this tier.
        /// </summary>
        public int PointThreshold { get; private set; }

        /// <summary>
        /// The point multiplier for this tier (e.g., 1.1 means 10% bonus points).
        /// </summary>
        public decimal PointMultiplier { get; private set; }

        /// <summary>
        /// The order in which this tier appears (0 = first/lowest tier).
        /// </summary>
        public int TierOrder { get; private set; }

        /// <summary>
        /// The list of benefits available to members of this tier.
        /// </summary>
        public IReadOnlyCollection<string> Benefits => _benefits.AsReadOnly();

        /// <summary>
        /// When this tier was created.
        /// </summary>
        public DateTime CreatedAt { get; private set; }
        
        /// <summary>
        /// When this tier was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; private set; }

        // Private constructor for EF Core
        private LoyaltyTier()
        {
            _benefits = new List<string>();
        }

        public LoyaltyTier(
            LoyaltyProgramId programId,
            string name,
            int pointThreshold,
            decimal pointMultiplier = 1.0m,
            int tierOrder = 0,
            IEnumerable<string> benefits = null)
        {
            if (programId == null)
                throw new ArgumentNullException(nameof(programId));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Tier name cannot be empty", nameof(name));

            if (pointThreshold < 0)
                throw new ArgumentException("Point threshold cannot be negative", nameof(pointThreshold));

            if (pointMultiplier <= 0)
                throw new ArgumentException("Point multiplier must be greater than zero", nameof(pointMultiplier));

            Id = Guid.NewGuid();
            ProgramId = programId;
            Name = name;
            PointThreshold = pointThreshold;
            PointMultiplier = pointMultiplier;
            TierOrder = tierOrder;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            _benefits = new List<string>();

            if (benefits != null)
            {
                foreach (var benefit in benefits)
                {
                    AddBenefit(benefit);
                }
            }
        }

        /// <summary>
        /// Add a benefit to this tier.
        /// </summary>
        public void AddBenefit(string benefit)
        {
            if (string.IsNullOrWhiteSpace(benefit))
                throw new ArgumentException("Benefit description cannot be empty", nameof(benefit));

            if (!_benefits.Contains(benefit))
            {
                _benefits.Add(benefit);
                UpdatedAt = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Remove a benefit from this tier.
        /// </summary>
        public void RemoveBenefit(string benefit)
        {
            if (_benefits.Remove(benefit))
            {
                UpdatedAt = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Update tier properties.
        /// </summary>
        public void Update(
            string name,
            int pointThreshold,
            decimal pointMultiplier,
            int tierOrder)
        {
            if (!string.IsNullOrWhiteSpace(name))
                Name = name;

            if (pointThreshold >= 0)
                PointThreshold = pointThreshold;

            if (pointMultiplier > 0)
                PointMultiplier = pointMultiplier;

            TierOrder = tierOrder;
            UpdatedAt = DateTime.UtcNow;
        }
    }
} 