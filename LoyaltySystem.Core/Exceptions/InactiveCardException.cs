namespace LoyaltySystem.Core.Exceptions;
public class InactiveCardException : LoyaltyCardExceptionBase
{
    public InactiveCardException(Guid userId, Guid businessId)
        : base(userId, businessId, $"The loyalty card for user {userId}, for business {businessId}, is inactive and cannot be stamped.") { }
}
