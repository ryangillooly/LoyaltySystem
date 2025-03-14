using System.Security.Claims;
using System.Threading.Tasks;
using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace LoyaltySystem.Application.Services;

public class StaffAuthorizationService : IStaffAuthorizationService
{
    private readonly IStoreRepository _storeRepository;
    private readonly ILogger<StaffAuthorizationService> _logger;

    public StaffAuthorizationService(
        IStoreRepository storeRepository,
        ILogger<StaffAuthorizationService> logger)
    {
        _storeRepository = storeRepository;
        _logger = logger;
    }

    public async Task<bool> IsAuthorizedForStore(ClaimsPrincipal user, StoreId storeId)
    {
        // Admin and SuperAdmin have access to all stores
        if (user.IsInRole("Admin") || user.IsInRole("SuperAdmin"))
            return true;

        // Get the store ID from the user's claims
        var assignedStoreId = await GetAssignedStoreId(user);
        if (assignedStoreId == null)
        {
            _logger.LogWarning("Staff user has no assigned store");
            return false;
        }

        // Check if the assigned store matches the requested store
        if (assignedStoreId.Value != storeId)
        {
            _logger.LogWarning("Staff user assigned to store {AssignedStore} attempted to access store {RequestedStore}",
                assignedStoreId, storeId);
            return false;
        }

        return true;
    }

    public async Task<StoreId?> GetAssignedStoreId(ClaimsPrincipal user)
    {
        var storeIdClaim = user.FindFirst("StoreId")?.Value;
        if (string.IsNullOrEmpty(storeIdClaim))
        {
            _logger.LogWarning("No store ID found in user claims");
            return null;
        }

        if (!StoreId.TryParse<StoreId>(storeIdClaim, out var storeId))
        {
            _logger.LogWarning("Invalid store ID format in claims: {StoreId}", storeIdClaim);
            return null;
        }

        // Verify the store exists
        var store = await _storeRepository.GetByIdAsync(storeId.ToString());
        if (store == null)
        {
            _logger.LogWarning("Store not found for ID: {StoreId}", storeId);
            return null;
        }

        return storeId;
    }
} 