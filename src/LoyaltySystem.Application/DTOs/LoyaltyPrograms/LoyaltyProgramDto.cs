using System;
using System.Collections.Generic;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.ValueObjects;
using LoyaltySystem.Domain.Enums;

namespace LoyaltySystem.Application.DTOs.LoyaltyPrograms
{
    public class LoyaltyProgramDto
    {
        public Guid Id { get; set; }
        public Guid BrandId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public LoyaltyProgramType Type { get; set; }
        public ExpirationPolicyDto ExpirationPolicy { get; set; }
        
        // For stamp-based programs
        public int? StampThreshold { get; set; }
        
        // For points-based programs
        public decimal? PointsConversionRate { get; set; }
        
        // New points configuration for more detailed control
        public PointsConfigDto PointsConfig { get; set; }
        
        // Optional constraints
        public int? DailyStampLimit { get; set; }
        public decimal? MinimumTransactionAmount { get; set; }
        
        public bool HasTiers { get; set; }
        public List<TierDto> Tiers { get; set; } = new List<TierDto>();
        public string TermsAndConditions { get; set; }
        public int EnrollmentBonusPoints { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Navigation properties for API responses
        public BrandDto Brand { get; set; }
        public List<RewardDto> Rewards { get; set; } = new List<RewardDto>();
    }
} 