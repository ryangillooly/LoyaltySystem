using System.Security.Claims;
using System.Threading.Tasks;
using LoyaltySystem.Domain.Common;

namespace LoyaltySystem.Application.Interfaces
{
    public interface IStaffAuthorizationService
    {
        Task<bool> IsAuthorizedForStore(ClaimsPrincipal user, StoreId storeId);
        Task<StoreId?> GetAssignedStoreId(ClaimsPrincipal user);
    }
} 