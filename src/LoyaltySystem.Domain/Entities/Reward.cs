using LoyaltySystem.Domain.Common;
using System;

namespace LoyaltySystem.Domain.Entities
{
    /// <summary>
    /// Represents a reward that customers can earn with their loyalty.
    /// </summary>
    public class Reward
    {
        public RewardId Id { get; set; }
        public LoyaltyProgramId ProgramId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int RequiredValue { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation property
        public virtual LoyaltyProgram Program { get; set; }
        
        public Reward() { }

        public Reward
            (LoyaltyProgramId programId,
            string title,
            string description,
            int requiredValue,
            DateTime? validFrom = null,
            DateTime? validTo = null)
        {
            if (programId == Guid.Empty)
                throw new ArgumentException("Program ID cannot be empty", nameof(programId));

            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Reward title cannot be empty", nameof(title));

            if (requiredValue <= 0)
                throw new ArgumentException("Required value must be greater than zero", nameof(requiredValue));

            // Validate date range if both are provided
            if (validFrom.HasValue && validTo.HasValue && validFrom.Value > validTo.Value)
                throw new ArgumentException("Valid from date must be before valid to date");

            Id = new RewardId();
            ProgramId = programId;
            Title = title;
            Description = description;
            RequiredValue = requiredValue;
            ValidFrom = validFrom;
            ValidTo = validTo;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the reward properties.
        /// </summary>
        public void Update(
            string title,
            string description,
            int requiredValue,
            DateTime? validFrom = null,
            DateTime? validTo = null)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Reward title cannot be empty", nameof(title));

            if (requiredValue <= 0)
                throw new ArgumentException("Required value must be greater than zero", nameof(requiredValue));

            // Validate date range if both are provided
            if (validFrom.HasValue && validTo.HasValue && validFrom.Value > validTo.Value)
                throw new ArgumentException("Valid from date must be before valid to date");

            Title = title;
            Description = description;
            RequiredValue = requiredValue;
            ValidFrom = validFrom;
            ValidTo = validTo;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Activates the reward, making it available for redemption.
        /// </summary>
        public void Activate()
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Deactivates the reward, making it unavailable for redemption.
        /// </summary>
        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Checks if the reward is valid at a specific point in time.
        /// </summary>
        public bool IsValidAt(DateTime date)
        {
            if (!IsActive)
                return false;

            // Check validity period
            if (ValidFrom.HasValue && date < ValidFrom.Value)
                return false;

            if (ValidTo.HasValue && date > ValidTo.Value)
                return false;

            return true;
        }
    }
} 