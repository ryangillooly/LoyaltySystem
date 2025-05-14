using LoyaltySystem.Application.Common;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Domain.Common;

namespace LoyaltySystem.Application.Interfaces;

public interface IBrandService 
{
    Task<OperationResult<BrandDto>> GetBrandByIdAsync(string id);
    Task<OperationResult<PagedResult<BrandDto>>> GetAllBrandsAsync(int skip, int limit);

    Task<OperationResult<BrandDto>> CreateBrandAsync(CreateBrandDto dto);

    Task<OperationResult<BrandDto>> UpdateBrandAsync(string id, UpdateBrandDto dto);
        
}
