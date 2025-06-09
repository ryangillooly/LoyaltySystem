using LoyaltySystem.Application.Common;
using Microsoft.Extensions.Logging;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Enums;
using LoyaltySystem.Domain.Repositories;
using LoyaltySystem.Domain.ValueObjects;

namespace LoyaltySystem.Application.Services;

public class LoyaltyCardService : ILoyaltyCardService
{
    private readonly ILoyaltyCardRepository _cardRepository;
    private readonly ILoyaltyProgramRepository _programRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IStoreRepository _storeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDomainEventPublisher _domainEventPublisher;
    private readonly ILogger<LoyaltyCardService> _logger;
        
    public LoyaltyCardService(
        ILoyaltyCardRepository cardRepository,
        ILoyaltyProgramRepository programRepository,
        ICustomerRepository customerRepository,
        IStoreRepository storeRepository,
        IUnitOfWork unitOfWork,
        IDomainEventPublisher domainEventPublisher,
        ILogger<LoyaltyCardService> logger)
    {
        _cardRepository = cardRepository ?? throw new ArgumentNullException(nameof(cardRepository));
        _programRepository = programRepository ?? throw new ArgumentNullException(nameof(programRepository));
        _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
        _storeRepository = storeRepository ?? throw new ArgumentNullException(nameof(storeRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _domainEventPublisher = domainEventPublisher ?? throw new ArgumentNullException(nameof(domainEventPublisher));
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
            return card == null 
                ? OperationResult<LoyaltyCardDto>.FailureResult($"Card with ID {id} not found") 
                : OperationResult<LoyaltyCardDto>.SuccessResult(MapToDto(card));

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
            return card == null
                ? OperationResult<LoyaltyCardDto>.FailureResult("Card not found") 
                : OperationResult<LoyaltyCardDto>.SuccessResult(MapToDto(card));

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
            var customer = await _customerRepository.GetByIdAsync(new CustomerId(customerId));
            if (customer == null)
                return OperationResult<LoyaltyCardDto>.FailureResult("Customer not found");

            // Check if program exists
            var program = await _programRepository.GetByIdAsync(new LoyaltyProgramId(programId));
            if (program == null)
                return OperationResult<LoyaltyCardDto>.FailureResult("Loyalty program not found");

            // Check if customer already has a card for this program
            var existingCards = await _cardRepository.GetByCustomerIdAsync(new CustomerId(customerId));
            if (existingCards.Any(c => c.ProgramId == programId))
                return OperationResult<LoyaltyCardDto>.FailureResult("Customer already enrolled in this program");

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
                return OperationResult<LoyaltyCardDto>.FailureResult("Card not found");
                
                
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

    public async Task<OperationResult<TransactionDto>> IssueStampsAsync(LoyaltyCardId cardId, int stampCount, StoreId storeId, decimal purchaseAmount, string transactionReference)
    {
        try
        {
            var card = await _cardRepository.GetByIdAsync(cardId);
            if (card == null)
                return OperationResult<TransactionDto>.FailureResult("Loyalty card not found");

            var program = await _programRepository.GetByIdAsync(card.ProgramId);
            if (program == null)
                return OperationResult<TransactionDto>.FailureResult("Loyalty program not found");

            var store = await _storeRepository.GetByIdAsync(storeId);
            if (store == null)
                return OperationResult<TransactionDto>.FailureResult("Store not found");

            // Use rich domain method that encapsulates business rules and raises events
            var transaction = card.IssueStamps(
                quantity: stampCount,
                storeId: storeId,
                programName: program.Name,
                storeName: store.Name,
                fraudPolicy: program.FraudPolicyOverride,
                dailyLimits: program.DailyLimitsOverride);

            await _cardRepository.UpdateAsync(card);
            
            // Publish domain events before committing transaction
            await _domainEventPublisher.PublishEventsAsync(card);
            
            await _unitOfWork.CommitTransactionAsync();

            return OperationResult<TransactionDto>.SuccessResult(MapToTransactionDto(transaction));
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Domain validation failed for stamp issuance on card {CardId}", cardId);
            await _unitOfWork.RollbackTransactionAsync();
            return OperationResult<TransactionDto>.FailureResult(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error issuing stamps for card {CardId}", cardId);
            await _unitOfWork.RollbackTransactionAsync();
            return OperationResult<TransactionDto>.FailureResult("An error occurred while issuing stamps");
        }
    }

    public async Task<OperationResult<TransactionDto>> AddPointsAsync(LoyaltyCardId cardId, decimal points, decimal transactionAmount, StoreId storeId, StaffId? staffId, string posTransactionId)
    {
        try
        {
            var card = await _cardRepository.GetByIdAsync(cardId);
            if (card == null)
                return OperationResult<TransactionDto>.FailureResult("Loyalty card not found");

            var program = await _programRepository.GetByIdAsync(card.ProgramId);
            if (program == null)
                return OperationResult<TransactionDto>.FailureResult("Loyalty program not found");

            var store = await _storeRepository.GetByIdAsync(storeId);
            if (store == null)
                return OperationResult<TransactionDto>.FailureResult("Store not found");

            // Use rich domain method that encapsulates business rules and raises events
            var transaction = card.AddPoints(
                pointsAmount: points,
                transactionAmount: transactionAmount,
                storeId: storeId,
                programName: program.Name,
                storeName: store.Name,
                conversionRate: program.ConversionRateOverride?.PointsToCurrency ?? program.PointsConversionRate ?? 1.0m,
                fraudPolicy: program.FraudPolicyOverride,
                dailyLimits: program.DailyLimitsOverride,
                staffId: staffId,
                posTransactionId: posTransactionId,
                currencyValue: program.ConversionRateOverride?.PointsToCurrency ?? 0.01m,
                minimumRedemptionPoints: program.MinimumPointsForRedemption);

            await _cardRepository.UpdateAsync(card);
            
            // Publish domain events before committing transaction
            await _domainEventPublisher.PublishEventsAsync(card);
            
            await _unitOfWork.CommitTransactionAsync();

            return OperationResult<TransactionDto>.SuccessResult(MapToTransactionDto(transaction));
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Domain validation failed for points addition on card {CardId}", cardId);
            await _unitOfWork.RollbackTransactionAsync();
            return OperationResult<TransactionDto>.FailureResult(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding points for card {CardId}", cardId);
            await _unitOfWork.RollbackTransactionAsync();
            return OperationResult<TransactionDto>.FailureResult("An error occurred while adding points");
        }
    }

    public async Task<OperationResult<TransactionDto>> RedeemRewardAsync(LoyaltyCardId cardId, RewardId rewardId, StoreId storeId, StaffId? staffId, RedeemRequestData redemptionData)
    {
        try
        {
            var card = await _cardRepository.GetByIdAsync(cardId);
            if (card == null)
                return OperationResult<TransactionDto>.FailureResult("Loyalty card not found");

            var program = await _programRepository.GetByIdAsync(card.ProgramId);
            if (program == null)
                return OperationResult<TransactionDto>.FailureResult("Loyalty program not found");

            var store = await _storeRepository.GetByIdAsync(storeId);
            if (store == null)
                return OperationResult<TransactionDto>.FailureResult("Store not found");

            // Find the reward in the program
            var reward = program.Rewards.FirstOrDefault(r => r.Id == rewardId);
            if (reward == null)
                return OperationResult<TransactionDto>.FailureResult("Reward not found in program");

            // Calculate currency value for the reward
            var currencyValue = program.ConversionRateOverride?.ConvertPointsToCurrency(reward.RequiredValue) ?? 
                               (reward.RequiredValue * (program.PointsConversionRate ?? 0.01m));

            // Use rich domain method that encapsulates business rules and raises events
            var transaction = card.RedeemReward(
                reward: reward,
                storeId: storeId,
                programName: program.Name,
                storeName: store.Name,
                fraudPolicy: program.FraudPolicyOverride,
                dailyLimits: program.DailyLimitsOverride,
                staffId: staffId,
                currencyValue: currencyValue);

            await _cardRepository.UpdateAsync(card);
            
            // Publish domain events before committing transaction
            await _domainEventPublisher.PublishEventsAsync(card);
            
            await _unitOfWork.CommitTransactionAsync();

            return OperationResult<TransactionDto>.SuccessResult(MapToTransactionDto(transaction));
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Domain validation failed for reward redemption on card {CardId}", cardId);
            await _unitOfWork.RollbackTransactionAsync();
            return OperationResult<TransactionDto>.FailureResult(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error redeeming reward for card {CardId}", cardId);
            await _unitOfWork.RollbackTransactionAsync();
            return OperationResult<TransactionDto>.FailureResult("An error occurred while redeeming reward");
        }
    }

    public async Task<OperationResult<string>> GenerateQrCodeAsync(LoyaltyCardId cardId)
    {
        try
        {
            var card = await _cardRepository.GetByIdAsync(cardId);
            if (card == null)
                return OperationResult<string>.FailureResult("Card not found");

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
                return OperationResult<string>.FailureResult("Card not found");

            if (!string.IsNullOrEmpty(card.QrCode))
                return OperationResult<string>.SuccessResult(card.QrCode);

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
                return OperationResult<IEnumerable<TransactionDto>>.FailureResult("Card not found");

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
                
            return !cards.Any() ? 0 :
                // Since we don't have access to transactions, we'll return 0
                // TODO: Implement this
                0;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating average transactions per card for program {ProgramId}", programId);
            return 0;
        }
    }

    /// <summary>
    /// Verifies that a loyalty card belongs to a specific customer
    /// </summary>
    /// <param name="cardId">The ID of the loyalty card to check</param>
    /// <param name="customerId">The ID of the customer</param>
    /// <returns>Success result if the card belongs to the customer, failure otherwise</returns>
    public async Task<OperationResult<bool>> VerifyCardOwnership(string cardId, CustomerId customerId)
    {
        try
        {
            // Parse the card ID
            var loyaltyCardId = EntityId.Parse<LoyaltyCardId>(cardId);
                
            // Get the card from the repository
            var card = await _cardRepository.GetByIdAsync(loyaltyCardId);
                
            // Check if the card exists
            if (card == null)
                return OperationResult<bool>.FailureResult($"Loyalty card with ID {cardId} not found");
                
            // Check if the card belongs to the customer
            if (card.CustomerId != customerId.Value)
            {
                _logger.LogWarning("Card ownership verification failed: Card {CardId} doesn't belong to customer {CustomerId}", 
                    cardId, customerId);
                return OperationResult<bool>.FailureResult("The loyalty card doesn't belong to the specified customer");
            }
                
            return OperationResult<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying card ownership for card {CardId} and customer {CustomerId}", 
                cardId, customerId);
            return OperationResult<bool>.FailureResult($"Error verifying card ownership: {ex.Message}");
        }
    }

    private static LoyaltyCardDto MapToDto(LoyaltyCard card) =>
        new ()
        {
            Id = card.Id,
            CustomerId = card.CustomerId,
            ProgramId = card.ProgramId,
            Type = card.Type,
            Status = card.Status,
            PointsBalance = card.PointsBalance,
            StampCount = card.StampsCollected,
            QrCode = card.QrCode,
            CreatedAt = card.CreatedAt,
            ExpiresAt = card.ExpiresAt,
            Transactions = card.Transactions,
            TotalTransactions = card.Transactions.Count
        };

    private static TransactionDto MapToTransactionDto(Transaction transaction) =>
        new ()
        {
            Id = transaction.Id.ToString(),
            CardId = transaction.CardId.ToString(),
            StoreId = transaction.StoreId.ToString(),
            //StoreName = transaction.Store?.Name ?? string.Empty,
            TransactionDate = transaction.Timestamp,
            TransactionType = transaction.Type.ToString(),
            Amount = transaction.TransactionAmount ?? 0,
            PointsEarned = transaction.PointsAmount ?? 0,
            StampsEarned = transaction.Quantity ?? 0,
            RewardId = transaction.RewardId?.ToString(),
            //RewardTitle = transaction.Reward?.Title ?? string.Empty,
            PosTransactionId = transaction.PosTransactionId ?? string.Empty
        };
}