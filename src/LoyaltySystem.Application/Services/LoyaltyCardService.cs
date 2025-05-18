using LoyaltySystem.Application.Common;
using Microsoft.Extensions.Logging;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Enums;
using LoyaltySystem.Domain.Repositories;

namespace LoyaltySystem.Application.Services;

public class LoyaltyCardService : ILoyaltyCardService
{
    private readonly ILoyaltyCardRepository _cardRepository;
    private readonly ILoyaltyProgramRepository _programRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<LoyaltyCardService> _logger;
        
    /// <summary>
    /// Initializes a new instance of the <see cref="LoyaltyCardService"/> class with required repositories, unit of work, and logger.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown if any required dependency is null.</exception>
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
    
    /// <summary>
    /// Retrieves a paged list of loyalty cards.
    /// </summary>
    /// <param name="skip">The number of records to skip for pagination.</param>
    /// <param name="limit">The maximum number of records to return.</param>
    /// <returns>An operation result containing a paged result of loyalty card DTOs, or a failure result with an error message if retrieval fails.</returns>
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
    /// <summary>
    /// Retrieves a loyalty card by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the loyalty card.</param>
    /// <returns>An <see cref="OperationResult{LoyaltyCardDto}"/> containing the card details if found, or a failure result if not found or on error.</returns>
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

    /// <summary>
    /// Retrieves a loyalty card by its QR code.
    /// </summary>
    /// <param name="qrCode">The QR code associated with the loyalty card.</param>
    /// <returns>An operation result containing the loyalty card DTO if found; otherwise, a failure result with an error message.</returns>
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

    /// <summary>
    /// Retrieves all loyalty cards associated with a specific customer.
    /// </summary>
    /// <param name="customerId">The unique identifier of the customer.</param>
    /// <returns>An operation result containing a collection of loyalty card DTOs if successful; otherwise, a failure result with an error message.</returns>
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

    /// <summary>
    /// Retrieves all loyalty cards associated with a specific loyalty program.
    /// </summary>
    /// <param name="programId">The unique identifier of the loyalty program.</param>
    /// <returns>An operation result containing a collection of loyalty card DTOs if successful; otherwise, a failure result with an error message.</returns>
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

    /// <summary>
    /// Creates a new loyalty card for a customer in a specified loyalty program.
    /// </summary>
    /// <param name="dto">The data required to create the loyalty card, including customer and program identifiers.</param>
    /// <returns>An operation result containing the created loyalty card DTO on success, or an error message on failure.</returns>
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

    /// <summary>
    /// Updates the status of a loyalty card to the specified value.
    /// </summary>
    /// <param name="id">The unique identifier of the loyalty card.</param>
    /// <param name="status">The new status to set for the card (Expired, Suspended, or Active).</param>
    /// <returns>An <see cref="OperationResult{LoyaltyCardDto}"/> containing the updated card data if successful, or an error message if the operation fails.</returns>
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

    /// <summary>
    /// Issues a specified number of stamps to a stamp-based loyalty card and records the transaction.
    /// </summary>
    /// <param name="cardId">The unique identifier of the loyalty card.</param>
    /// <param name="stampCount">The number of stamps to issue.</param>
    /// <param name="storeId">The identifier of the store where the transaction occurred.</param>
    /// <param name="purchaseAmount">The purchase amount associated with the stamp issuance.</param>
    /// <param name="transactionReference">The reference identifier for the transaction.</param>
    /// <returns>An <see cref="OperationResult{TransactionDto}"/> containing the transaction details if successful, or an error message if the operation fails.</returns>
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
                return OperationResult<TransactionDto>.FailureResult($"Card with ID {cardId} not found");
                
            if (card.Status != CardStatus.Active)
                return OperationResult<TransactionDto>.FailureResult($"Card is not active. Current status: {card.Status}");
                
            if (card.Type != LoyaltyProgramType.Stamp)
                return OperationResult<TransactionDto>.FailureResult("Cannot issue stamps to a points-based card");
                
            if (stampCount <= 0)
                return OperationResult<TransactionDto>.FailureResult("Stamp count must be greater than zero");
                
            _logger.LogInformation($"Issuing {stampCount} stamps to card {cardId}");

            await _unitOfWork.BeginTransactionAsync();
                
            // Create transaction
            var transaction = new Transaction
            (
                new LoyaltyCardId(cardId.Value),
                TransactionType.StampIssuance,
                quantity: stampCount,
                transactionAmount: purchaseAmount,
                storeId: new StoreId(storeId.Value),
                posTransactionId: transactionReference
            );
                
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

