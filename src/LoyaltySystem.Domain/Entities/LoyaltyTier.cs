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
        
        public void AddBenefit(string benefit)
        {
            ArgumentNullException.ThrowIfNull(benefit, nameof(benefit));
            
            if (!_benefits.Contains(benefit))
            {
                _benefits.Add(benefit);
                UpdatedAt = DateTime.UtcNow;
            }
        }
        
        public void RemoveBenefit(string benefit)
        {
            ArgumentNullException.ThrowIfNull(benefit, nameof(benefit));
            
            if (_benefits.Remove(benefit))
                UpdatedAt = DateTime.UtcNow;
        }

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