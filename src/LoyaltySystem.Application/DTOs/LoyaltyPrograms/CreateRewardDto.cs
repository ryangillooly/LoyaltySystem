using System;
using System.ComponentModel.DataAnnotations;

namespace LoyaltySystem.Application.DTOs.LoyaltyPrograms
{
    public class CreateRewardDto
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Title { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        [Range(0, int.MaxValue)]
        public int RequiredPoints { get; set; }

        [Range(0, int.MaxValue)]
        public int RequiredStamps { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? StartDate { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        public bool Validate()
        {
            if (RequiredPoints <= 0 && RequiredStamps <= 0)
            {
                throw new ValidationException("Either RequiredPoints or RequiredStamps must be greater than 0");
            }

            if (RequiredPoints > 0 && RequiredStamps > 0)
            {
                throw new ValidationException("Cannot require both points and stamps - only one should be set");
            }

            if (StartDate.HasValue && EndDate.HasValue && StartDate.Value > EndDate.Value)
            {
                throw new ValidationException("Start date must be before end date");
            }

            return true;
        }
    }
} 