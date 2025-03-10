using System;
using System.Collections.Generic;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;

namespace LoyaltySystem.Application.DTOs
{
    /// <summary>
    /// Data transfer object for loyalty cards.
    /// </summary>
    public class LoyaltyCardDto
    {
        /// <summary>
        /// The unique identifier of the loyalty card.
        /// </summary>
        public LoyaltyCardId Id { get; set; }
        
        /// <summary>
        /// The program this card belongs to.
        /// </summary>
        public LoyaltyProgramId ProgramId { get; set; }
        
        /// <summary>
        /// The name of the program.
        /// </summary>
        public string ProgramName { get; set; }
        
        /// <summary>
        /// The customer who owns this card.
        /// </summary>
        public CustomerId CustomerId { get; set; }
        
        /// <summary>
        /// The type of loyalty program (Stamp or Points).
        /// </summary>
        public string Type { get; set; }
        
        /// <summary>
        /// The number of stamps collected (for stamp-based programs).
        /// </summary>
        public int StampsCollected { get; set; }
        
        /// <summary>
        /// The current points balance (for points-based programs).
        /// </summary>
        public decimal PointsBalance { get; set; }
        
        /// <summary>
        /// The status of the card (Active, Expired, or Suspended).
        /// </summary>
        public string Status { get; set; }
        
        /// <summary>
        /// The QR code for this card.
        /// </summary>
        public string QrCode { get; set; }
        
        /// <summary>
        /// When the card was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// When the card expires, if applicable.
        /// </summary>
        public DateTime? ExpiresAt { get; set; }
        
        /// <summary>
        /// For stamp-based programs, the number of stamps needed for a reward.
        /// </summary>
        public int? StampThreshold { get; set; }
        
        /// <summary>
        /// The transaction history for this card.
        /// </summary>
        public List<TransactionDto> Transactions { get; set; } = new();
    }
    
    /// <summary>
    /// Data transfer object for transactions.
    /// </summary>
    public class TransactionDto
    {
        /// <summary>
        /// The unique identifier of the transaction.
        /// </summary>
        public TransactionId Id { get; set; }
        
        /// <summary>
        /// The card the transaction is for.
        /// </summary>
        public LoyaltyCardId CardId { get; set; }
        
        /// <summary>
        /// The type of transaction.
        /// </summary>
        public string Type { get; set; }
        
        /// <summary>
        /// The reward redeemed, if applicable.
        /// </summary>
        public RewardId RewardId { get; set; }
        
        /// <summary>
        /// The quantity of stamps issued or redeemed.
        /// </summary>
        public int? Quantity { get; set; }
        
        /// <summary>
        /// The amount of points issued or redeemed.
        /// </summary>
        public decimal? PointsAmount { get; set; }
        
        /// <summary>
        /// The purchase amount that triggered this transaction.
        /// </summary>
        public decimal? TransactionAmount { get; set; }
        
        /// <summary>
        /// The store where the transaction occurred.
        /// </summary>
        public StoreId StoreId { get; set; }
        
        /// <summary>
        /// The staff member who processed the transaction.
        /// </summary>
        public Guid? StaffId { get; set; }
        
        /// <summary>
        /// The ID of the POS transaction, if applicable.
        /// </summary>
        public string PosTransactionId { get; set; }
        
        /// <summary>
        /// When the transaction occurred.
        /// </summary>
        public DateTime Timestamp { get; set; }
    }
    
    /// <summary>
    /// Response class for API operations.
    /// </summary>
    public class Result<T>
    {
        /// <summary>
        /// Whether the operation was successful.
        /// </summary>
        public bool Success { get; private set; }
        
        /// <summary>
        /// The result data, if successful.
        /// </summary>
        public T Data { get; private set; }
        
        /// <summary>
        /// Error messages, if not successful.
        /// </summary>
        public string Errors { get; private set; }
        
        /// <summary>
        /// Creates a successful result with data.
        /// </summary>
        public static Result<T> Success(T data) => new Result<T> { Success = true, Data = data };
        
        /// <summary>
        /// Creates a failure result with error messages.
        /// </summary>
        public static Result<T> Failure(string errors) => new Result<T> { Success = false, Errors = errors };
    }
} 