using LoyaltySystem.Core.Models;

namespace LoyaltySystem.Core.Exceptions;

public class EmailExceptions
{
    public abstract class EmailExceptionBase : Exception
    {
        public Guid UserId { get; set; }
        public Guid Token { get; }

        protected EmailExceptionBase(Guid userId, Guid token, string message)
            : base(message) => (UserId, Token) = (userId, token);
    }
    
    public class VerificationEmailExpiredException : EmailExceptionBase
    {
        public VerificationEmailExpiredException(Guid userId, Guid token)
            : base(userId, token, $"The Verification Email {token} for user {userId} has expired") { }
    }

    public class NoVerificationEmailFoundException : EmailExceptionBase
    {
        public NoVerificationEmailFoundException(Guid userId, Guid token)
            : base(userId, token, $"No Verification Email {token} was found for user {userId}") { }
    }

    public class VerificationEmailAlreadyVerifiedException : EmailExceptionBase
    {
        public VerificationEmailAlreadyVerifiedException(Guid userId, Guid token)
            : base(userId, token, $"The Verification Email {token} for user {userId} has already been verified") { }
    }
}