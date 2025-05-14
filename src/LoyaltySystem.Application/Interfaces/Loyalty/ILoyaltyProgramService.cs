using System.Collections.Generic;
using System.Threading.Tasks;
using LoyaltySystem.Application.Common;
using LoyaltySystem.Application.DTOs.LoyaltyPrograms;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Entities;

namespace LoyaltySystem.Application.Interfaces
{
    public interface ILoyaltyProgramService 
    {
        Task<OperationResult<LoyaltyProgramDto>> GetProgramByIdAsync(string id);
        Task<OperationResult<PagedResult<LoyaltyProgramDto>>> GetAllProgramsAsync(string brandId, int skip, int limit);
        Task<OperationResult<List<LoyaltyProgramDto>>> GetProgramsByBrandIdAsync(string brandId);
        Task<OperationResult<PagedResult<LoyaltyProgramDto>>> GetProgramsByTypeAsync(string type, int skip, int limit);
        Task<OperationResult<LoyaltyProgramDto>> CreateProgramAsync(CreateLoyaltyProgramDto dto);
        Task<OperationResult<LoyaltyProgramDto>> UpdateProgramAsync(string id, UpdateLoyaltyProgramDto dto);
        Task<OperationResult<bool>> DeleteProgramAsync(string id);
        Task<OperationResult<PagedResult<LoyaltyProgramDto>>> GetNearbyProgramsAsync(double latitude, double longitude, double radiusKm, int page, int pageSize);
    }
} 