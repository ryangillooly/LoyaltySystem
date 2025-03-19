using LoyaltySystem.Application.Common;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.DTOs.LoyaltyPrograms;
using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Repositories;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;
using LoyaltySystem.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LoyaltySystem.Application.Services;
public class LoyaltyProgramService : ILoyaltyProgramService
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

            return program is null 
                ? OperationResult<LoyaltyProgramDto>.FailureResult($"Loyalty program with ID {id} not found") 
                : OperationResult<LoyaltyProgramDto>.SuccessResult(MapToDto(program));
        }
        catch (Exception ex)
        {
            return OperationResult<LoyaltyProgramDto>.FailureResult($"Failed to get loyalty program: {ex.Message}");
        }
    }

    public async Task<OperationResult<PagedResult<LoyaltyProgramDto>>> GetAllProgramsAsync(string brandId, int skip, int limit)
    {
        try
        {
            List<LoyaltyProgram> programs;
            
            if (string.IsNullOrEmpty(brandId))
            {
                // If no brand ID is specified, get all programs with pagination to avoid loading everything at once
                programs = (List<LoyaltyProgram>) await _programRepository.GetAllAsync(skip, limit);
            }
            else
            {
                // For brand-specific filtering, we still need to get all for that brand
                // as the repository might not have a paginated version for brand filtering
                var brandIdObj = EntityId.Parse<BrandId>(brandId);
                programs = (List<LoyaltyProgram>) await _programRepository.GetByBrandIdAsync(brandIdObj);
                
                // Apply pagination manually if needed
                if (programs.Count > limit)
                    programs = programs.Skip(skip).Take(limit).ToList();
            }
            
            var programDtos = programs.Select(MapToDto).ToList();

            // For accurate total count, we would need a separate count query
            // For now, use approximate counting based on what we have
            int totalCount = string.IsNullOrEmpty(brandId) 
                ? programs.Count * limit // Rough estimate for all programs TODO: Not sure this is right
                : programs.Count; // For brand filtering, we have the actual count
            
            var result = new PagedResult<LoyaltyProgramDto>(programDtos, totalCount, skip, limit);

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

    public async Task<OperationResult<PagedResult<LoyaltyProgramDto>>> GetProgramsByTypeAsync(string type, int skip, int limit)
    {
        try
        {
            if (!Enum.TryParse<LoyaltyProgramType>(type, true, out var programType))
            {
                return OperationResult<PagedResult<LoyaltyProgramDto>>.FailureResult($"Invalid program type: {type}");
            }
            
            var programs = await _programRepository.GetByTypeAsync(programType, skip, limit);
            var totalCount = await _programRepository.GetCountByTypeAsync(programType);
            
            var programDtos = programs.Select(MapToDto).ToList();
            var result = new PagedResult<LoyaltyProgramDto>(programDtos, totalCount, skip, limit);
            
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
            // Create expiration policy if provided
            ExpirationPolicy expirationPolicy = null;
            if (dto.ExpirationPolicy != null)
            {
                expirationPolicy = new ExpirationPolicy
                {                    
                    ExpirationDay = dto.ExpirationPolicy.ExpirationDay,
                    HasExpiration = dto.ExpirationPolicy.HasExpiration,
                    ExpirationType = dto.ExpirationPolicy.ExpirationType,
                    ExpirationValue = dto.ExpirationPolicy.ExpirationValue,
                    ExpirationMonth = dto.ExpirationPolicy.ExpirationMonth,
                    ExpiresOnSpecificDate = dto.ExpirationPolicy.ExpiresOnSpecificDate
                };
            }

            // Create points config if provided
            PointsConfig pointsConfig = null;
            if (dto.PointsConfig != null)
            {
                pointsConfig = dto.PointsConfig.ToPointsConfig();
            }

            var program = new LoyaltyProgram
            {
                Id = new LoyaltyProgramId(),
                BrandId = EntityId.Parse<BrandId>(dto.BrandId),
                Name = dto.Name,
                Description = dto.Description,
                Type = dto.Type,
                ExpirationPolicy = dto.ExpirationPolicy.ToExpirationPolicy(),
                StampThreshold = dto.StampThreshold,
                PointsConversionRate = dto.PointsConversionRate,
                PointsConfig = dto.PointsConfig.ToPointsConfig(),
                DailyStampLimit = dto.DailyStampLimit,
                MinimumTransactionAmount = dto.MinimumTransactionAmount,
                IsActive = dto.IsActive,
                HasTiers = dto.HasTiers,
                TermsAndConditions = dto.TermsAndConditions,
                EnrollmentBonusPoints = dto.EnrollmentBonusPoints,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Add tiers if the program supports them
            if (dto is { HasTiers: true, Tiers: { } } && dto.Tiers.Any())
            {
                foreach (var tierDto in dto.Tiers)
                {
                    var tier = new LoyaltyTier(
                        program.Id,
                        tierDto.Name,
                        tierDto.PointThreshold,
                        tierDto.PointMultiplier,
                        tierDto.TierOrder
                    );
                    
                    if (tierDto.Benefits != null)
                    {
                        foreach (var benefit in tierDto.Benefits)
                        {
                            tier.AddBenefit(benefit);
                        }
                    }
                    
                    program.AddTier(tier);
                }
            }

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                await _programRepository.AddAsync(program, _unitOfWork.CurrentTransaction);
            });

            return OperationResult<LoyaltyProgramDto>.SuccessResult(MapToDto(program));
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return OperationResult<LoyaltyProgramDto>.FailureResult($"Failed to create loyalty program: {ex.Message}");
        }
    }

    public async Task<OperationResult<LoyaltyProgramDto>> UpdateProgramAsync(string id, UpdateLoyaltyProgramDto dto)
    {
        try
        {
            var programId = EntityId.Parse<LoyaltyProgramId>(id);
            var program = await _programRepository.GetByIdAsync(programId);

            if (program == null)
            {
                return OperationResult<LoyaltyProgramDto>.FailureResult($"Loyalty program with ID {id} not found");
            }

            // Create expiration policy if provided
            ExpirationPolicy expirationPolicy = null;
            if (dto.ExpirationPolicy != null)
            {
                if (dto.ExpirationPolicy.ExpiresOnSpecificDate && dto.ExpirationPolicy.ExpirationDay.HasValue)
                {
                    // Create date-based expiration policy
                    expirationPolicy = new ExpirationPolicy(
                        dto.ExpirationPolicy.ExpirationDay.Value,
                        dto.ExpirationPolicy.ExpirationMonth
                    );
                }
                else if (dto.ExpirationPolicy.HasExpiration && dto.ExpirationPolicy.ExpirationValue > 0)
                {
                    // Create period-based expiration policy
                    expirationPolicy = new ExpirationPolicy(
                        dto.ExpirationPolicy.ExpirationType,
                        dto.ExpirationPolicy.ExpirationValue
                    );
                }
                else
                {
                    // Create default policy with no expiration
                    expirationPolicy = new ExpirationPolicy();
                }
            }

            // Create points config if provided
            PointsConfig pointsConfig = null;
            if (dto.PointsConfig != null)
            {
                pointsConfig = dto.PointsConfig.ToPointsConfig();
            }

            // Update the program with all properties
            program.Update(
                dto.Name,
                dto.StampThreshold,
                dto.PointsConversionRate,
                pointsConfig,
                dto.HasTiers,
                dto.DailyStampLimit,
                dto.MinimumTransactionAmount,
                expirationPolicy,
                dto.Description,
                dto.TermsAndConditions,
                dto.EnrollmentBonusPoints,
                dto.StartDate,
                dto.EndDate
            );
            
            // Handle IsActive separately if needed
            if (program.IsActive != dto.IsActive)
            {
                // Using reflection as a workaround since IsActive setter is private
                typeof(LoyaltyProgram).GetProperty("IsActive").SetValue(program, dto.IsActive);
            }

            await _unitOfWork.BeginTransactionAsync();
            await _programRepository.UpdateAsync(program);
            await _unitOfWork.CommitTransactionAsync();

            return OperationResult<LoyaltyProgramDto>.SuccessResult(MapToDto(program));
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return OperationResult<LoyaltyProgramDto>.FailureResult($"Failed to update loyalty program: {ex.Message}");
        }
    }
    
    private LoyaltyProgramDto MapToDto(LoyaltyProgram program)
    {
        if (program == null) return null;
        
        var rewards = program.Rewards?.Select(MapToRewardDto).ToList() ?? new List<RewardDto>();
        
        var tiers = program.Tiers?.Select(t => new TierDto
        {
            Id = t.Id.ToString(),
            Name = t.Name,
            PointThreshold = t.PointThreshold,
            PointMultiplier = t.PointMultiplier,
            TierOrder = t.TierOrder,
            Benefits = t.Benefits?.ToList() ?? new List<string>()
        }).ToList() ?? new List<TierDto>();
        
        return new LoyaltyProgramDto
        {
            Id = program.Id.ToString(),
            BrandId = program.BrandId.ToString(),
            Name = program.Name,
            Description = program.Description,
            Type = program.Type,
            ExpirationPolicy = program.ExpirationPolicy is { } 
                ? new ExpirationPolicyDto
                {
                    ExpirationType = program.ExpirationPolicy.ExpirationType,
                    ExpirationValue = program.ExpirationPolicy.ExpirationValue,
                    ExpirationMonth = program.ExpirationPolicy.ExpirationMonth,
                    ExpirationDay = program.ExpirationPolicy.ExpirationDay,
                    HasExpiration = program.ExpirationPolicy.HasExpiration,
                    ExpiresOnSpecificDate = program.ExpirationPolicy.ExpiresOnSpecificDate
                } 
                : null,
            PointsConfig = program.PointsConfig is { } 
                ? new PointsConfigDto(program.PointsConfig)
                : null,
            TermsAndConditions = program.TermsAndConditions,
            HasTiers = program.HasTiers,
            EnrollmentBonusPoints = program.EnrollmentBonusPoints,
            StartDate = program.StartDate,
            EndDate = program.EndDate,
            IsActive = program.IsActive,
            Rewards = rewards,
            Tiers = tiers
        };
    }

    private RewardDto MapToRewardDto(Reward reward)
    {
        if (reward == null) return null;
        
        return new RewardDto
        {
            Id = reward.Id.ToString(),
            ProgramId = reward.ProgramId.ToString(),
            Title = reward.Title,
            Description = reward.Description,
            RequiredPoints = reward.RequiredValue,
            StartDate = reward.ValidFrom,
            EndDate = reward.ValidTo
        };
    }

    public async Task<OperationResult<PagedResult<LoyaltyProgramDto>>> GetNearbyProgramsAsync(double latitude, double longitude, double radiusKm, int page, int pageSize)
    {
        try
        {
            // Calculate proper pagination parameters
            int skip = (page - 1) * pageSize;
            
            // Use the paginated version of GetAllAsync to avoid the type casting issue
            var programs = await _programRepository.GetAllAsync(skip, pageSize);
            var programDtos = new List<LoyaltyProgramDto>();

            // TODO: In a real implementation, you would:
            // 1. Check if each program's brand has a store with coordinates
            // 2. Calculate distance between user location and store
            // 3. Filter by those within the radius
            // For now, we'll filter to just active programs
            
            foreach (var program in programs)
            {
                // Only include active programs 
                if (program.IsActive)
                {
                    programDtos.Add(MapToDto(program));
                }
            }

            // For accurate total count, we would need a separate count query
            // For now, use approximate counting based on what we have
            int totalCount = programDtos.Count * (page + 1); // Rough estimate for all programs
            
            var result = new PagedResult<LoyaltyProgramDto>(programDtos, totalCount, page, pageSize);

            return OperationResult<PagedResult<LoyaltyProgramDto>>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            return OperationResult<PagedResult<LoyaltyProgramDto>>.FailureResult($"Failed to get nearby loyalty programs: {ex.Message}");
        }
    }

    public async Task<OperationResult<bool>> DeleteProgramAsync(string id)
    {
        try
        {
            var programId = EntityId.Parse<LoyaltyProgramId>(id);
            var program = await _programRepository.GetByIdAsync(programId);

            if (program == null)
            {
                return OperationResult<bool>.FailureResult($"Loyalty program with ID {id} not found");
            }

            // Since there's no DeleteAsync method, we'll mark it as inactive instead
            // Using reflection as a workaround since IsActive setter is private
            typeof(LoyaltyProgram).GetProperty("IsActive").SetValue(program, false);
            
            await _unitOfWork.BeginTransactionAsync();
            await _programRepository.UpdateAsync(program);
            await _unitOfWork.CommitTransactionAsync();

            return OperationResult<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return OperationResult<bool>.FailureResult($"Failed to delete loyalty program: {ex.Message}");
        }
    }
}
