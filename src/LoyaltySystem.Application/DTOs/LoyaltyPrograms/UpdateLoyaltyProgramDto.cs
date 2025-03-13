using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Enums;
using LoyaltySystem.Domain.ValueObjects;

namespace LoyaltySystem.Application.DTOs.LoyaltyPrograms;

public class UpdateLoyaltyProgramDto
{
    [Required]
    public string Id { get; set; }
    
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; }
    
    [StringLength(500)]
    public string Description { get; set; }
    
    // For stamp-based programs
    public int? StampThreshold { get; set; }
    
    // For points-based programs
    public decimal? PointsConversionRate { get; set; }
    
    // New points configuration for more detailed control
    public PointsConfigDto PointsConfig { get; set; }
    
    // Tiers for points-based programs
    public List<TierDto> Tiers { get; set; } = new List<TierDto>();
    
    // Optional constraints
    public int? DailyStampLimit { get; set; }
    public decimal? MinimumTransactionAmount { get; set; }
    
    public ExpirationPolicyDto ExpirationPolicy { get; set; }
    public string TermsAndConditions { get; set; }
    public bool? HasTiers { get; set; }
    public int EnrollmentBonusPoints { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
}