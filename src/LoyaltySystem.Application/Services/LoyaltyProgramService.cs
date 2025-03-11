using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LoyaltySystem.Application.Common;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Repositories;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;

namespace LoyaltySystem.Application.Services
{
    public class LoyaltyProgramService
    {
        private readonly ILoyaltyProgramRepository _programRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly IUnitOfWork _unitOfWork;

        public LoyaltyProgramService(
            ILoyaltyProgramRepository programRepository,
            IBrandRepository brandRepository,
            IUnitOfWork unitOfWork)
        {
            _programRepository = programRepository ?? throw new ArgumentNullException(nameof(programRepository));
            _brandRepository = brandRepository ?? throw new ArgumentNullException(nameof(brandRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<OperationResult<LoyaltyProgramDto>> GetProgramByIdAsync(string id)
        {
            try
            {
                var programId = EntityId.Parse<LoyaltyProgramId>(id);
                var program = await _programRepository.GetByIdAsync(programId);

                if (program == null)
                {
                    return OperationResult<LoyaltyProgramDto>.FailureResult($"Loyalty program with ID {id} not found");
                }

                return OperationResult<LoyaltyProgramDto>.SuccessResult(MapToDto(program));
            }
            catch (Exception ex)
            {
                return OperationResult<LoyaltyProgramDto>.FailureResult($"Failed to get loyalty program: {ex.Message}");
            }
        }

        public async Task<OperationResult<PagedResult<LoyaltyProgramDto>>> GetAllProgramsAsync(int page, int pageSize)
        {
            try
            {
                var programs = await _programRepository.GetByBrandIdAsync(null);
                var programDtos = new List<LoyaltyProgramDto>();

                foreach (var program in programs)
                {
                    programDtos.Add(MapToDto(program));
                }

                var totalCount = programDtos.Count; // This is not accurate, but we don't have a count method in the repository
                var result = new PagedResult<LoyaltyProgramDto>(programDtos, totalCount, page, pageSize);

                return OperationResult<PagedResult<LoyaltyProgramDto>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return OperationResult<PagedResult<LoyaltyProgramDto>>.FailureResult($"Failed to get loyalty programs: {ex.Message}");
            }
        }

        public async Task<OperationResult<List<LoyaltyProgramDto>>> GetProgramsByBrandIdAsync(string brandId)
        {
            try
            {
                var brandIdObj = new BrandId(Guid.Parse(brandId));
                var programs = await _programRepository.GetByBrandIdAsync(brandIdObj);
                var programDtos = programs.Select(MapToDto).ToList();
                
                return OperationResult<List<LoyaltyProgramDto>>.SuccessResult(programDtos);
            }
            catch (Exception ex)
            {
                return OperationResult<List<LoyaltyProgramDto>>.FailureResult($"Failed to get loyalty programs: {ex.Message}");
            }
        }

        public async Task<OperationResult<PagedResult<LoyaltyProgramDto>>> GetProgramsByTypeAsync(string type, int page, int pageSize)
        {
            try
            {
                if (!Enum.TryParse<LoyaltyProgramType>(type, true, out var programType))
                {
                    return OperationResult<PagedResult<LoyaltyProgramDto>>.FailureResult($"Invalid program type: {type}");
                }
                
                var programs = await _programRepository.GetByTypeAsync(programType, page, pageSize);
                var totalCount = await _programRepository.GetCountByTypeAsync(programType);
                
                var programDtos = programs.Select(MapToDto).ToList();
                var result = new PagedResult<LoyaltyProgramDto>(programDtos, totalCount, page, pageSize);
                
                return OperationResult<PagedResult<LoyaltyProgramDto>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return OperationResult<PagedResult<LoyaltyProgramDto>>.FailureResult($"Failed to get loyalty programs by type: {ex.Message}");
            }
        }

        public async Task<OperationResult<LoyaltyProgramDto>> CreateProgramAsync(CreateLoyaltyProgramDto dto)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                
                var brandId = new BrandId(Guid.Parse(dto.BrandId));
                var brand = await _brandRepository.GetByIdAsync(brandId);
                
                if (brand == null)
                    return OperationResult<LoyaltyProgramDto>.FailureResult("Brand not found");
                
                // Create expiration policy based on DTO
                ExpirationPolicy expirationPolicy = null;
                if (dto.HasExpiration)
                {
                    if (dto.ExpiresOnSpecificDate)
                    {
                        expirationPolicy = new ExpirationPolicy(
                            dto.ExpirationDay.Value,
                            dto.ExpirationMonth);
                    }
                    else
                    {
                        var expirationType = (ExpirationType)dto.ExpirationType.Value;
                        expirationPolicy = new ExpirationPolicy(
                            expirationType,
                            dto.ExpirationValue.Value);
                    }
                }
                
                var program = new LoyaltyProgram(
                    brandId.Value,
                    dto.Name,
                    (LoyaltyProgramType)dto.Type,
                    dto.StampThreshold,
                    dto.PointsConversionRate,
                    dto.DailyStampLimit,
                    dto.MinimumTransactionAmount,
                    expirationPolicy);
                
                await _programRepository.AddAsync(program);
                await _unitOfWork.CommitTransactionAsync();
                
                return OperationResult<LoyaltyProgramDto>.SuccessResult(MapToDto(program));
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return OperationResult<LoyaltyProgramDto>.FailureResult(ex.Message);
            }
        }

        public async Task<OperationResult<LoyaltyProgramDto>> UpdateProgramAsync(string programId, UpdateLoyaltyProgramDto dto)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                
                var programIdObj = new LoyaltyProgramId(Guid.Parse(programId));
                var program = await _programRepository.GetByIdAsync(programIdObj);
                
                if (program == null)
                    return OperationResult<LoyaltyProgramDto>.FailureResult("Program not found");
                
                // Create expiration policy based on DTO
                ExpirationPolicy expirationPolicy = null;
                if (dto.HasExpiration)
                {
                    if (dto.ExpiresOnSpecificDate)
                    {
                        expirationPolicy = new ExpirationPolicy(
                            dto.ExpirationDay.Value,
                            dto.ExpirationMonth);
                    }
                    else
                    {
                        var expirationType = (ExpirationType)dto.ExpirationType.Value;
                        expirationPolicy = new ExpirationPolicy(
                            expirationType,
                            dto.ExpirationValue.Value);
                    }
                }
                
                program.Update(
                    dto.Name,
                    dto.StampThreshold,
                    dto.PointsConversionRate,
                    dto.DailyStampLimit,
                    dto.MinimumTransactionAmount,
                    expirationPolicy);
                
                await _programRepository.UpdateAsync(program);
                await _unitOfWork.CommitTransactionAsync();
                
                return OperationResult<LoyaltyProgramDto>.SuccessResult(MapToDto(program));
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return OperationResult<LoyaltyProgramDto>.FailureResult(ex.Message);
            }
        }

