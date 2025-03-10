using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.Events;
using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Enums;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Repositories;

namespace LoyaltySystem.Application.Services
{
    /// <summary>
    /// Service for managing loyalty card operations.
    /// </summary>
    public class LoyaltyCardService
    {
        private readonly ILoyaltyCardRepository _loyaltyCardRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEventPublisher _eventPublisher;

        public LoyaltyCardService(
            ILoyaltyCardRepository loyaltyCardRepository,
            IUnitOfWork unitOfWork,
            IEventPublisher eventPublisher)
        {
            _loyaltyCardRepository = loyaltyCardRepository ?? throw new ArgumentNullException(nameof(loyaltyCardRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        }

        /// <summary>
        /// Creates a new loyalty card for a customer.
        /// </summary>
        public async Task<Result<LoyaltyCardDto>> CreateCardAsync(CustomerId customerId, LoyaltyProgramId programId)
        {
            try
            {
                // Get the program to verify it exists and to get the program type
                var program = await GetProgramAsync(programId);
                
                if (program == null)
                    return Result<LoyaltyCardDto>.Failure("Loyalty program not found");
                
                if (!program.IsActive)
                    return Result<LoyaltyCardDto>.Failure("Cannot create a card for an inactive program");

                // Calculate expiration date if applicable
                DateTime? expiresAt = null;
                if (program.ExpirationPolicy.HasExpiration)
                {
                    expiresAt = program.ExpirationPolicy.CalculateExpirationDate(DateTime.UtcNow);
                }

                // Create the card
                var card = new LoyaltyCard(programId, customerId, program.Type, expiresAt);
                await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    await _loyaltyCardRepository.AddAsync(card);
                    
                    // Publish event
                    await _eventPublisher.PublishAsync(new CardCreatedEvent
                    {
                        CardId = card.Id.ToString(),
                        CustomerId = card.CustomerId.ToString(),
                        ProgramId = card.ProgramId.ToString(),
                        Type = card.Type.ToString(),
                        Timestamp = DateTime.UtcNow
                    });
                });

                // Return the result
                var cardDto = new LoyaltyCardDto
                {
                    Id = card.Id,
                    ProgramId = card.ProgramId,
                    ProgramName = program.Name,
                    CustomerId = card.CustomerId,
                    Type = card.Type.ToString(),
                    StampsCollected = card.StampsCollected,
                    PointsBalance = card.PointsBalance,
                    Status = card.Status.ToString(),
                    QrCode = card.QrCode,
                    CreatedAt = card.CreatedAt,
                    ExpiresAt = card.ExpiresAt,
                    StampThreshold = program.StampThreshold
                };

                return Result<LoyaltyCardDto>.Success(cardDto);
            }
            catch (Exception ex)
            {
                return Result<LoyaltyCardDto>.Failure($"Error creating loyalty card: {ex.Message}");
            }
        }

        /// <summary>
        /// Issues stamps to a loyalty card.
        /// </summary>
        public async Task<Result<TransactionDto>> IssueStampsAsync(IssueStampsRequest request)
        {
            try
            {
                var card = await _loyaltyCardRepository.GetByIdAsync(request.CardId);
                
                if (card == null)
                    return Result<TransactionDto>.Failure("Loyalty card not found.");
                    
                if (card.Type != LoyaltyProgramType.Stamp)
                    return Result<TransactionDto>.Failure("Cannot issue stamps to a points-based card.");
                    
                var program = await GetProgramAsync(card.ProgramId);
                
                if (program == null)
                    return Result<TransactionDto>.Failure("Loyalty program not found.");
                    
                // Check daily stamp limit
                if (program.DailyStampLimit.HasValue)
                {
                    var stampsIssuedToday = card.GetStampsIssuedToday();
                    if (stampsIssuedToday + request.Quantity > program.DailyStampLimit.Value)
                    {
                        return Result<TransactionDto>.Failure($"Daily stamp limit of {program.DailyStampLimit.Value} would be exceeded.");
                    }
                }
                
                var transaction = card.IssueStamps(
                    request.Quantity, 
                    request.StoreId, 
                    request.StaffId,
                    request.PosTransactionId);
                    
                await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    await _loyaltyCardRepository.UpdateAsync(card);
                    
                    // Publish event
                    await _eventPublisher.PublishAsync(new StampsIssuedEvent
                    {
                        CardId = card.Id.ToString(),
                        CustomerId = card.CustomerId.ToString(),
                        Quantity = request.Quantity,
                        StoreId = request.StoreId.ToString(),
                        Timestamp = DateTime.UtcNow
                    });
                });
                
