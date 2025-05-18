using System;
using System.Collections.Generic;
using LoyaltySystem.Domain.Common;

namespace LoyaltySystem.Domain.Entities
{
    /// <summary>
    /// Represents a tier level in a loyalty program with specific benefits.
    /// </summary>
    public class LoyaltyTier : Entity<LoyaltyTierId>
    {
        private readonly List<string> _benefits;
        
        public LoyaltyProgramId ProgramId { get; private set; }
        public string Name { get; private set; }
        public int PointThreshold { get; private set; }
        public decimal PointMultiplier { get; private set; }
        public int TierOrder { get; private set; }
        public IReadOnlyCollection<string> Benefits => _benefits.AsReadOnly();

        /// <summary>
        /// Initializes a new loyalty tier with the specified program, name, point threshold, multiplier, order, and optional benefits.
        /// </summary>
        /// <param name="programId">The identifier of the loyalty program this tier belongs to.</param>
        /// <param name="name">The name of the tier.</param>
        /// <param name="pointThreshold">The minimum points required to reach this tier. Must be non-negative.</param>
        /// <param name="pointMultiplier">The multiplier applied to points earned in this tier. Must be greater than zero.</param>
        /// <param name="tierOrder">The order of the tier within the program.</param>
        /// <param name="benefits">An optional collection of benefits associated with the tier.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="programId"/> or <paramref name="name"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="pointThreshold"/> is negative or <paramref name="pointMultiplier"/> is not greater than zero.</exception>
        public LoyaltyTier
        (
            LoyaltyProgramId programId,
            string name,
            int pointThreshold,
            decimal pointMultiplier = 1.0m,
            int tierOrder = 0,
            IEnumerable<string> benefits = null
        )
        : base(new LoyaltyTierId())
        {
            if (pointThreshold < 0)
                throw new ArgumentException("Point threshold cannot be negative", nameof(pointThreshold));

            if (pointMultiplier <= 0)
                throw new ArgumentException("Point multiplier must be greater than zero", nameof(pointMultiplier));
            
            Name = name ?? throw new ArgumentNullException(nameof(name));
            ProgramId = programId ?? throw new ArgumentNullException(nameof(programId));
            PointThreshold = pointThreshold;
            PointMultiplier = pointMultiplier;
            TierOrder = tierOrder;
            _benefits = new List<string>();

            if (benefits is { })
                foreach (var benefit in benefits)
                    AddBenefit(benefit);
        }
        
        /// <summary>
        /// Adds a unique benefit to the loyalty tier if it does not already exist.
        /// </summary>
        /// <param name="benefit">The benefit to add.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="benefit"/> is null.</exception>
        public void AddBenefit(string benefit)
        {
            ArgumentNullException.ThrowIfNull(benefit, nameof(benefit));
            
            if (!_benefits.Contains(benefit))
            {
                _benefits.Add(benefit);
                UpdatedAt = DateTime.UtcNow;
            }
        }
        
        /// <summary>
        /// Removes a benefit from the tier if it exists.
        /// </summary>
        /// <param name="benefit">The benefit to remove.</param>
        public void RemoveBenefit(string benefit)
        {
            ArgumentNullException.ThrowIfNull(benefit, nameof(benefit));
            
            if (_benefits.Remove(benefit))
                UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the tier's name, point threshold, point multiplier, and order if the provided values are valid.
        /// </summary>
        /// <param name="name">The new name for the tier. Ignored if null, empty, or whitespace.</param>
        /// <param name="pointThreshold">The new minimum points required for the tier. Must be non-negative to update.</param>
        /// <param name="pointMultiplier">The new multiplier for points earned. Must be greater than zero to update.</param>
        /// <param name="tierOrder">The new order of the tier.</param>
        public void Update
        (
            string name,
            int pointThreshold,
            decimal pointMultiplier,
            int tierOrder
        )
        {
            ArgumentNullException.ThrowIfNull(name, nameof(name));
            ArgumentNullException.ThrowIfNull(tierOrder, nameof(tierOrder));
            ArgumentNullException.ThrowIfNull(pointThreshold, nameof(pointThreshold));
            ArgumentNullException.ThrowIfNull(pointMultiplier, nameof(pointMultiplier));
            
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