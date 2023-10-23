namespace LoyaltySystem.Core.Exceptions;

public class EmailExceptions
{
    public abstract class EmailExceptionBase : Exception
    {
        public Guid UserId { get; set; }
        public Guid Token { get; }

        protected EmailExceptionBase(Guid businessId, string message)
            : base(message) => (BusinessId) = (businessId);
    }
    
    public class BusinessNotFoundException : EmailExceptionBase
    {
        public BusinessNotFoundException(Guid businessId)
            : base(businessId, $"The Business {businessId} was not found.") { }
    }
}