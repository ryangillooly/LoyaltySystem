using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Enums;
using LoyaltySystem.Domain.ValueObjects;

namespace LoyaltySystem.Application.DTOs.LoyaltyPrograms
{
    public class CreateLoyaltyProgramDto
    {
        [Required]
        public string BrandId { get; set; }
        
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public LoyaltyProgramType Type { get; set; }
        
        public ExpirationPolicyDto ExpirationPolicy { get; set; }
        
        // New points configuration for more detailed control
        public PointsConfigDto PointsConfig { get; set; }
        
        // Tiers for points-based programs
        public List<TierDto> Tiers { get; set; } = new ();

        public string TermsAndConditions { get; set; } = string.Empty;
        public bool HasTiers { get; set; }
        public int EnrollmentBonusPoints { get; set; }
        public int StampThreshold { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; } = true;
    }
} 