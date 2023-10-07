namespace LoyaltySystem.Core.Exceptions;

public abstract class LoyaltyCardExceptionBase : Exception
{
    public Guid UserId { get; }
    public Guid BusinessId { get; }

    protected LoyaltyCardExceptionBase(Guid userId, Guid businessId, string message)
        : base(message) =>
        (UserId, BusinessId) = (userId, businessId);
}