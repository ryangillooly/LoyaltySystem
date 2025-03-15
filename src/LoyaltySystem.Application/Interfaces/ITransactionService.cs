using LoyaltySystem.Application.Common;
using System.Collections.Generic;
using System.Threading.Tasks;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Domain.Common;

namespace LoyaltySystem.Application.Interfaces
{
    public interface ITransactionService
    {
        /// <summary>
        /// Records a transaction initiated by a customer from the Customer API
        /// </summary>
        /// <param name="transactionDto">The transaction details</param>
        /// <returns>A result containing the created transaction or errors</returns>
        Task<OperationResult<TransactionDto>> RecordCustomerTransaction(RecordTransactionDto transactionDto);
        
        /// <summary>
        /// Records a transaction initiated by a staff member from the Staff API
        /// </summary>
        /// <param name="transactionDto">The transaction details</param>
        /// <returns>A result containing the created transaction or errors</returns>
        Task<OperationResult<TransactionDto>> RecordStaffTransaction(RecordTransactionDto transactionDto);
        
        /// <summary>
        /// Gets a transaction by its ID
        /// </summary>
        /// <param name="transactionId">The ID of the transaction to retrieve</param>
        /// <returns>A result containing the transaction or errors</returns>
        Task<OperationResult<TransactionDto>> GetTransactionById(string transactionId);
        
        /// <summary>
        /// Gets all transactions for a loyalty card
        /// </summary>
        /// <param name="loyaltyCardId">The ID of the loyalty card</param>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <returns>A result containing the transactions or errors</returns>
        Task<OperationResult<PagedResult<TransactionDto>>> GetTransactionsByLoyaltyCard(
            string loyaltyCardId, int page = 1, int pageSize = 20);
        
        /// <summary>
        /// Gets all transactions for a customer
        /// </summary>
        /// <param name="customerId">The ID of the customer</param>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <returns>A result containing the transactions or errors</returns>
        Task<OperationResult<PagedResult<TransactionDto>>> GetTransactionsByCustomer(
            string customerId, int page = 1, int pageSize = 20);
    }
} 