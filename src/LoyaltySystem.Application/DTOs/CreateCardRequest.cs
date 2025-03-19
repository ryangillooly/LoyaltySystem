using LoyaltySystem.Domain.Common;

namespace LoyaltySystem.Application.DTOs
{
    /// <summary>
    /// Request model for creating a loyalty card.
    /// </summary>
    public class CreateCardRequest
    {
        /// <summary>
        /// The customer for whom to create the card.
        /// </summary>
        public CustomerId? CustomerId { get; set; }
        
        /// <summary>
        /// The loyalty program to enroll in.
        /// </summary>
        public LoyaltyProgramId? ProgramId { get; set; }
    }
} 