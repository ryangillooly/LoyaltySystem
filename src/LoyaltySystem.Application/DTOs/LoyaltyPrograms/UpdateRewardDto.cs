using System;

namespace LoyaltySystem.Application.DTOs.LoyaltyPrograms
{
    public class UpdateRewardDto
    {
        public string Id { get; set; }
        public string ProgramId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int RequiredPoints { get; set; }
        public int RequiredStamps { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; } = true;
    }
} 