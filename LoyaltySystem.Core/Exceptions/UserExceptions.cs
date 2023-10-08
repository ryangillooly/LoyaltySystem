namespace LoyaltySystem.Core.Exceptions;

public class UserExceptions
{
    public abstract class UserExceptionsBase : Exception
    {
        public Guid UserId { get; set; }

        protected UserExceptionsBase(Guid userId, string message)
            : base(message) => (UserId) = (userId);
    }
    
    public class UserNotFoundException : UserExceptionsBase
    {
        public UserNotFoundException(Guid userId)
            : base(userId, $"The User {userId} was not found.") { }
    }
}