        public async Task<OperationResult<RewardDto>> AddRewardToProgramAsync(string programId, CreateRewardDto dto)
        {
            try
            {
                var pId = EntityId.Parse<LoyaltyProgramId>(programId);
                var program = await _programRepository.GetByIdAsync(pId);

                if (program == null)
                {
                    return OperationResult<RewardDto>.FailureResult($"Loyalty program with ID {programId} not found");
                }

                var reward = new Reward(
                    program.Id,
                    dto.Title,
                    dto.Description,
                    dto.RequiredPoints > 0 ? dto.RequiredPoints : dto.RequiredStamps,
                    dto.StartDate,
                    dto.EndDate
                );

                await _unitOfWork.BeginTransactionAsync();
                await _programRepository.AddRewardAsync(reward);
                await _unitOfWork.CommitTransactionAsync();

                return OperationResult<RewardDto>.SuccessResult(MapToRewardDto(reward));
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return OperationResult<RewardDto>.FailureResult($"Failed to add reward to program: {ex.Message}");
            }
        }

        public async Task<OperationResult<RewardDto>> UpdateRewardAsync(string rewardId, UpdateRewardDto dto)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                
                var rewardIdObj = new RewardId(Guid.Parse(rewardId));
                var reward = await _programRepository.GetRewardByIdAsync(rewardIdObj);
                
