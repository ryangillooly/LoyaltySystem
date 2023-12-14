using Amazon.DynamoDBv2.Model;
using LoyaltySystem.Core.Models;

namespace LoyaltySystem.Core.Exceptions;

public class EmailExceptions
{
    public abstract class EmailExceptionBase : Exception
    {
        public Guid UserId { get; set; } = Guid.Empty;
        public Guid Token { get; } = Guid.Empty;
        public string Email { get; set; } = string.Empty;

        protected EmailExceptionBase(Guid userId, Guid token, string message)
            : base(message) => (UserId, Token) = (userId, token);
        
        protected EmailExceptionBase(string email, string message)
            : base(message) => Email = email;
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
    
    public class EmailAlreadyExistsException : EmailExceptionBase
    {
        public EmailAlreadyExistsException(string email)
            : base(email, $"Email {email} already exists") { } 
    }
}