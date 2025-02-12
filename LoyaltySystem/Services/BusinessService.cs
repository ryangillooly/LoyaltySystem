namespace LoyaltySystem.Services;

using System.Collections.Generic;
using System.Threading.Tasks;
using LoyaltySystem.Data;
using LoyaltySystem.Repositories;
using System;

public interface IBusinessService
{
    Task<int> CreateBusinessAsync(CreateBusinessDto dto);
    Task<List<Business>> GetAllBusinessesAsync(int userId);
    Task<Business?> GetBusinessByIdAsync(int businessId);
    Task UpdateBusinessAsync(int businessId, UpdateBusinessDto dto);
    Task DeleteBusinessAsync(int businessId);

    // Staff
    Task<List<BusinessUser>> GetStaffAsync(int businessId);
    Task AddStaffAsync(int businessId, AddStaffDto dto);
    Task UpdateStaffAsync(int businessUserId, UpdateStaffDto dto);
    Task RemoveStaffAsync(int businessUserId);
}

public class BusinessService : IBusinessService
{
    private readonly IBusinessRepository _businessRepository;

    public BusinessService(IBusinessRepository businessRepository)
    {
        _businessRepository = businessRepository;
    }

    public async Task<int> CreateBusinessAsync(CreateBusinessDto dto)
    {
        var business = new Business
        {
            BusinessName = dto.BusinessName,
            Category = dto.Category,
            WebsiteUrl = dto.WebsiteUrl,
            Description = dto.Description,
            LogoUrl = dto.LogoUrl,
            CoverImageUrl = dto.CoverImageUrl,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return await _businessRepository.CreateBusinessAsync(business);
    }

    public async Task<List<Business>> GetAllBusinessesAsync(int userId)
    {
        // Possibly filter only businesses user can access
        return await _businessRepository.GetAllBusinessesAsync(userId);
    }

    public async Task<Business?> GetBusinessByIdAsync(int businessId)
    {
        return await _businessRepository.GetBusinessByIdAsync(businessId);
    }

    public async Task UpdateBusinessAsync(int businessId, UpdateBusinessDto dto)
    {
        var existing = await _businessRepository.GetBusinessByIdAsync(businessId);
        if (existing == null) throw new Exception("Business not found");

        existing.BusinessName = dto.BusinessName ?? existing.BusinessName;
        existing.Category = dto.Category ?? existing.Category;
        existing.WebsiteUrl = dto.WebsiteUrl ?? existing.WebsiteUrl;
        existing.Description = dto.Description ?? existing.Description;
        existing.LogoUrl = dto.LogoUrl ?? existing.LogoUrl;
        existing.CoverImageUrl = dto.CoverImageUrl ?? existing.CoverImageUrl;

        await _businessRepository.UpdateBusinessAsync(existing);
    }

    public async Task DeleteBusinessAsync(int businessId)
    {
        // optionally check if you have permission
        await _businessRepository.DeleteBusinessAsync(businessId);
    }

    // ========== Staff Methods ==========
    public async Task<List<BusinessUser>> GetStaffAsync(int businessId)
    {
        return await _businessRepository.GetStaffAsync(businessId);
    }

    public async Task AddStaffAsync(int businessId, AddStaffDto dto)
    {
        var staff = new BusinessUser
        {
            BusinessId = businessId,
            UserId = dto.UserId,
            Role = dto.Role,
            CreatedAt = DateTime.UtcNow
        };
        await _businessRepository.AddStaffAsync(staff);
    }

    public async Task UpdateStaffAsync(int businessUserId, UpdateStaffDto dto)
    {
        var existing = await _businessRepository.GetBusinessUserAsync(businessUserId);
        if (existing == null) throw new Exception("Staff record not found");

        existing.Role = dto.Role ?? existing.Role;
        await _businessRepository.UpdateStaffAsync(existing);
    }

    public async Task RemoveStaffAsync(int businessUserId)
    {
        await _businessRepository.RemoveStaffAsync(businessUserId);
    }
}

// DTOs
public class CreateBusinessDto
{
    public string BusinessName { get; set; }
    public string Category { get; set; }
    public string WebsiteUrl { get; set; }
    public string Description { get; set; }
    public string LogoUrl { get; set; }
    public string CoverImageUrl { get; set; }
}

public class UpdateBusinessDto
{
    public string BusinessName { get; set; }
    public string Category { get; set; }
    public string WebsiteUrl { get; set; }
    public string Description { get; set; }
    public string LogoUrl { get; set; }
    public string CoverImageUrl { get; set; }
}

public class AddStaffDto
{
    public int UserId { get; set; }
    public string Role { get; set; }
}

public class UpdateStaffDto
{
    public string Role { get; set; }
}
