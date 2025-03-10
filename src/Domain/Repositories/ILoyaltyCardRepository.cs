using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Enums;

namespace LoyaltySystem.Domain.Repositories
{
    /// <summary>
    /// Repository interface for LoyaltyCard entities.
    /// </summary>
    public interface ILoyaltyCardRepository
    {
        /// <summary>
        /// Gets a loyalty card by its ID.
        /// </summary>
        Task<LoyaltyCard> GetByIdAsync(LoyaltyCardId id);
        
        /// <summary>
        /// Gets loyalty cards for a specific customer.
        /// </summary>
        Task<IEnumerable<LoyaltyCard>> GetByCustomerIdAsync(CustomerId customerId);
        
        /// <summary>
        /// Gets loyalty cards for a specific program.
        /// </summary>
        Task<IEnumerable<LoyaltyCard>> GetByProgramIdAsync(LoyaltyProgramId programId);
        
        /// <summary>
        /// Gets a loyalty card by its QR code.
        /// </summary>
        Task<LoyaltyCard> GetByQrCodeAsync(string qrCode);
        
        /// <summary>
        /// Adds a new loyalty card.
        /// </summary>
        Task AddAsync(LoyaltyCard card);
        
        /// <summary>
        /// Updates an existing loyalty card.
        /// </summary>
        Task UpdateAsync(LoyaltyCard card);
        
        /// <summary>
        /// Finds cards that are near expiration.
        /// </summary>
        Task<IEnumerable<LoyaltyCard>> FindCardsNearExpirationAsync(int daysUntilExpiration);
        
        /// <summary>
        /// Get count of active cards for a program.
        /// </summary>
        Task<int> GetActiveCardCountForProgramAsync(LoyaltyProgramId programId);
        
        /// <summary>
        /// Find cards by status with pagination.
        /// </summary>
        Task<IEnumerable<LoyaltyCard>> FindByStatusAsync(CardStatus status, int skip, int take);
    }
} 