                return Result<TransactionDto>.Success(MapToDto(transaction));
            }
            catch (Exception ex)
            {
                return Result<TransactionDto>.Failure($"Failed to issue stamps: {ex.Message}");
            }
        }

        /// <summary>
        /// Adds points to a loyalty card.
        /// </summary>
        public async Task<Result<TransactionDto>> AddPointsAsync(AddPointsRequest request)
        {
            try
            {
                var card = await _loyaltyCardRepository.GetByIdAsync(request.CardId);
                
                if (card == null)
                    return Result<TransactionDto>.Failure("Loyalty card not found.");
                    
                if (card.Type != LoyaltyProgramType.Points)
                    return Result<TransactionDto>.Failure("Cannot add points to a stamp-based card.");
                    
                var transaction = card.AddPoints(
                    request.Points, 
                    request.TransactionAmount, 
                    request.StoreId, 
                    request.StaffId,
                    request.PosTransactionId);
                    
                await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    await _loyaltyCardRepository.UpdateAsync(card);
                    
                    // Publish event
                    await _eventPublisher.PublishAsync(new PointsAddedEvent
                    {
                        CardId = card.Id.ToString(),
                        CustomerId = card.CustomerId.ToString(),
                        Points = request.Points,
                        TransactionAmount = request.TransactionAmount,
                        StoreId = request.StoreId.ToString(),
                        Timestamp = DateTime.UtcNow
                    });
                });
                
                return Result<TransactionDto>.Success(MapToDto(transaction));
            }
            catch (Exception ex)
            {
                return Result<TransactionDto>.Failure($"Failed to add points: {ex.Message}");
            }
        }

        /// <summary>
        /// Redeems a reward for a loyalty card.
        /// </summary>
        public async Task<Result<TransactionDto>> RedeemRewardAsync(RedeemRewardRequest request)
        {
            try
            {
                var card = await _loyaltyCardRepository.GetByIdAsync(request.CardId);
                
                if (card == null)
                    return Result<TransactionDto>.Failure("Loyalty card not found.");
                    
                var reward = await GetRewardAsync(request.RewardId);
                
                if (reward == null)
                    return Result<TransactionDto>.Failure("Reward not found.");
                    
                var transaction = card.RedeemReward(reward, request.StoreId, request.StaffId);
                
                await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    await _loyaltyCardRepository.UpdateAsync(card);
                    
                    // Publish event
                    await _eventPublisher.PublishAsync(new RewardRedeemedEvent
                    {
                        CardId = card.Id.ToString(),
                        CustomerId = card.CustomerId.ToString(),
                        RewardId = reward.Id.ToString(),
                        StoreId = request.StoreId.ToString(),
                        Timestamp = DateTime.UtcNow
                    });
                });
                
                return Result<TransactionDto>.Success(MapToDto(transaction));
            }
            catch (Exception ex)
            {
                return Result<TransactionDto>.Failure($"Failed to redeem reward: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets a loyalty card by its ID.
        /// </summary>
        public async Task<Result<LoyaltyCardDto>> GetCardByIdAsync(LoyaltyCardId id)
        {
            var card = await _loyaltyCardRepository.GetByIdAsync(id);
            
            if (card == null)
                return Result<LoyaltyCardDto>.Failure("Loyalty card not found.");
                
            return Result<LoyaltyCardDto>.Success(MapToDto(card));
        }

        /// <summary>
        /// Gets a loyalty card by its QR code.
        /// </summary>
        public async Task<Result<LoyaltyCardDto>> GetCardByQrCodeAsync(string qrCode)
        {
            var card = await _loyaltyCardRepository.GetByQrCodeAsync(qrCode);
            
            if (card == null)
                return Result<LoyaltyCardDto>.Failure("Loyalty card not found.");
                
            return Result<LoyaltyCardDto>.Success(MapToDto(card));
        }

        /// <summary>
        /// Gets all loyalty cards for a customer.
        /// </summary>
        public async Task<Result<List<LoyaltyCardDto>>> GetCardsByCustomerIdAsync(CustomerId customerId)
        {
            var cards = await _loyaltyCardRepository.GetByCustomerIdAsync(customerId);
            
            if (!cards.Any())
                return Result<List<LoyaltyCardDto>>.Success(new List<LoyaltyCardDto>());
                
            return Result<List<LoyaltyCardDto>>.Success(cards.Select(MapToDto).ToList());
        }

        // In a real implementation, this would be a domain service or repository method
        private Task<LoyaltyProgram> GetProgramAsync(LoyaltyProgramId programId)
        {
            // Mock implementation for demo purposes
            return Task.FromResult(new LoyaltyProgram
            {
                Id = programId,
                Type = LoyaltyProgramType.Stamp,
                DailyStampLimit = 3
            });
        }

        // In a real implementation, this would be a domain service or repository method
        private Task<Reward> GetRewardAsync(RewardId rewardId)
        {
            // Mock implementation for demo purposes
            return Task.FromResult(new Reward
            {
                Id = rewardId,
                ProgramId = EntityId.New<LoyaltyProgramId>(),
                IsActive = true,
                RequiredValue = 5
            });
        }

        private LoyaltyCardDto MapToDto(LoyaltyCard card)
        {
            return new LoyaltyCardDto
            {
                Id = card.Id,
                ProgramId = card.ProgramId,
                ProgramName = card.Program.Name,
                CustomerId = card.CustomerId,
                Type = card.Type.ToString(),
                StampsCollected = card.StampsCollected,
                PointsBalance = card.PointsBalance,
                Status = card.Status.ToString(),
                QrCode = card.QrCode,
                CreatedAt = card.CreatedAt,
                ExpiresAt = card.ExpiresAt,
                StampThreshold = card.Program.StampThreshold
            };
        }

        private TransactionDto MapToDto(Transaction transaction)
        {
            return new TransactionDto
            {
                Id = transaction.Id,
                CardId = transaction.CardId,
                Type = transaction.Type.ToString(),
                RewardId = transaction.RewardId,
                Quantity = transaction.Quantity,
                PointsAmount = transaction.PointsAmount,
                TransactionAmount = transaction.TransactionAmount,
                StoreId = transaction.StoreId,
                StaffId = transaction.StaffId,
                PosTransactionId = transaction.PosTransactionId,
                Timestamp = transaction.Timestamp
            };
        }
    }

    // Event classes for domain events
    public class CardCreatedEvent
    {
        public string CardId { get; set; }
        public string CustomerId { get; set; }
        public string ProgramId { get; set; }
        public string Type { get; set; }
        public DateTime Timestamp { get; set; }
    }
    
    public class StampsIssuedEvent
    {
        public string CardId { get; set; }
        public string CustomerId { get; set; }
        public int Quantity { get; set; }
        public string StoreId { get; set; }
        public DateTime Timestamp { get; set; }
    }
    
    public class PointsAddedEvent
    {
        public string CardId { get; set; }
        public string CustomerId { get; set; }
        public decimal Points { get; set; }
        public decimal TransactionAmount { get; set; }
        public string StoreId { get; set; }
        public DateTime Timestamp { get; set; }
    }
    
    public class RewardRedeemedEvent
    {
        public string CardId { get; set; }
        public string CustomerId { get; set; }
        public string RewardId { get; set; }
        public string StoreId { get; set; }
        public DateTime Timestamp { get; set; }
    }
} 