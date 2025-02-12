namespace LoyaltySystem.Services;

using LoyaltySystem.Repositories;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

public interface IAnalyticsService
{
    Task<SummaryResult> GetSummaryAsync(int businessId);
    Task<List<ActivityResult>> GetActivityAsync(int businessId, string range);
    Task<List<BreakdownResult>> GetStampBreakdownAsync(int businessId, string by);
    Task<List<BreakdownResult>> GetRedemptionsBreakdownAsync(int businessId, string by);
    Task<string> GenerateReportAsync(int businessId);
}

public class AnalyticsService : IAnalyticsService
{
    private readonly IAnalyticsRepository _repo;

    public AnalyticsService(IAnalyticsRepository repo)
    {
        _repo = repo;
    }

    public async Task<SummaryResult> GetSummaryAsync(int businessId)
    {
        // Query top-level counts from stamps, redemptions, members, etc.
        var totalStamps = await _repo.GetTotalStampsAsync(businessId);
        var totalRedemptions = await _repo.GetTotalRedemptionsAsync(businessId);
        var totalMembers = await _repo.GetTotalMembersAsync(businessId);
        var totalRewards = await _repo.GetTotalRewardsAsync(businessId);

        return new SummaryResult
        {
            TotalStamps = totalStamps,
            TotalRedemptions = totalRedemptions,
            TotalMembers = totalMembers,
            TotalRewards = totalRewards
        };
    }

    public async Task<List<ActivityResult>> GetActivityAsync(int businessId, string range)
    {
        // For example, return daily aggregates for the last X days
        // parse 'range' (like '7d', '30d')
        var days = range == "30d" ? 30 : 7;
        return await _repo.GetActivityAsync(businessId, days);
    }

    public async Task<List<BreakdownResult>> GetStampBreakdownAsync(int businessId, string by)
    {
        return await _repo.GetStampBreakdownAsync(businessId, by);
    }

    public async Task<List<BreakdownResult>> GetRedemptionsBreakdownAsync(int businessId, string by)
    {
        return await _repo.GetRedemptionsBreakdownAsync(businessId, by);
    }

    public async Task<string> GenerateReportAsync(int businessId)
    {
        // Possibly gather data from multiple queries, generate CSV or PDF, etc.
        // For now, return a JSON or text string
        return await _repo.GenerateReportAsync(businessId);
    }
}

// =========== Example Return Classes ===========

public class SummaryResult
{
    public int TotalStamps { get; set; }
    public int TotalRedemptions { get; set; }
    public int TotalMembers { get; set; }
    public int TotalRewards { get; set; }
}

public class ActivityResult
{
    public DateTime Date { get; set; }
    public int StampsIssued { get; set; }
    public int Redemptions { get; set; }
    public int NewMembers { get; set; }
}

public class BreakdownResult
{
    public string Key { get; set; }   // e.g., store name or reward title
    public int Count { get; set; }
}
