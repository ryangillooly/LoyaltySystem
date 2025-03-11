using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;

namespace LoyaltySystem.Application.Interfaces
{
    public interface ILoyaltyCardService
    {
        Task<OperationResult<LoyaltyCardDto>> GetByIdAsync(LoyaltyCardId id);
        Task<OperationResult<LoyaltyCardDto>> GetByQrCodeAsync(string qrCode);
        Task<OperationResult<IEnumerable<LoyaltyCardDto>>> GetByCustomerIdAsync(CustomerId customerId);
        Task<OperationResult<IEnumerable<LoyaltyCardDto>>> GetByProgramIdAsync(LoyaltyProgramId programId);
        Task<OperationResult<LoyaltyCardDto>> CreateCardAsync(CustomerId customerId, LoyaltyProgramId programId);
        Task<OperationResult<LoyaltyCardDto>> UpdateCardStatusAsync(LoyaltyCardId id, CardStatus status);
        Task<OperationResult<TransactionDto>> IssueStampsAsync(LoyaltyCardId cardId, int stampCount, StoreId storeId, decimal purchaseAmount, string transactionReference);
        Task<OperationResult<TransactionDto>> AddPointsAsync(LoyaltyCardId cardId, decimal points, decimal transactionAmount, StoreId storeId, Guid? staffId, string posTransactionId);
        Task<OperationResult<TransactionDto>> RedeemRewardAsync(LoyaltyCardId cardId, RewardId rewardId, StoreId storeId, Guid? staffId, RedeemRequestData redemptionData);
        Task<OperationResult<string>> GenerateQrCodeAsync(LoyaltyCardId cardId);
        Task<OperationResult<string>> GetOrGenerateQrCodeAsync(LoyaltyCardId cardId);
        Task<OperationResult<IEnumerable<TransactionDto>>> GetCardTransactionsAsync(LoyaltyCardId cardId);
        Task<OperationResult<Dictionary<CardStatus, int>>> GetCardCountByStatusAsync();
        Task<OperationResult<ProgramAnalyticsDto>> GetProgramCardAnalyticsAsync(LoyaltyProgramId programId);
        Task<int> GetActiveCardCountForProgramAsync(string programId);
        Task<int> GetCardCountForProgramAsync(string programId);
        Task<decimal> GetAverageTransactionsPerCardForProgramAsync(string programId);
    }
} 