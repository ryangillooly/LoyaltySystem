using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.ValueObjects;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;

namespace LoyaltySystem.Domain.Specifications;

/// <summary>
/// Specifications for LoyaltyCard entity queries
/// Demonstrates the specification pattern for complex business queries
/// </summary>
public static class LoyaltyCardSpecifications
{
    /// <summary>
    /// Specification for active loyalty cards
    /// </summary>
    public class ActiveCardsSpecification : BaseSpecification<LoyaltyCard>
    {
        public ActiveCardsSpecification() : base(card => card.Status == CardStatus.Active)
        {
            ApplyOrderBy(card => card.CreatedAt);
        }
    }

    /// <summary>
    /// Specification for loyalty cards by customer ID
    /// </summary>
    public class CardsByCustomerSpecification : BaseSpecification<LoyaltyCard>
    {
        public CardsByCustomerSpecification(CustomerId customerId) 
            : base(card => card.CustomerId == customerId)
        {
            ApplyOrderByDescending(card => card.CreatedAt);
        }
    }

    /// <summary>
    /// Specification for loyalty cards by program ID
    /// </summary>
    public class CardsByProgramSpecification : BaseSpecification<LoyaltyCard>
    {
        public CardsByProgramSpecification(LoyaltyProgramId programId) 
            : base(card => card.ProgramId == programId)
        {
            ApplyOrderBy(card => card.CreatedAt);
        }
    }

    /// <summary>
    /// Specification for loyalty cards with minimum points balance
    /// </summary>
    public class CardsWithMinimumPointsSpecification : BaseSpecification<LoyaltyCard>
    {
        public CardsWithMinimumPointsSpecification(int minimumPoints) 
            : base(card => card.PointsBalance >= minimumPoints)
        {
            ApplyOrderByDescending(card => card.PointsBalance);
        }
    }

    /// <summary>
    /// Specification for loyalty cards with minimum stamps
    /// </summary>
    public class CardsWithMinimumStampsSpecification : BaseSpecification<LoyaltyCard>
    {
        public CardsWithMinimumStampsSpecification(int minimumStamps) 
            : base(card => card.StampsCollected >= minimumStamps)
        {
            ApplyOrderByDescending(card => card.StampsCollected);
        }
    }

    /// <summary>
    /// Specification for loyalty cards that expire within a date range
    /// </summary>
    public class CardsExpiringInRangeSpecification : BaseSpecification<LoyaltyCard>
    {
        public CardsExpiringInRangeSpecification(DateTime startDate, DateTime endDate) 
            : base(card => card.ExpiresAt.HasValue && 
                          card.ExpiresAt.Value >= startDate && 
                          card.ExpiresAt.Value <= endDate)
        {
            ApplyOrderBy(card => card.ExpiresAt!);
        }
    }

    /// <summary>
    /// Specification for loyalty cards created within a date range
    /// </summary>
    public class CardsCreatedInRangeSpecification : BaseSpecification<LoyaltyCard>
    {
        public CardsCreatedInRangeSpecification(DateTime startDate, DateTime endDate) 
            : base(card => card.CreatedAt >= startDate && card.CreatedAt <= endDate)
        {
            ApplyOrderByDescending(card => card.CreatedAt);
        }
    }

    /// <summary>
    /// Specification for loyalty cards with recent activity
    /// Note: LoyaltyCard doesn't have LastUsedAt property, so we'll use UpdatedAt as proxy
    /// </summary>
    public class CardsWithRecentActivitySpecification : BaseSpecification<LoyaltyCard>
    {
        public CardsWithRecentActivitySpecification(DateTime sinceDate) 
            : base(card => card.UpdatedAt >= sinceDate)
        {
            ApplyOrderByDescending(card => card.UpdatedAt);
        }
    }

    /// <summary>
    /// Specification for inactive loyalty cards (no recent activity)
    /// Note: LoyaltyCard doesn't have LastUsedAt property, so we'll use UpdatedAt as proxy
    /// </summary>
    public class InactiveCardsSpecification : BaseSpecification<LoyaltyCard>
    {
        public InactiveCardsSpecification(DateTime beforeDate) 
            : base(card => card.UpdatedAt < beforeDate)
        {
            ApplyOrderBy(card => card.UpdatedAt);
        }
    }

    /// <summary>
    /// Complex specification combining multiple criteria
    /// Example: Active cards with points above threshold for a specific program
    /// </summary>
    public class ActiveCardsWithPointsForProgramSpecification : BaseSpecification<LoyaltyCard>
    {
        public ActiveCardsWithPointsForProgramSpecification(
            LoyaltyProgramId programId, 
            int minimumPoints) 
            : base(card => card.Status == CardStatus.Active && 
                          card.ProgramId == programId && 
                          card.PointsBalance >= minimumPoints)
        {
            ApplyOrderByDescending(card => card.PointsBalance);
        }
    }

    /// <summary>
    /// Specification for loyalty cards eligible for tier upgrade
    /// </summary>
    public class CardsEligibleForTierUpgradeSpecification : BaseSpecification<LoyaltyCard>
    {
        public CardsEligibleForTierUpgradeSpecification(int pointsThreshold, int stampsThreshold) 
            : base(card => card.Status == CardStatus.Active && 
                          (card.PointsBalance >= pointsThreshold || 
                           card.StampsCollected >= stampsThreshold))
        {
            ApplyOrderByDescending(card => card.PointsBalance + card.StampsCollected);
        }
    }
} 