namespace LoyaltySystem.Services;

using LoyaltySystem.Data;
using LoyaltySystem.Repositories;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

public interface IMembersService
{
    Task<int> AddMemberAsync(int businessId, AddMemberDto dto);
    Task<Member> GetMemberAsync(int businessId, int memberId);
    Task<List<Member>> GetMembersAsync(int businessId);
    Task UpdateMemberAsync(int businessId, int memberId, UpdateMemberDto dto);
    Task DeleteMemberAsync(int businessId, int memberId);
}

public class MembersService : IMembersService
{
    private readonly IMembersRepository _repo;

    public MembersService(IMembersRepository repo)
    {
        _repo = repo;
    }

    public async Task<int> AddMemberAsync(int businessId, AddMemberDto dto)
    {
        var member = new Member
        {
            BusinessId = businessId,
            Name = dto.Name,
            Email = dto.Email,
            // JoinedAt = DateTime.UtcNow, // or default in DB
        };
        return await _repo.AddMemberAsync(member);
    }

    public async Task<Member> GetMemberAsync(int businessId, int memberId)
    {
        var member = await _repo.GetMemberByIdAsync(businessId, memberId);
        // optionally throw if null
        return member;
    }

    public async Task<List<Member>> GetMembersAsync(int businessId)
    {
        return await _repo.GetMembersByBusinessAsync(businessId);
    }

    public async Task UpdateMemberAsync(int businessId, int memberId, UpdateMemberDto dto)
    {
        var existing = await _repo.GetMemberByIdAsync(businessId, memberId);
        if (existing == null)
            throw new Exception("Member not found");

        existing.Name = dto.Name ?? existing.Name;
        existing.Email = dto.Email ?? existing.Email;

        await _repo.UpdateMemberAsync(existing);
    }

    public async Task DeleteMemberAsync(int businessId, int memberId)
    {
        await _repo.DeleteMemberAsync(businessId, memberId);
    }
}

// Dtos
public class AddMemberDto
{
    public string Name { get; set; }
    public string Email { get; set; }
}

public class UpdateMemberDto
{
    public string Name { get; set; }
    public string Email { get; set; }
}