                if (reward == null)
                    return OperationResult<RewardDto>.FailureResult("Reward not found");
                
                reward.Update(
                    dto.Title,
                    dto.Description,
                    dto.RequiredPoints > 0 ? dto.RequiredPoints : dto.RequiredStamps,
                    dto.StartDate,
                    dto.EndDate
                );
                
                await _programRepository.UpdateRewardAsync(reward);
                await _unitOfWork.CommitTransactionAsync();
                
                return OperationResult<RewardDto>.SuccessResult(MapToRewardDto(reward));
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return OperationResult<RewardDto>.FailureResult(ex.Message);
            }
        }

        public async Task<OperationResult<List<RewardDto>>> GetRewardsByProgramIdAsync(string programId)
        {
            try
            {
                var programIdObj = new LoyaltyProgramId(Guid.Parse(programId));
                var rewards = await _programRepository.GetRewardsForProgramAsync(programIdObj);
                var rewardDtos = rewards.Select(MapToRewardDto).ToList();
                
                return OperationResult<List<RewardDto>>.SuccessResult(rewardDtos);
            }
            catch (Exception ex)
            {
                return OperationResult<List<RewardDto>>.FailureResult(ex.Message);
            }
        }

        public async Task<OperationResult<RewardDto>> GetRewardByIdAsync(string rewardId)
        {
            try
            {
                var rewardIdObj = new RewardId(Guid.Parse(rewardId));
                var reward = await _programRepository.GetRewardByIdAsync(rewardIdObj);
                
                if (reward == null)
                    return OperationResult<RewardDto>.FailureResult($"Reward with ID {rewardId} not found");
                
                return OperationResult<RewardDto>.SuccessResult(MapToRewardDto(reward));
            }
            catch (Exception ex)
            {
                return OperationResult<RewardDto>.FailureResult($"Failed to get reward: {ex.Message}");
            }
        }

        public async Task<OperationResult<RewardDto>> CreateRewardAsync(string programId, CreateRewardDto dto)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                
                var programIdObj = new LoyaltyProgramId(Guid.Parse(programId));
                var program = await _programRepository.GetByIdAsync(programIdObj);
                
                if (program == null)
                    return OperationResult<RewardDto>.FailureResult("Program not found");
                
                var reward = program.CreateReward(
                    dto.Title,
                    dto.Description,
                    dto.RequiredPoints > 0 ? dto.RequiredPoints : dto.RequiredStamps,
                    dto.StartDate,
                    dto.EndDate
                );
                
                await _programRepository.AddRewardAsync(reward);
                await _unitOfWork.CommitTransactionAsync();
                
                return OperationResult<RewardDto>.SuccessResult(MapToRewardDto(reward));
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return OperationResult<RewardDto>.FailureResult(ex.Message);
            }
        }

        private LoyaltyProgramDto MapToDto(LoyaltyProgram program)
        {
            return new LoyaltyProgramDto
            {
                Id = program.Id.ToString(),
                BrandId = program.BrandId.ToString(),
                Name = program.Name,
                Type = program.Type.ToString(),
                StampThreshold = program.StampThreshold ?? 0,
                PointsConversionRate = program.PointsConversionRate ?? 0m,
                DailyStampLimit = program.DailyStampLimit ?? 0,
                MinimumTransactionAmount = program.MinimumTransactionAmount ?? 0m,
                IsActive = program.IsActive,
                CreatedAt = program.CreatedAt,
                UpdatedAt = program.UpdatedAt
            };
        }

        private RewardDto MapToRewardDto(Reward reward)
        {
            return new RewardDto
            {
                Id = reward.Id.ToString(),
                ProgramId = reward.ProgramId.ToString(),
                Title = reward.Title,
                Description = reward.Description,
                RequiredPoints = reward.RequiredValue,
                RequiredStamps = reward.RequiredValue,
                StartDate = reward.ValidFrom,
                EndDate = reward.ValidTo,
                IsActive = reward.IsActive,
                CreatedAt = reward.CreatedAt
            };
        }

        public async Task<OperationResult<ProgramAnalyticsDto>> GetProgramAnalyticsAsync()
        {
            try
            {
                // Get all programs
                var programs = await _programRepository.GetAllAsync(1, 1000);
                
                // Calculate analytics
                int totalPrograms = programs.Count;
                int activePrograms = programs.Count(p => p.IsActive);
                int stampPrograms = programs.Count(p => p.Type == LoyaltyProgramType.Stamp);
                int pointsPrograms = programs.Count(p => p.Type == LoyaltyProgramType.Points);
                
                // Get all rewards
                var rewards = new List<Reward>();
                foreach (var program in programs)
                {
                    var programRewards = await _programRepository.GetRewardsForProgramAsync(program.Id);
                    rewards.AddRange(programRewards);
                }
                
                int totalRewards = rewards.Count;
                int activeRewards = rewards.Count(r => r.IsActive);
                
                var analytics = new ProgramAnalyticsDto
                {
                    TotalPrograms = totalPrograms,
                    ActivePrograms = activePrograms,
                    StampPrograms = stampPrograms,
                    PointsPrograms = pointsPrograms,
                    TotalRewards = totalRewards,
                    ActiveRewards = activeRewards,
                    ProgramsByBrand = await GetProgramCountByBrandAsync()
                };
                
                return OperationResult<ProgramAnalyticsDto>.SuccessResult(analytics);
            }
            catch (Exception ex)
            {
                return OperationResult<ProgramAnalyticsDto>.FailureResult($"Failed to get program analytics: {ex.Message}");
            }
        }
        
        private async Task<Dictionary<string, int>> GetProgramCountByBrandAsync()
        {
            var result = new Dictionary<string, int>();
            var brands = await _brandRepository.GetAllAsync(1, 1000);
            
            foreach (var brand in brands)
            {
                var programs = await _programRepository.GetByBrandIdAsync(brand.Id);
                result[brand.Name] = programs.Count();
            }
            
            return result;
        }
        
        public async Task<OperationResult<ProgramDetailedAnalyticsDto>> GetProgramDetailedAnalyticsAsync(string programId)
        {
            try
            {
                var programIdObj = new LoyaltyProgramId(Guid.Parse(programId));
                var program = await _programRepository.GetByIdAsync(programIdObj);
                
                if (program == null)
                    return OperationResult<ProgramDetailedAnalyticsDto>.FailureResult($"Program with ID {programId} not found");
                
                // Get all rewards for this program
                var rewards = await _programRepository.GetRewardsForProgramAsync(programIdObj);
                
                // Get all cards for this program
                var cards = await _unitOfWork.LoyaltyCardRepository.GetByProgramIdAsync(programIdObj);
                
                // Calculate analytics
                int totalCards = cards.Count();
                int activeCards = cards.Count(c => c.Status == CardStatus.Active);
                int suspendedCards = cards.Count(c => c.Status == CardStatus.Suspended);
                int expiredCards = cards.Count(c => c.Status == CardStatus.Expired);
                
                // Get all transactions for this program's cards
                var transactions = new List<Transaction>();
                foreach (var card in cards)
                {
                    var cardTransactions = await _unitOfWork.TransactionRepository.GetByCardIdAsync(card.Id);
                    transactions.AddRange(cardTransactions);
                }
                
                int totalTransactions = transactions.Count;
                decimal totalPointsIssued = transactions.Where(t => t.Type == TransactionType.PointsIssued).Sum(t => t.Points ?? 0);
                decimal totalPointsRedeemed = transactions.Where(t => t.Type == TransactionType.PointsRedeemed).Sum(t => t.Points ?? 0);
                int totalStampsIssued = transactions.Where(t => t.Type == TransactionType.StampIssued).Sum(t => t.Stamps ?? 0);
                int totalStampsRedeemed = transactions.Where(t => t.Type == TransactionType.StampRedeemed).Sum(t => t.Stamps ?? 0);
                int totalRedemptions = transactions.Count(t => t.Type == TransactionType.RewardRedeemed);
                
                var analytics = new ProgramDetailedAnalyticsDto
                {
                    ProgramId = program.Id.ToString(),
                    ProgramName = program.Name,
                    ProgramType = program.Type.ToString(),
                    IsActive = program.IsActive,
                    TotalCards = totalCards,
                    ActiveCards = activeCards,
                    SuspendedCards = suspendedCards,
                    ExpiredCards = expiredCards,
                    TotalRewards = rewards.Count(),
                    ActiveRewards = rewards.Count(r => r.IsActive),
                    TotalTransactions = totalTransactions,
                    TotalPointsIssued = totalPointsIssued,
                    TotalPointsRedeemed = totalPointsRedeemed,
                    TotalStampsIssued = totalStampsIssued,
                    TotalStampsRedeemed = totalStampsRedeemed,
                    TotalRedemptions = totalRedemptions
                };
                
                return OperationResult<ProgramDetailedAnalyticsDto>.SuccessResult(analytics);
            }
            catch (Exception ex)
            {
                return OperationResult<ProgramDetailedAnalyticsDto>.FailureResult($"Failed to get detailed program analytics: {ex.Message}");
            }
        }
    }

    public class LoyaltyProgramDto
    {
        public string Id { get; set; } = string.Empty;
        public string BrandId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int StampThreshold { get; set; }
        public decimal PointsConversionRate { get; set; }
        public int DailyStampLimit { get; set; }
        public decimal MinimumTransactionAmount { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class ExpirationPolicyDto
    {
        public string Type { get; set; } = string.Empty;
        public DateTime? ExpirationDate { get; set; }
        public int? PeriodMonths { get; set; }
    }

    public class CreateLoyaltyProgramDto
    {
        public string BrandId { get; set; }
        public string Name { get; set; }
        public LoyaltyProgramType Type { get; set; }
        public int? StampThreshold { get; set; }
        public decimal? PointsConversionRate { get; set; }
        public int? DailyStampLimit { get; set; }
        public decimal? MinimumTransactionAmount { get; set; }
        
        // Expiration settings
        public bool HasExpiration { get; set; }
        public bool ExpiresOnSpecificDate { get; set; }
        public int? ExpirationType { get; set; }
        public int? ExpirationValue { get; set; }
        public int? ExpirationDay { get; set; }
        public int? ExpirationMonth { get; set; }
    }

    public class UpdateLoyaltyProgramDto
    {
        public string Name { get; set; }
        public int? StampThreshold { get; set; }
        public decimal? PointsConversionRate { get; set; }
        public int? DailyStampLimit { get; set; }
        public decimal? MinimumTransactionAmount { get; set; }
        
        // Expiration settings
        public bool HasExpiration { get; set; }
        public bool ExpiresOnSpecificDate { get; set; }
        public int? ExpirationType { get; set; }
        public int? ExpirationValue { get; set; }
        public int? ExpirationDay { get; set; }
        public int? ExpirationMonth { get; set; }
    }

    public class RewardDto
    {
        public string Id { get; set; }
        public string ProgramId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int RequiredPoints { get; set; }
        public int RequiredStamps { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateRewardDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int RequiredPoints { get; set; }
        public int RequiredStamps { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class UpdateRewardDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int RequiredPoints { get; set; }
        public int RequiredStamps { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class ProgramAnalyticsDto
    {
        public int TotalPrograms { get; set; }
        public int ActivePrograms { get; set; }
        public int StampPrograms { get; set; }
        public int PointsPrograms { get; set; }
        public int TotalRewards { get; set; }
        public int ActiveRewards { get; set; }
        public Dictionary<string, int> ProgramsByBrand { get; set; } = new Dictionary<string, int>();
    }
    
    public class ProgramDetailedAnalyticsDto
    {
        public string ProgramId { get; set; } = string.Empty;
        public string ProgramName { get; set; } = string.Empty;
        public string ProgramType { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int TotalCards { get; set; }
        public int ActiveCards { get; set; }
        public int SuspendedCards { get; set; }
        public int ExpiredCards { get; set; }
        public int TotalRewards { get; set; }
        public int ActiveRewards { get; set; }
        public int TotalTransactions { get; set; }
        public decimal TotalPointsIssued { get; set; }
        public decimal TotalPointsRedeemed { get; set; }
        public int TotalStampsIssued { get; set; }
        public int TotalStampsRedeemed { get; set; }
        public int TotalRedemptions { get; set; }
    }
} 