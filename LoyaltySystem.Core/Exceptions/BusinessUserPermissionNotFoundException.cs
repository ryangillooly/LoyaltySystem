namespace LoyaltySystem.Core.Exceptions;

public class BusinessUserPermissionNotFoundException : LoyaltyCardExceptionBase
{
    public BusinessUserPermissionNotFoundException(Guid userId, Guid businessId)
        : base(userId, businessId, $"The permission for user {userId} to access business {businessId} was not found.") { }
}