namespace LoyaltySystem.Services;

using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using LoyaltySystem.Data;
using LoyaltySystem.Repositories;

public interface IPromotionsService
{
    Task<int> CreatePromotionAsync(int businessId, CreatePromotionDto dto);
    Task<List<Promotion>> GetPromotionsAsync(int businessId);
    Task<Promotion?> GetPromotionAsync(int businessId, int promotionId);
    Task UpdatePromotionAsync(int businessId, int promotionId, UpdatePromotionDto dto);
    Task DeletePromotionAsync(int businessId, int promotionId);
}

public class PromotionsService : IPromotionsService
{
    private readonly IPromotionsRepository _repo;

    public PromotionsService(IPromotionsRepository repo)
    {
        _repo = repo;
    }

    public async Task<int> CreatePromotionAsync(int businessId, CreatePromotionDto dto)
    {
        var promo = new Promotion
        {
            BusinessId = businessId,
            Title = dto.Title,
            Description = dto.Description,
            ValidFrom = dto.ValidFrom,
            ValidUntil = dto.ValidUntil,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return await _repo.CreatePromotionAsync(promo);
    }

    public async Task<List<Promotion>> GetPromotionsAsync(int businessId)
    {
        return await _repo.GetPromotionsByBusinessAsync(businessId);
    }

    public async Task<Promotion?> GetPromotionAsync(int businessId, int promotionId)
    {
        return await _repo.GetPromotionByIdAsync(businessId, promotionId);
    }

    public async Task UpdatePromotionAsync(int businessId, int promotionId, UpdatePromotionDto dto)
    {
        var existing = await _repo.GetPromotionByIdAsync(businessId, promotionId);
        if (existing == null)
        {
            throw new Exception("Promotion not found.");
        }

        existing.Title = dto.Title ?? existing.Title;
        existing.Description = dto.Description ?? existing.Description;
        if (dto.ValidFrom.HasValue) existing.ValidFrom = dto.ValidFrom.Value;
        if (dto.ValidUntil.HasValue) existing.ValidUntil = dto.ValidUntil.Value;

        await _repo.UpdatePromotionAsync(existing);
    }

    public async Task DeletePromotionAsync(int businessId, int promotionId)
    {
        await _repo.DeletePromotionAsync(businessId, promotionId);
    }
}

// ============= DTOs =============
public class CreatePromotionDto
{
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidUntil { get; set; }
}

public class UpdatePromotionDto
{
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidUntil { get; set; }
}
