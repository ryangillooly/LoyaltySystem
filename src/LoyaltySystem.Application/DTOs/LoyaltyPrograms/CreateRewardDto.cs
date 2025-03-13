using System;

namespace LoyaltySystem.Application.DTOs.LoyaltyPrograms
{
    public class CreateRewardDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int RequiredPoints { get; set; }
        public int RequiredStamps { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; } = true;
    }
} 