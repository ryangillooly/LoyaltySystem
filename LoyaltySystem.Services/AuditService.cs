using System.Globalization;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using LoyaltySystem.Core.Enums;
using LoyaltySystem.Core.Interfaces;
using LoyaltySystem.Core.Settings;
using LoyaltySystem.Core.Extensions;
using LoyaltySystem.Core.Models;

namespace LoyaltySystem.Services;

public class AuditService : IAuditService
{
    private readonly IAuditRepository _auditRepository;
    public AuditService(IAuditRepository auditRepository) => _auditRepository = auditRepository;

    public async Task CreateAuditRecordAsync<T>(AuditRecord auditRecord)
    {
        await _auditRepository.CreateAuditRecordAsync<T>(auditRecord);
    }
}
