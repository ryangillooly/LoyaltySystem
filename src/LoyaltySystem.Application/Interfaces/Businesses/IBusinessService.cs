using LoyaltySystem.Application.Common;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Domain.Common;

namespace LoyaltySystem.Application.Interfaces;

public interface IBusinessService 
{
    Task<OperationResult<BusinessDto>> GetBusinessByIdAsync(string id);
    Task<OperationResult<PagedResult<BusinessDto>>> GetAllBusinessesAsync(int skip, int limit);
    Task<OperationResult<BusinessDto>> CreateBusinessAsync(CreateBusinessDto dto);
    Task<OperationResult<BusinessDto>> UpdateBusinessAsync(string id, UpdateBusinessDto dto);
    Task<OperationResult<bool>> DeleteBusinessAsync(string id);
    Task<OperationResult<BusinessDto>> GetBusinessDetailAsync(string id);
    Task<OperationResult<PagedResult<BusinessSummaryDto>>> SearchBusinessesByNameAsync(string nameQuery, int page, int pageSize);

}    