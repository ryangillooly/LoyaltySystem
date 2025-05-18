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
        Task<OperationResult<LoyaltyCardDto>> CreateCardAsync(CreateLoyaltyCardDto dto);
        Task<OperationResult<LoyaltyCardDto>> UpdateCardStatusAsync(LoyaltyCardId id, CardStatus status);
        Task<OperationResult<TransactionDto>> IssueStampsAsync(LoyaltyCardId cardId, int stampCount, StoreId storeId, decimal purchaseAmount, string transactionReference);
        Task<OperationResult<TransactionDto>> AddPointsAsync(LoyaltyCardId cardId, decimal points, decimal transactionAmount, StoreId storeId, StaffId? staffId, string posTransactionId);
        Task<OperationResult<TransactionDto>> RedeemRewardAsync(LoyaltyCardId cardId, RewardId rewardId, StoreId storeId, StaffId? staffId, RedeemRequestData redemptionData);
        Task<OperationResult<string>> GenerateQrCodeAsync(LoyaltyCardId cardId);
        Task<OperationResult<string>> GetOrGenerateQrCodeAsync(LoyaltyCardId cardId);
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