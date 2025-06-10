using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Enums;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.ValueObjects;
using System.Data;

namespace LoyaltySystem.Domain.Repositories
{
    /// <summary>
    /// Repository interface for the LoyaltyCard aggregate.
    /// Inherits common CRUD operations from generic repository and adds domain-specific methods.
    /// </summary>
    public interface ILoyaltyCardRepository : IRepository<LoyaltyCard, LoyaltyCardId>
    {
        /// <summary>
        /// Gets loyalty cards for a specific customer.
        /// </summary>
        Task<IEnumerable<LoyaltyCard>> GetByCustomerIdAsync(CustomerId customerId);
        
        /// <summary>
        /// Gets loyalty cards for a specific program.
        /// </summary>
        Task<IEnumerable<LoyaltyCard>> GetByProgramIdAsync(LoyaltyProgramId programId);
        
        /// <summary>
        /// Get loyalty card count by status
        /// </summary>
        Task<int> GetCardCountByStatusAsync(CardStatus status);
        
        /// <summary>
        /// Gets a loyalty card by its QR code.
        /// </summary>
        Task<LoyaltyCard?> GetByQrCodeAsync(string qrCode);
        
        /// <summary>
        /// Finds cards that are near expiration.
        /// </summary>
        Task<IEnumerable<LoyaltyCard>> FindCardsNearExpirationAsync(int daysUntilExpiration);
        
        /// <summary>
        /// Get count of active cards for a program.
        /// </summary>
        Task<int> GetActiveCardCountForProgramAsync(LoyaltyProgramId programId);
        
        /// <summary>
        /// Find cards by status.
        /// </summary>
        Task<IEnumerable<LoyaltyCard>> FindByStatusAsync(CardStatus status, int skip, int take);
        
        /// <summary>
        /// Gets a loyalty card by its ID including all transactions.
        /// </summary>
        Task<LoyaltyCard?> GetByIdWithTransactionsAsync(LoyaltyCardId id);

        /// <summary>
        /// Gets loyalty cards with points balance above a threshold.
        /// </summary>
        Task<IEnumerable<LoyaltyCard>> GetCardsWithMinimumPointsAsync(int minimumPoints, int skip = 0, int limit = 50);

        /// <summary>
        /// Gets loyalty cards with stamps above a threshold.
        /// </summary>
        Task<IEnumerable<LoyaltyCard>> GetCardsWithMinimumStampsAsync(int minimumStamps, int skip = 0, int limit = 50);

        /// <summary>
        /// Gets loyalty cards that haven't been used since a specific date.
        /// </summary>
        Task<IEnumerable<LoyaltyCard>> GetInactiveCardsSinceAsync(DateTime sinceDate, int skip = 0, int limit = 50);

        /// <summary>
        /// Gets loyalty cards created within a date range.
        /// </summary>
        Task<IEnumerable<LoyaltyCard>> GetCardsCreatedInRangeAsync(DateTime startDate, DateTime endDate, int skip = 0, int limit = 50);

        /// <summary>
        /// Gets loyalty cards eligible for tier upgrade based on points or stamps thresholds.
        /// </summary>
        Task<IEnumerable<LoyaltyCard>> GetCardsEligibleForTierUpgradeAsync(int pointsThreshold, int stampsThreshold, int skip = 0, int limit = 50);
    }
} 