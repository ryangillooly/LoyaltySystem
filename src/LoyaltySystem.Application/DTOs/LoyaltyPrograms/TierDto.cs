using System;
using System.Collections.Generic;
using LoyaltySystem.Domain.Entities;

namespace LoyaltySystem.Application.DTOs.LoyaltyPrograms
{
    public class TierDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int PointThreshold { get; set; }
        public decimal PointMultiplier { get; set; }
        public int TierOrder { get; set; }
        public List<string> Benefits { get; set; } = new ();
        
        public TierDto()
        {
            // For serialization
            // Generate a new ID by default to satisfy validation
            Id = Guid.NewGuid().ToString();
        }

        public TierDto(string id, string name, int pointThreshold, decimal pointMultiplier, int tierOrder)
        {
            Id = id ?? Guid.NewGuid().ToString();
            Name = name;
            PointThreshold = pointThreshold;
            PointMultiplier = pointMultiplier;
            TierOrder = tierOrder;
            Benefits = new List<string>();
        }
        
        public TierDto(LoyaltyTier tier)
        {
            if (tier == null) return;
            
            Id = tier.Id.ToString();
            Name = tier.Name;
            PointThreshold = tier.PointThreshold;
            PointMultiplier = tier.PointMultiplier;
            TierOrder = tier.TierOrder;
            Benefits = tier.Benefits?.ToList() ?? new List<string>();
        }
    }
} 