using LoyaltySystem.Application.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.Services;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;
using ProgramAnalyticsDto = LoyaltySystem.Application.DTOs.ProgramAnalyticsDto;

namespace LoyaltySystem.Application.Interfaces
{
    public interface ILoyaltyCardService
    {
        Task<OperationResult<LoyaltyCardDto>> GetByIdAsync(LoyaltyCardId id);
        Task<OperationResult<PagedResult<LoyaltyCardDto>>> GetAllAsync(int skip, int limit);
        Task<OperationResult<LoyaltyCardDto>> GetByQrCodeAsync(string qrCode);
        Task<OperationResult<IEnumerable<LoyaltyCardDto>>> GetByCustomerIdAsync(CustomerId customerId);
        Task<OperationResult<IEnumerable<LoyaltyCardDto>>> GetByProgramIdAsync(LoyaltyProgramId programId);
        /// <summary>
/// Creates a new loyalty card based on the provided data.
/// </summary>
/// <param name="dto">The data required to create the loyalty card.</param>
/// <returns>The result of the creation operation, including the created loyalty card details if successful.</returns>
Task<OperationResult<LoyaltyCardDto>> CreateCardAsync(CreateLoyaltyCardDto dto);
        /// <summary>
/// Updates the status of a loyalty card identified by its ID.
/// </summary>
/// <param name="id">The unique identifier of the loyalty card.</param>
/// <param name="status">The new status to assign to the loyalty card.</param>
/// <returns>An operation result containing the updated loyalty card data.</returns>
Task<OperationResult<LoyaltyCardDto>> UpdateCardStatusAsync(LoyaltyCardId id, CardStatus status);
        /// <summary>
/// Issues a specified number of stamps to a loyalty card for a given purchase and store.
/// </summary>
/// <param name="cardId">The identifier of the loyalty card to update.</param>
/// <param name="stampCount">The number of stamps to issue.</param>
/// <param name="storeId">The identifier of the store where the transaction occurred.</param>
/// <param name="purchaseAmount">The amount spent in the transaction.</param>
/// <param name="transactionReference">A reference identifier for the transaction.</param>
/// <returns>The result of the stamp issuance operation, including transaction details if successful.</returns>
Task<OperationResult<TransactionDto>> IssueStampsAsync(LoyaltyCardId cardId, int stampCount, StoreId storeId, decimal purchaseAmount, string transactionReference);
        /// <summary>
/// Adds points to a loyalty card based on a transaction, recording the associated store, staff member, and POS transaction ID.
/// </summary>
/// <param name="cardId">The identifier of the loyalty card to credit.</param>
/// <param name="points">The number of points to add.</param>
/// <param name="transactionAmount">The monetary value of the transaction.</param>
/// <param name="storeId">The store where the transaction occurred.</param>
/// <param name="staffId">The staff member who processed the transaction, if applicable.</param>
/// <param name="posTransactionId">The point-of-sale transaction identifier.</param>
/// <returns>The result of the points addition, including transaction details if successful.</returns>
Task<OperationResult<TransactionDto>> AddPointsAsync(LoyaltyCardId cardId, decimal points, decimal transactionAmount, StoreId storeId, StaffId? staffId, string posTransactionId);
        /// <summary>
/// Redeems a specified reward from a loyalty card and records the transaction.
/// </summary>
/// <param name="cardId">The identifier of the loyalty card to redeem from.</param>
/// <param name="rewardId">The identifier of the reward to be redeemed.</param>
/// <param name="storeId">The identifier of the store where the redemption occurs.</param>
/// <param name="staffId">The identifier of the staff member processing the redemption, if applicable.</param>
/// <param name="redemptionData">Additional data required for the redemption process.</param>
/// <returns>The result of the redemption transaction, including transaction details if successful.</returns>
Task<OperationResult<TransactionDto>> RedeemRewardAsync(LoyaltyCardId cardId, RewardId rewardId, StoreId storeId, StaffId? staffId, RedeemRequestData redemptionData);
        /// <summary>
/// Generates a QR code for the specified loyalty card.
/// </summary>
/// <param name="cardId">The unique identifier of the loyalty card.</param>
/// <returns>An operation result containing the generated QR code as a string.</returns>
Task<OperationResult<string>> GenerateQrCodeAsync(LoyaltyCardId cardId);
        /// <summary>
/// Retrieves the QR code associated with a loyalty card, generating one if it does not already exist.
/// </summary>
/// <param name="cardId">The unique identifier of the loyalty card.</param>
/// <returns>An operation result containing the QR code string.</returns>
Task<OperationResult<string>> GetOrGenerateQrCodeAsync(LoyaltyCardId cardId);
        /// <summary>
/// Retrieves the transaction history for a specified loyalty card.
/// </summary>
/// <param name="cardId">The unique identifier of the loyalty card.</param>
/// <returns>A task containing the operation result with a collection of transaction details for the card.</returns>
Task<OperationResult<IEnumerable<TransactionDto>>> GetCardTransactionsAsync(LoyaltyCardId cardId);
        Task<OperationResult<int>> GetCardCountByStatusAsync(CardStatus status);
        Task<OperationResult<ProgramAnalyticsDto>> GetProgramCardAnalyticsAsync(LoyaltyProgramId programId);
        Task<int> GetActiveCardCountForProgramAsync(string programId);
        Task<int> GetCardCountForProgramAsync(string programId);
        Task<decimal> GetAverageTransactionsPerCardForProgramAsync(string programId);
        
        /// <summary>
        /// Verifies that a loyalty card belongs to a specific customer
        /// </summary>
        /// <param name="cardId">The ID of the loyalty card to check</param>
        /// <param name="customerId">The ID of the customer</param>
        /// <returns>Success result if the card belongs to the customer, failure otherwise</returns>
        Task<OperationResult<bool>> VerifyCardOwnership(string cardId, CustomerId customerId);
    }
} 