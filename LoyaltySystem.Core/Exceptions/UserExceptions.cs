using LoyaltySystem.Core.Enums;

namespace LoyaltySystem.Core.Exceptions;

public class UserExceptions
{
    public abstract class UserExceptionsBase : Exception
    {
        public Guid UserId { get; set; }
        public UserStatus UserStatus { get; set; }

        protected UserExceptionsBase(Guid userId, string message)
            : base(message) => (UserId) = (userId);
        
        protected UserExceptionsBase(Guid userId, UserStatus userStatus, string message)
            : base(message) => (UserId, UserStatus) = (userId, userStatus);
    }
    
    public class UserNotFoundException : UserExceptionsBase
    {
        public UserNotFoundException(Guid userId)
            : base(userId, $"The User {userId} was not found.") { }
    }
    
    public class UserNotActiveException : UserExceptionsBase
    {
        public UserNotActiveException(Guid userId, UserStatus userStatus)
            : base(userId, userStatus, $"User {userId} is currently not in Active status. The status is {userStatus}") { }
    }
    
    
}