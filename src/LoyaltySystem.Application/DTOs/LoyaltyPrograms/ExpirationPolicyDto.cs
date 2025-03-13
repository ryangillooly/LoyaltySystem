using LoyaltySystem.Domain.Entities;
using System;
using LoyaltySystem.Domain.Enums;

namespace LoyaltySystem.Application.DTOs.LoyaltyPrograms;

/// <summary>
/// DTO representing the expiration policy for a loyalty program.
/// </summary>
public class ExpirationPolicyDto
{
    /// <summary>
    /// Whether the points or stamps expire.
    /// </summary>
    public bool HasExpiration { get; set; }
    
    /// <summary>
    /// Whether expiration occurs on a specific date.
    /// </summary>
    public bool ExpiresOnSpecificDate { get; set; }
    
    /// <summary>
    /// Type of expiration period (days, months, years).
    /// </summary>
    public ExpirationType ExpirationType { get; set; }
    
    /// <summary>
    /// Value for the expiration period.
    /// </summary>
    public int ExpirationValue { get; set; }
    
    /// <summary>
    /// Specific day of month for expiration (if applicable).
    /// </summary>
    public int? ExpirationDay { get; set; }
    
    /// <summary>
    /// Specific month for expiration (if applicable).
    /// </summary>
    public int? ExpirationMonth { get; set; }
    
    public ExpirationPolicyDto()
    {
        // Default constructor for serialization
    }
    
    public ExpirationPolicyDto(ExpirationPolicy policy)
    {
        if (policy == null)
            return;
            
        HasExpiration = policy.HasExpiration;
        ExpiresOnSpecificDate = policy.ExpiresOnSpecificDate;
        ExpirationType = policy.ExpirationType;
        ExpirationValue = policy.ExpirationValue;
        ExpirationDay = policy.ExpirationDay;
        ExpirationMonth = policy.ExpirationMonth;
    }
    
    public ExpirationPolicy ToExpirationPolicy()
    {
        if (!HasExpiration)
            return new ExpirationPolicy();
            
        if (ExpiresOnSpecificDate && ExpirationDay.HasValue)
        {
            return new ExpirationPolicy(ExpirationDay.Value, ExpirationMonth);
        }
        else
        {
            return new ExpirationPolicy(ExpirationType, ExpirationValue);
        }
    }
} 