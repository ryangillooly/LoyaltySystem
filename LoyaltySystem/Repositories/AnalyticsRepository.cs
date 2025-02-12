namespace LoyaltySystem.Repositories;

using LoyaltySystem.Data;
using Microsoft.Extensions.Configuration;
using LinqToDB;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System;
using LoyaltySystem.Services; // for the DTO classes (ActivityResult, BreakdownResult, etc.)

public interface IAnalyticsRepository
{
    Task<int> GetTotalStampsAsync(int businessId);
    Task<int> GetTotalRedemptionsAsync(int businessId);
    Task<int> GetTotalMembersAsync(int businessId);
    Task<int> GetTotalRewardsAsync(int businessId);

    Task<List<ActivityResult>> GetActivityAsync(int businessId, int days);
    Task<List<BreakdownResult>> GetStampBreakdownAsync(int businessId, string by);
    Task<List<BreakdownResult>> GetRedemptionsBreakdownAsync(int businessId, string by);
    Task<string> GenerateReportAsync(int businessId);
}

public class AnalyticsRepository : IAnalyticsRepository
{
    private readonly IConfiguration _config;

    public AnalyticsRepository(IConfiguration config)
    {
        _config = config;
    }

    public async Task<int> GetTotalStampsAsync(int businessId)
    {
        var connString = _config.GetConnectionString("DefaultConnection");
        using var db = new Linq2DbConnection(connString);

        // Count StampTransaction rows
        return await db.StampTransactions
            .Where(s => s.BusinessId == businessId)
            .CountAsync();
    }

    public async Task<int> GetTotalRedemptionsAsync(int businessId)
    {
        var connString = _config.GetConnectionString("DefaultConnection");
        using var db = new Linq2DbConnection(connString);

        // Count RedemptionTransaction rows
        return await db.RedemptionTransactions
            .Where(r => r.BusinessId == businessId)
            .CountAsync();
    }

    public async Task<int> GetTotalMembersAsync(int businessId)
    {
        var connString = _config.GetConnectionString("DefaultConnection");
        using var db = new Linq2DbConnection(connString);

        // Count members table
        return await db.Members
            .Where(m => m.BusinessId == businessId)
            .CountAsync();
    }

    public async Task<int> GetTotalRewardsAsync(int businessId)
    {
        var connString = _config.GetConnectionString("DefaultConnection");
        using var db = new Linq2DbConnection(connString);

        // Count rows in Rewards
        return await db.Rewards
            .Where(w => w.BusinessId == businessId)
            .CountAsync();
    }

    public async Task<List<ActivityResult>> GetActivityAsync(int businessId, int days)
    {
        var connString = _config.GetConnectionString("DefaultConnection");
        using var db = new Linq2DbConnection(connString);

        var now = DateTime.UtcNow;
        var startDate = now.AddDays(-days);

        // Example: group by date, count stamps & redemptions & new members
        // Stamps
        var stampsQuery = db.StampTransactions
            .Where(s => s.BusinessId == businessId && s.Timestamp >= startDate)
            .Select(s => new { Date = s.Timestamp.Date });

        var stampsGroup = await stampsQuery
            .GroupBy(x => x.Date)
            .Select(g => new
            {
                Date = g.Key,
                StampsCount = g.Count()
            })
            .ToListAsync();

        // Redemptions
        var redemptionsQuery = db.RedemptionTransactions
            .Where(r => r.BusinessId == businessId && r.Timestamp >= startDate)
            .Select(r => new { Date = r.Timestamp.Date });

        var redemptionsGroup = await redemptionsQuery
            .GroupBy(x => x.Date)
            .Select(g => new
            {
                Date = g.Key,
                RedemptionCount = g.Count()
            })
            .ToListAsync();

        // New Members
        var membersQuery = db.Members
            .Where(m => m.BusinessId == businessId && m.JoinedAt >= startDate)
            .Select(m => new { Date = m.JoinedAt.Date });

        var membersGroup = await membersQuery
            .GroupBy(x => x.Date)
            .Select(g => new
            {
                Date = g.Key,
                NewMembersCount = g.Count()
            })
            .ToListAsync();

        // Combine results
        // For each date in the range, we find stamps, redemptions, new members
        var results = new List<ActivityResult>();
        var allDates = stampsGroup.Select(x => x.Date)
            .Union(redemptionsGroup.Select(x => x.Date))
            .Union(membersGroup.Select(x => x.Date))
            .Distinct()
            .OrderBy(d => d);

        foreach (var d in allDates)
        {
            var stamps = stampsGroup.FirstOrDefault(x => x.Date == d)?.StampsCount ?? 0;
            var reds = redemptionsGroup.FirstOrDefault(x => x.Date == d)?.RedemptionCount ?? 0;
            var mems = membersGroup.FirstOrDefault(x => x.Date == d)?.NewMembersCount ?? 0;

            results.Add(new ActivityResult
            {
                Date = d,
                StampsIssued = stamps,
                Redemptions = reds,
                NewMembers = mems
            });
        }

        return results;
    }

    public async Task<List<BreakdownResult>> GetStampBreakdownAsync(int businessId, string by)
    {
        // E.g., group stamps by store, or by loyalty card
        var connString = _config.GetConnectionString("DefaultConnection");
        using var db = new Linq2DbConnection(connString);

        if (by == "store")
        {
            // Group stamptransaction by store
            var query = db.StampTransactions
                .Where(s => s.BusinessId == businessId)
                .GroupBy(s => s.StoreId)
                .Select(g => new BreakdownResult
                {
                    Key = g.Key.ToString(), // storeId as string
                    Count = g.Count()
                });
            return await query.ToListAsync();
        }
        else if (by == "loyaltyCard")
        {
            var query = db.StampTransactions
                .Where(s => s.BusinessId == businessId)
                .GroupBy(s => s.UserLoyaltyCardId)
                .Select(g => new BreakdownResult
                {
                    Key = g.Key.ToString(), // userLoyaltyCardId as string
                    Count = g.Count()
                });
            return await query.ToListAsync();
        }
        else
        {
            // Default
            return new List<BreakdownResult>();
        }
    }

    public async Task<List<BreakdownResult>> GetRedemptionsBreakdownAsync(int businessId, string by)
    {
        // Similar logic for redemptiontransaction
        var connString = _config.GetConnectionString("DefaultConnection");
        using var db = new Linq2DbConnection(connString);

        if (by == "store")
        {
            var query = db.RedemptionTransactions
                .Where(r => r.BusinessId == businessId)
                .GroupBy(r => r.StoreId)
                .Select(g => new BreakdownResult
                {
                    Key = g.Key.ToString(),
                    Count = g.Count()
                });
            return await query.ToListAsync();
        }
        else if (by == "reward")
        {
            var query = db.RedemptionTransactions
                .Where(r => r.BusinessId == businessId)
                .GroupBy(r => r.RewardId)
                .Select(g => new BreakdownResult
                {
                    Key = g.Key.ToString(),
                    Count = g.Count()
                });
            return await query.ToListAsync();
        }
        else
        {
            // Default
            return new List<BreakdownResult>();
        }
    }

    public async Task<string> GenerateReportAsync(int businessId)
    {
        // Example: gather some data and return as CSV or JSON string
        var stamps = await GetTotalStampsAsync(businessId);
        var redemptions = await GetTotalRedemptionsAsync(businessId);

        // For demonstration, just return a simple string
        var report = $"Business {businessId} - Stamps: {stamps}, Redemptions: {redemptions}";
        return report;
    }
}