    /// <summary>
    /// Adds points to a points-based loyalty card and records the transaction.
    /// </summary>
    /// <param name="cardId">The unique identifier of the loyalty card.</param>
    /// <param name="points">The number of points to add.</param>
    /// <param name="purchaseAmount">The purchase amount associated with the points issuance.</param>
    /// <param name="storeId">The identifier of the store where the transaction occurred.</param>
    /// <param name="staffId">The identifier of the staff member processing the transaction, if applicable.</param>
    /// <param name="posTransactionId">The point-of-sale transaction identifier.</param>
    /// <returns>An operation result containing the transaction details if successful; otherwise, a failure result with an error message.</returns>
    public async Task<OperationResult<TransactionDto>> AddPointsAsync(LoyaltyCardId cardId, decimal points, decimal purchaseAmount, StoreId storeId, StaffId? staffId, string posTransactionId)
    {
        try
        {
            var card = await _cardRepository.GetByIdAsync(cardId);
            if (card == null)
                return OperationResult<TransactionDto>.FailureResult("Card not found");

            if (card.Status != CardStatus.Active)
                return OperationResult<TransactionDto>.FailureResult($"Card is not active. Current status: {card.Status}");

            if (card.Type != LoyaltyProgramType.Points)
                return OperationResult<TransactionDto>.FailureResult("Cannot add points to a stamp-based card");

            if (points <= 0)
                return OperationResult<TransactionDto>.FailureResult("Points amount must be greater than zero");

            _logger.LogInformation($"Adding {points} points to card {cardId}");

            // Create transaction
            var transaction = new Transaction
            (
                cardId,
                TransactionType.PointsIssuance,
                pointsAmount: points,
                transactionAmount: purchaseAmount,
                storeId: storeId,
                posTransactionId: posTransactionId
            );

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

    /// <summary>
    /// Redeems a reward for a loyalty card, deducting the required points or stamps and recording the transaction.
    /// </summary>
    /// <param name="cardId">The ID of the loyalty card.</param>
    /// <param name="rewardId">The ID of the reward to redeem.</param>
    /// <param name="storeId">The ID of the store where the redemption occurs.</param>
    /// <param name="staffId">The ID of the staff member processing the redemption, if applicable.</param>
    /// <param name="redemptionData">Additional data related to the redemption request.</param>
    /// <returns>An <see cref="OperationResult{TransactionDto}"/> containing the reward redemption transaction if successful, or a failure result with an error message.</returns>
    public async Task<OperationResult<TransactionDto>> RedeemRewardAsync(LoyaltyCardId cardId, RewardId rewardId, StoreId storeId, StaffId? staffId, RedeemRequestData redemptionData)
    {
        try
        {
            var card = await _cardRepository.GetByIdAsync(cardId);
            if (card == null)
                return OperationResult<TransactionDto>.FailureResult("Card not found");

            if (card.Status != CardStatus.Active)
                return OperationResult<TransactionDto>.FailureResult($"Card is not active. Current status: {card.Status}");

            var reward = await _programRepository.GetRewardByIdAsync(rewardId);
            if (reward == null)
                return OperationResult<TransactionDto>.FailureResult("Reward not found");

            if (reward.ProgramId != card.ProgramId)
                return OperationResult<TransactionDto>.FailureResult("Reward does not belong to the card's program");

            // Check if reward is active
            if (!reward.IsActive)
                return OperationResult<TransactionDto>.FailureResult("Reward is not active");

            // Check if reward is within valid date range
            var now = DateTime.UtcNow;
            if (reward.ValidFrom.HasValue && reward.ValidFrom.Value > now)
                return OperationResult<TransactionDto>.FailureResult("Reward is not yet available");

            if (reward.ValidTo.HasValue && reward.ValidTo.Value < now)
                return OperationResult<TransactionDto>.FailureResult("Reward has expired");

            // Check if card has enough points/stamps based on card type
            if (card.Type == LoyaltyProgramType.Points && card.PointsBalance < reward.RequiredValue)
                return OperationResult<TransactionDto>.FailureResult($"Insufficient points. Required: {reward.RequiredValue}, Available: {card.PointsBalance}");
                
            if (card.Type == LoyaltyProgramType.Stamp && card.StampsCollected < reward.RequiredValue)
                return OperationResult<TransactionDto>.FailureResult($"Insufficient stamps. Required: {reward.RequiredValue}, Available: {card.StampsCollected}");

            // Create transaction with the appropriate deduction values based on card type
            var transaction = new Transaction
            (
                new LoyaltyCardId(cardId.Value),
                TransactionType.RewardRedemption,
                rewardId: new RewardId(rewardId.Value),
                quantity: card.Type == LoyaltyProgramType.Stamp ? -reward.RequiredValue : null,
                pointsAmount: card.Type == LoyaltyProgramType.Points ? -reward.RequiredValue : null,
                storeId: new StoreId(storeId.Value),
                staffId: new StaffId(staffId.Value)
            );

            switch (card.Type)
            {
                case LoyaltyProgramType.Points:
                    card.PointsBalance -= reward.RequiredValue;
                    break;
                case LoyaltyProgramType.Stamp:
                    card.StampsCollected -= reward.RequiredValue;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(card.Type), card.Type, null);
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

    /// <summary>
    /// Generates a new unique QR code for the specified loyalty card and updates the card with the generated code.
    /// </summary>
    /// <param name="cardId">The identifier of the loyalty card.</param>
    /// <returns>An operation result containing the generated QR code string if successful; otherwise, a failure result with an error message.</returns>
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

    /// <summary>
    /// Retrieves the QR code for a loyalty card, generating a new one if it does not exist.
    /// </summary>
    /// <param name="cardId">The unique identifier of the loyalty card.</param>
    /// <returns>An operation result containing the QR code string if successful; otherwise, an error message.</returns>
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

    /// <summary>
    /// Retrieves all transactions associated with a specified loyalty card.
    /// </summary>
    /// <param name="cardId">The unique identifier of the loyalty card.</param>
    /// <returns>An operation result containing a collection of transaction DTOs if successful; otherwise, a failure result with an error message.</returns>
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

    /// <summary>
    /// Retrieves the total number of loyalty cards with the specified status.
    /// </summary>
    /// <param name="status">The status to filter loyalty cards by.</param>
    /// <returns>An <see cref="OperationResult{T}"/> containing the count of cards with the given status, or a failure result if an error occurs.</returns>
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

    /// <summary>
    /// Retrieves analytics data for a specific loyalty program, including counts of cards, rewards, and program types.
    /// </summary>
    /// <param name="programId">The unique identifier of the loyalty program.</param>
    /// <returns>An <see cref="OperationResult{ProgramAnalyticsDto}"/> containing the analytics data if successful; otherwise, a failure result with an error message.</returns>
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

    /// <summary>
    /// Retrieves the number of active loyalty cards for a specified loyalty program.
    /// </summary>
    /// <param name="programId">The unique identifier of the loyalty program as a string.</param>
    /// <returns>The count of active loyalty cards for the given program, or 0 if an error occurs.</returns>
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

    /// <summary>
    /// Retrieves the total number of loyalty cards associated with a specified loyalty program.
    /// </summary>
    /// <param name="programId">The unique identifier of the loyalty program as a string.</param>
    /// <returns>The count of loyalty cards for the program, or 0 if an error occurs.</returns>
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

    /// <summary>
    /// Returns the average number of transactions per loyalty card for a specified program.
    /// </summary>
    /// <param name="programId">The unique identifier of the loyalty program.</param>
    /// <returns>The average transaction count per card, or 0 if unavailable or on error.</returns>
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
    /// <summary>
    /// Verifies whether a loyalty card belongs to the specified customer.
    /// </summary>
    /// <param name="cardId">The unique identifier of the loyalty card.</param>
    /// <param name="customerId">The unique identifier of the customer.</param>
    /// <returns>A result indicating success if the card belongs to the customer, or failure with an error message otherwise.</returns>
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

    /// <summary>
        /// Maps a <see cref="LoyaltyCard"/> entity to a <see cref="LoyaltyCardDto"/>.
        /// </summary>
        /// <param name="card">The loyalty card entity to map.</param>
        /// <returns>A <see cref="LoyaltyCardDto"/> containing the card's details and transactions.</returns>
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

    /// <summary>
        /// Converts a <see cref="Transaction"/> entity to a <see cref="TransactionDto"/> for data transfer.
        /// </summary>
        /// <param name="transaction">The transaction entity to map.</param>
        /// <returns>A <see cref="TransactionDto"/> containing transaction details.</returns>
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