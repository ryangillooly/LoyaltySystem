namespace LoyaltySystem.Core.Exceptions;

public class CardNotFoundException : LoyaltyCardExceptionBase
{
    public CardNotFoundException(Guid userId, Guid businessId)
        : base(userId, businessId, $"The loyalty card for user {userId}, for business {businessId}, was not found.") { }
}