using LoyaltySystem.Domain.Common;
using System.Security.Claims;

namespace LoyaltySystem.Shared.API.Extensions;

public static class ClaimsPrincipalExtensions 
{
    public static bool TryGetUserId(this ClaimsPrincipal principal, out UserId? userId)
    {
        userId = null;
        var userIdString = principal.FindFirstValue("UserId");
        
        return 
            !string.IsNullOrEmpty(userIdString) && 
            EntityId.TryParse<UserId>(userIdString, out userId);
    }
}
