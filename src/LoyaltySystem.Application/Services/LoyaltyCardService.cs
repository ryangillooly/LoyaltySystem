using LoyaltySystem.Application.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Enums;
using LoyaltySystem.Domain.Repositories;

namespace LoyaltySystem.Application.Services
{
    public class LoyaltyCardService : ILoyaltyCardService
    {
        private readonly ILoyaltyCardRepository _cardRepository;
        private readonly ILoyaltyProgramRepository _programRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<LoyaltyCardService> _logger;

        public LoyaltyCardService() { }
        
        public LoyaltyCardService(
            ILoyaltyCardRepository cardRepository,
            ILoyaltyProgramRepository programRepository,
            ICustomerRepository customerRepository,
            IUnitOfWork unitOfWork,
            ILogger<LoyaltyCardService> logger)
        {
            _cardRepository = cardRepository ?? throw new ArgumentNullException(nameof(cardRepository));
            _programRepository = programRepository ?? throw new ArgumentNullException(nameof(programRepository));
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        public async Task<OperationResult<PagedResult<LoyaltyCardDto>>> GetAllAsync(int skip, int limit)
        {
            try
            {
                var customers = await _cardRepository.GetAllAsync(skip, limit);
                var totalCount = await _cardRepository.GetTotalCountAsync();

                var customerDtos = customers.Select(MapToDto).ToList();

                var result = new PagedResult<LoyaltyCardDto>(customerDtos, totalCount, skip, limit);

                return OperationResult<PagedResult<LoyaltyCardDto>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return OperationResult<PagedResult<LoyaltyCardDto>>.FailureResult($"Failed to get customers: {ex.Message}");
            }
        }
        public async Task<OperationResult<LoyaltyCardDto>> GetByIdAsync(LoyaltyCardId id)
        {
            try
            {
                var card = await _cardRepository.GetByIdAsync(id);
                if (card == null)
                {
                    return OperationResult<LoyaltyCardDto>.FailureResult($"Card with ID {id} not found");
                }

                return OperationResult<LoyaltyCardDto>.SuccessResult(MapToDto(card));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving card with ID {CardId}", id);
                return OperationResult<LoyaltyCardDto>.FailureResult($"Error retrieving card: {ex.Message}");
            }
        }

        public async Task<OperationResult<LoyaltyCardDto>> GetByQrCodeAsync(string qrCode)
        {
            try
            {
                var card = await _cardRepository.GetByQrCodeAsync(qrCode);
                if (card == null)
                {
                    return OperationResult<LoyaltyCardDto>.FailureResult("Card not found");
                }

                return OperationResult<LoyaltyCardDto>.SuccessResult(MapToDto(card));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting card by QR code {QrCode}", qrCode);
                return OperationResult<LoyaltyCardDto>.FailureResult($"Error retrieving card: {ex.Message}");
            }
        }

        public async Task<OperationResult<IEnumerable<LoyaltyCardDto>>> GetByCustomerIdAsync(CustomerId customerId)
        {
            try
            {
                var cards = await _cardRepository.GetByCustomerIdAsync(customerId);
                return OperationResult<IEnumerable<LoyaltyCardDto>>.SuccessResult(cards.Select(MapToDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cards for customer {CustomerId}", customerId);
                return OperationResult<IEnumerable<LoyaltyCardDto>>.FailureResult($"Error retrieving cards: {ex.Message}");
            }
        }

        public async Task<OperationResult<IEnumerable<LoyaltyCardDto>>> GetByProgramIdAsync(LoyaltyProgramId programId)
        {
            try
            {
                var cards = await _cardRepository.GetByProgramIdAsync(programId);
                return OperationResult<IEnumerable<LoyaltyCardDto>>.SuccessResult(cards.Select(MapToDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cards for program {ProgramId}", programId);
                return OperationResult<IEnumerable<LoyaltyCardDto>>.FailureResult($"Error retrieving cards: {ex.Message}");
            }
        }

        public async Task<OperationResult<LoyaltyCardDto>> CreateCardAsync(CreateLoyaltyCardDto dto)
        {
            var programId = EntityId.Parse<LoyaltyProgramId>(dto.ProgramId);
            var customerId = EntityId.Parse<CustomerId>(dto.CustomerId);
            try
            {
                // Check if customer exists
                var customer = await _customerRepository.GetByIdAsync(new CustomerId(customerId));
                if (customer == null)
                {
                    return OperationResult<LoyaltyCardDto>.FailureResult("Customer not found");
                }

                // Check if program exists
                var program = await _programRepository.GetByIdAsync(new LoyaltyProgramId(programId));
                if (program == null)
                {
                    return OperationResult<LoyaltyCardDto>.FailureResult("Loyalty program not found");
                }

                // Check if customer already has a card for this program
                var existingCards = await _cardRepository.GetByCustomerIdAsync(new CustomerId(customerId));
                if (existingCards.Any(c => c.ProgramId == programId))
                {
                    return OperationResult<LoyaltyCardDto>.FailureResult("Customer already enrolled in this program");
                }

                // Create new card
                // TODO: Should this create a Stamp or Points card? 
                var card = new LoyaltyCard(programId, customerId, LoyaltyProgramType.Points);

                await _cardRepository.AddAsync(card);
                await _unitOfWork.SaveChangesAsync();

                return OperationResult<LoyaltyCardDto>.SuccessResult(MapToDto(card));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating card for customer {CustomerId} in program {ProgramId}", dto.CustomerId, dto.ProgramId);
                return OperationResult<LoyaltyCardDto>.FailureResult($"Error creating card: {ex.Message}");
            }
        }

        public async Task<OperationResult<LoyaltyCardDto>> UpdateCardStatusAsync(LoyaltyCardId id, CardStatus status)
        {
            try
            {
                var card = await _cardRepository.GetByIdAsync(id);
                if (card == null)
                {
                    return OperationResult<LoyaltyCardDto>.FailureResult("Card not found");
                }

                // Use the appropriate domain method based on the requested status
                switch (status)
                {
                    case CardStatus.Expired:
                        card.Expire();
                        break;
                    case CardStatus.Suspended:
                        card.Suspend();
                        break;
                    case CardStatus.Active:
                        card.Reactivate();
                        break;
                    default:
                        return OperationResult<LoyaltyCardDto>.FailureResult($"Unsupported status change to {status}");
                }

                await _cardRepository.UpdateAsync(card);
                await _unitOfWork.SaveChangesAsync();

                return OperationResult<LoyaltyCardDto>.SuccessResult(MapToDto(card));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating card status for {CardId} to {Status}", id, status);
                return OperationResult<LoyaltyCardDto>.FailureResult($"Error updating card status: {ex.Message}");
            }
        }

        public async Task<OperationResult<TransactionDto>> IssueStampsAsync(
            LoyaltyCardId cardId, 
            int stampCount, 
            StoreId storeId, 
            decimal purchaseAmount, 
            string transactionReference)
        {
            try
            {
                var card = await _cardRepository.GetByIdAsync(cardId);
                
                if (card == null)
                {
                    return OperationResult<TransactionDto>.FailureResult($"Card with ID {cardId} not found");
                }
                
                if (card.Status != CardStatus.Active)
                {
                    return OperationResult<TransactionDto>.FailureResult($"Card is not active. Current status: {card.Status}");
                }
                
                if (card.Type != LoyaltyProgramType.Stamp)
                {
                    return OperationResult<TransactionDto>.FailureResult("Cannot issue stamps to a points-based card");
                }
                
                if (stampCount <= 0)
                {
                    return OperationResult<TransactionDto>.FailureResult("Stamp count must be greater than zero");
                }
                
                _logger.LogInformation($"Issuing {stampCount} stamps to card {cardId}");

                await _unitOfWork.BeginTransactionAsync();
                
                // Create transaction
                var transaction = new Transaction(
                    cardId.Value,
                    TransactionType.StampIssuance,
                    quantity: stampCount,
                    transactionAmount: purchaseAmount,
                    storeId: storeId.Value,
                    posTransactionId: transactionReference);
                
                // Update card
                card.StampsCollected += stampCount;
                card.UpdatedAt = DateTime.UtcNow;
                
                await _unitOfWork.TransactionRepository.AddAsync(transaction);
                await _cardRepository.UpdateAsync(card);
                await _unitOfWork.CommitTransactionAsync();
                
                _logger.LogInformation($"Successfully issued {stampCount} stamps to card {cardId}");
                
                return OperationResult<TransactionDto>.SuccessResult(MapToTransactionDto(transaction));
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error issuing stamps to card {0}: {1}", cardId, ex.Message);
                return OperationResult<TransactionDto>.FailureResult("Error issuing stamps: " + ex.Message);
            }
        }

        public async Task<OperationResult<TransactionDto>> AddPointsAsync(LoyaltyCardId cardId, decimal points, decimal purchaseAmount, StoreId storeId, Guid? staffId, string posTransactionId)
        {
            try
            {
                var card = await _cardRepository.GetByIdAsync(cardId);
                if (card == null)
                {
                    return OperationResult<TransactionDto>.FailureResult("Card not found");
                }

                if (card.Status != CardStatus.Active)
                {
                    return OperationResult<TransactionDto>.FailureResult($"Card is not active. Current status: {card.Status}");
                }

                if (card.Type != LoyaltyProgramType.Points)
                {
                    return OperationResult<TransactionDto>.FailureResult("Cannot add points to a stamp-based card");
                }

                if (points <= 0)
                {
                    return OperationResult<TransactionDto>.FailureResult("Points amount must be greater than zero");
                }

                _logger.LogInformation($"Adding {points} points to card {cardId}");

                // Create transaction
                var transaction = new Transaction(
                    cardId.Value,
                    TransactionType.PointsIssuance,
                    pointsAmount: points,
                    transactionAmount: purchaseAmount,
                    storeId: storeId.Value,
                    posTransactionId: posTransactionId);

                // Update card with points
                card.PointsBalance += points;
                card.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.TransactionRepository.AddAsync(transaction);
                await _cardRepository.UpdateAsync(card);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Successfully added {points} points to card {cardId}");

                return OperationResult<TransactionDto>.SuccessResult(MapToTransactionDto(transaction));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding points to card {CardId}", cardId);
                return OperationResult<TransactionDto>.FailureResult("Error adding points: " + ex.Message);
            }
        }

        public async Task<OperationResult<TransactionDto>> RedeemRewardAsync(LoyaltyCardId cardId, RewardId rewardId, StoreId storeId, Guid? staffId, RedeemRequestData redemptionData)
        {
            try
            {
                var card = await _cardRepository.GetByIdAsync(cardId);
                if (card == null)
                {
                    return OperationResult<TransactionDto>.FailureResult("Card not found");
                }

                if (card.Status != CardStatus.Active)
                {
                    return OperationResult<TransactionDto>.FailureResult($"Card is not active. Current status: {card.Status}");
                }

                var reward = await _programRepository.GetRewardByIdAsync(rewardId);
                if (reward == null)
                {
                    return OperationResult<TransactionDto>.FailureResult("Reward not found");
                }

                if (reward.ProgramId != card.ProgramId)
                {
                    return OperationResult<TransactionDto>.FailureResult("Reward does not belong to the card's program");
                }

                // Check if reward is active
                if (!reward.IsActive)
                {
                    return OperationResult<TransactionDto>.FailureResult("Reward is not active");
                }

                // Check if reward is within valid date range
                var now = DateTime.UtcNow;
                if (reward.ValidFrom.HasValue && reward.ValidFrom.Value > now)
                {
                    return OperationResult<TransactionDto>.FailureResult("Reward is not yet available");
                }

                if (reward.ValidTo.HasValue && reward.ValidTo.Value < now)
                {
                    return OperationResult<TransactionDto>.FailureResult("Reward has expired");
                }

                // Check if card has enough points/stamps based on card type
                if (card.Type == LoyaltyProgramType.Points && card.PointsBalance < reward.RequiredValue)
                {
                    return OperationResult<TransactionDto>.FailureResult($"Insufficient points. Required: {reward.RequiredValue}, Available: {card.PointsBalance}");
                }
                else if (card.Type == LoyaltyProgramType.Stamp && card.StampsCollected < reward.RequiredValue)
                {
                    return OperationResult<TransactionDto>.FailureResult($"Insufficient stamps. Required: {reward.RequiredValue}, Available: {card.StampsCollected}");
                }

                // Create transaction with the appropriate deduction values based on card type
                var transaction = new Transaction(
                    cardId.Value,
                    TransactionType.RewardRedemption,
                    rewardId: rewardId.Value,
                    quantity: card.Type == LoyaltyProgramType.Stamp ? -reward.RequiredValue : null,
                    pointsAmount: card.Type == LoyaltyProgramType.Points ? -reward.RequiredValue : null,
                    storeId: storeId.Value,
                    staffId: staffId);

                // Update card
                if (card.Type == LoyaltyProgramType.Points)
                {
                    card.PointsBalance -= reward.RequiredValue;
                }
                else if (card.Type == LoyaltyProgramType.Stamp)
                {
                    card.StampsCollected -= reward.RequiredValue;
                }

                card.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.TransactionRepository.AddAsync(transaction);
                await _cardRepository.UpdateAsync(card);
                await _unitOfWork.SaveChangesAsync();

                return OperationResult<TransactionDto>.SuccessResult(MapToTransactionDto(transaction));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error redeeming reward {RewardId} for card {CardId}", rewardId, cardId);
                return OperationResult<TransactionDto>.FailureResult($"Error redeeming reward: {ex.Message}");
            }
        }

        public async Task<OperationResult<string>> GenerateQrCodeAsync(LoyaltyCardId cardId)
        {
            try
            {
                var card = await _cardRepository.GetByIdAsync(cardId);
                if (card == null)
                {
                    return OperationResult<string>.FailureResult("Card not found");
                }

                // Generate a unique QR code
                var qrCode = Guid.NewGuid().ToString("N");
                
                card.UpdateQrCode(qrCode);

                await _cardRepository.UpdateAsync(card);
                await _unitOfWork.SaveChangesAsync();

                return OperationResult<string>.SuccessResult(qrCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR code for card {CardId}", cardId);
                return OperationResult<string>.FailureResult($"Error generating QR code: {ex.Message}");
            }
        }

        public async Task<OperationResult<string>> GetOrGenerateQrCodeAsync(LoyaltyCardId cardId)
        {
            try
            {
                var card = await _cardRepository.GetByIdAsync(cardId);
                if (card == null)
                {
                    return OperationResult<string>.FailureResult("Card not found");
                }

                if (!string.IsNullOrEmpty(card.QrCode))
                {
                    return OperationResult<string>.SuccessResult(card.QrCode);
                }

                return await GenerateQrCodeAsync(cardId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting or generating QR code for card {CardId}", cardId);
                return OperationResult<string>.FailureResult($"Error with QR code: {ex.Message}");
            }
        }

        public async Task<OperationResult<IEnumerable<TransactionDto>>> GetCardTransactionsAsync(LoyaltyCardId cardId)
        {
            try
            {
                var card = await _cardRepository.GetByIdAsync(cardId);
                if (card == null)
                {
                    return OperationResult<IEnumerable<TransactionDto>>.FailureResult("Card not found");
                }

                var transactions = await _unitOfWork.TransactionRepository.GetByCardIdAsync(cardId);
                return OperationResult<IEnumerable<TransactionDto>>.SuccessResult(transactions.Select(MapToTransactionDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transactions for card {CardId}", cardId);
                return OperationResult<IEnumerable<TransactionDto>>.FailureResult($"Error retrieving transactions: {ex.Message}");
            }
        }

        public async Task<OperationResult<int>> GetCardCountByStatusAsync(CardStatus status)
        {
            try
            {
                var counts = await _cardRepository.GetCardCountByStatusAsync(status);
                return OperationResult<int>.SuccessResult(counts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting card counts by status");
                return OperationResult<int>.FailureResult($"Error retrieving card counts: {ex.Message}");
            }
        }

        public async Task<OperationResult<DTOs.ProgramAnalyticsDto>> GetProgramCardAnalyticsAsync(LoyaltyProgramId programId)
        {
            try
            {
                _logger.LogInformation($"Retrieving program analytics for program {programId}");
                
                var program = await _programRepository.GetByIdAsync(programId);
                if (program == null)
                {
                    _logger.LogWarning($"Program not found: {programId}");
                    return OperationResult<DTOs.ProgramAnalyticsDto>.FailureResult("Program not found");
                }

                // Get the cards for the program
                var cards = await _cardRepository.GetByProgramIdAsync(programId);
                
                // Get all transactions for the program
                var transactions = await _unitOfWork.TransactionRepository.GetByProgramIdAsync(programId);
                
                // Get rewards for the program
                var rewards = await _programRepository.GetRewardsForProgramAsync(programId);
                
                // Build analytics
                var analytics = new DTOs.ProgramAnalyticsDto
                {
                    TotalPrograms = 1, // We're only looking at one program
                    ActivePrograms = program.IsActive ? 1 : 0,
                    StampPrograms = program.Type == LoyaltyProgramType.Stamp ? 1 : 0,
                    PointsPrograms = program.Type == LoyaltyProgramType.Points ? 1 : 0,
                    TotalRewards = rewards.Count(),
                    ActiveRewards = rewards.Count(r => r.IsActive),
                    ProgramsByBrand = new Dictionary<string, int> { { program.BrandId.ToString(), 1 } }
                };
                
                _logger.LogInformation($"Successfully retrieved program analytics for program {programId}");
                return OperationResult<DTOs.ProgramAnalyticsDto>.SuccessResult(analytics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving program analytics: {ex.Message}");
                return OperationResult<DTOs.ProgramAnalyticsDto>.FailureResult($"Error retrieving program analytics: {ex.Message}");
            }
        }

        public async Task<int> GetActiveCardCountForProgramAsync(string programId)
        {
            try
            {
                var programIdObj = new LoyaltyProgramId(Guid.Parse(programId));
                var cards = await _cardRepository.GetByProgramIdAsync(programIdObj);
                return cards.Count(c => c.Status == CardStatus.Active);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active card count for program {ProgramId}", programId);
                return 0;
            }
        }

        public async Task<int> GetCardCountForProgramAsync(string programId)
        {
            try
            {
                var programIdObj = new LoyaltyProgramId(Guid.Parse(programId));
                var cards = await _cardRepository.GetByProgramIdAsync(programIdObj);
                return cards.Count();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving card count for program {ProgramId}", programId);
                return 0;
            }
        }

        public async Task<decimal> GetAverageTransactionsPerCardForProgramAsync(string programId)
        {
            try
            {
                var programIdObj = new LoyaltyProgramId(Guid.Parse(programId));
                var cards = await _cardRepository.GetByProgramIdAsync(programIdObj);
                
                if (!cards.Any())
                {
                    return 0;
                }
                
                // Since we don't have access to transactions, we'll return 0
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating average transactions per card for program {ProgramId}", programId);
                return 0;
            }
        }

        private LoyaltyCardDto MapToDto(LoyaltyCard card)
        {
            return new LoyaltyCardDto
            {
                Id = card.Id,
                CustomerId = card.CustomerId,
                ProgramId = card.ProgramId,
                Status = card.Status,
                PointsBalance = card.PointsBalance,
                StampCount = card.StampsCollected,
                CreatedAt = card.CreatedAt,
                ExpiresAt = card.ExpiresAt,
                TotalTransactions = card.Transactions.Count
            };
        }

        private TransactionDto MapToTransactionDto(Transaction transaction)
        {
            return new TransactionDto
            {
                Id = transaction.Id.ToString(),
                CardId = transaction.CardId.ToString(),
                StoreId = transaction.StoreId.ToString(),
                StoreName = transaction.Store?.Name ?? string.Empty,
                TransactionDate = transaction.Timestamp,
                TransactionType = transaction.Type.ToString(),
                Amount = transaction.TransactionAmount ?? 0,
                PointsEarned = transaction.PointsAmount ?? 0,
                StampsEarned = transaction.Quantity ?? 0,
                RewardId = transaction.RewardId?.ToString(),
                RewardTitle = transaction.Reward?.Title ?? string.Empty,
                PosTransactionId = transaction.PosTransactionId ?? string.Empty
            };
        }
    }
} 