namespace LoyaltySystem.Services;

using LoyaltySystem.Data;
using LoyaltySystem.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

public interface ILoyaltyCardService
{
    // Templates
    Task<int> CreateTemplateAsync(int businessId, CreateTemplateDto dto);
    Task<List<LoyaltyCardTemplate>> GetTemplatesAsync(int businessId);
    Task<LoyaltyCardTemplate?> GetTemplateAsync(int businessId, int templateId);
    Task UpdateTemplateAsync(int businessId, int templateId, UpdateTemplateDto dto);
    Task DeleteTemplateAsync(int businessId, int templateId);

    // User Cards
    Task<int> CreateUserCardAsync(int businessId, CreateUserCardDto dto);
    Task<List<UserLoyaltyCard>> GetUserCardsAsync(int businessId);
    Task<UserLoyaltyCard?> GetUserCardAsync(int businessId, int userCardId);
    Task UpdateUserCardAsync(int businessId, int userCardId, UpdateUserCardDto dto);
    Task DeleteUserCardAsync(int businessId, int userCardId);
}

public class LoyaltyCardService : ILoyaltyCardService
{
    private readonly ILoyaltyCardRepository _repository;

    public LoyaltyCardService(ILoyaltyCardRepository repository)
    {
        _repository = repository;
    }

    // ============ Templates ============
    public async Task<int> CreateTemplateAsync(int businessId, CreateTemplateDto dto)
    {
        var template = new LoyaltyCardTemplate
        {
            BusinessId = businessId,
            CardType = dto.CardType,
            CardName = dto.CardName,
            RequiredStamps = dto.RequiredStamps,
            MinimumSpendCondition = dto.MinimumSpendCondition,
            Description = dto.Description,
            ResetAfterCompletion = dto.ResetAfterCompletion,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return await _repository.CreateTemplateAsync(template);
    }

    public async Task<List<LoyaltyCardTemplate>> GetTemplatesAsync(int businessId)
    {
        return await _repository.GetTemplatesByBusinessAsync(businessId);
    }

    public async Task<LoyaltyCardTemplate?> GetTemplateAsync(int businessId, int templateId)
    {
        return await _repository.GetTemplateByIdAsync(businessId, templateId);
    }

    public async Task UpdateTemplateAsync(int businessId, int templateId, UpdateTemplateDto dto)
    {
        var existing = await _repository.GetTemplateByIdAsync(businessId, templateId);
        if (existing == null) throw new Exception("Template not found.");

        existing.CardType = dto.CardType ?? existing.CardType;
        existing.CardName = dto.CardName ?? existing.CardName;
        if (dto.RequiredStamps.HasValue) existing.RequiredStamps = dto.RequiredStamps.Value;
        if (dto.MinimumSpendCondition.HasValue) existing.MinimumSpendCondition = dto.MinimumSpendCondition.Value;
        existing.Description = dto.Description ?? existing.Description;
        if (dto.ResetAfterCompletion.HasValue) existing.ResetAfterCompletion = dto.ResetAfterCompletion.Value;

        await _repository.UpdateTemplateAsync(existing);
    }

    public async Task DeleteTemplateAsync(int businessId, int templateId)
    {
        await _repository.DeleteTemplateAsync(businessId, templateId);
    }

    // ============ User Cards ============
    public async Task<int> CreateUserCardAsync(int businessId, CreateUserCardDto dto)
    {
        var card = new UserLoyaltyCard
        {
            UserId = dto.UserId,
            LoyaltyCardTemplateId = dto.LoyaltyCardTemplateId,
            BusinessId = businessId,
            CurrentStampCount = dto.CurrentStampCount,
            Status = dto.Status ?? "Active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return await _repository.CreateUserCardAsync(card);
    }

    public async Task<List<UserLoyaltyCard>> GetUserCardsAsync(int businessId)
    {
        return await _repository.GetUserCardsByBusinessAsync(businessId);
    }

    public async Task<UserLoyaltyCard?> GetUserCardAsync(int businessId, int userCardId)
    {
        return await _repository.GetUserCardByIdAsync(businessId, userCardId);
    }

    public async Task UpdateUserCardAsync(int businessId, int userCardId, UpdateUserCardDto dto)
    {
        var existing = await _repository.GetUserCardByIdAsync(businessId, userCardId);
        if (existing == null) throw new Exception("User loyalty card not found.");

        if (dto.CurrentStampCount.HasValue) existing.CurrentStampCount = dto.CurrentStampCount.Value;
        existing.Status = dto.Status ?? existing.Status;

        await _repository.UpdateUserCardAsync(existing);
    }

    public async Task DeleteUserCardAsync(int businessId, int userCardId)
    {
        await _repository.DeleteUserCardAsync(businessId, userCardId);
    }
}

// ================== DTOs ==================
public class CreateTemplateDto
{
    public string CardType { get; set; }
    public string CardName { get; set; }
    public int RequiredStamps { get; set; }
    public decimal? MinimumSpendCondition { get; set; }
    public string Description { get; set; }
    public bool ResetAfterCompletion { get; set; }
}

public class UpdateTemplateDto
{
    public string CardType { get; set; }
    public string CardName { get; set; }
    public int? RequiredStamps { get; set; }
    public decimal? MinimumSpendCondition { get; set; }
    public string Description { get; set; }
    public bool? ResetAfterCompletion { get; set; }
}

public class CreateUserCardDto
{
    public int UserId { get; set; }
    public int LoyaltyCardTemplateId { get; set; }
    public int CurrentStampCount { get; set; }
    public string Status { get; set; }
}

public class UpdateUserCardDto
{
    public int? CurrentStampCount { get; set; }
    public string Status { get; set; }
}
