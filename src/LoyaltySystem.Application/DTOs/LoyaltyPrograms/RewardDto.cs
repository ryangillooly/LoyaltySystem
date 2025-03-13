using System;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Common;

namespace LoyaltySystem.Application.DTOs.LoyaltyPrograms
{
    public class RewardDto
    {
        public string Id { get; set; }
        public string ProgramId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int RequiredPoints { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        public RewardDto()
        {
            // Default constructor for serialization
        }
        
        public RewardDto(Reward reward)
        {
            if (reward == null) return;
            
            Id = reward.Id.ToString();
            ProgramId = reward.ProgramId.ToString();
            Title = reward.Title;
            Description = reward.Description;
            RequiredPoints = reward.RequiredValue;
            StartDate = reward.ValidFrom;
            EndDate = reward.ValidTo;
            IsActive = reward.IsActive;
            CreatedAt = reward.CreatedAt;
            UpdatedAt = reward.UpdatedAt;
        }
    }
} 