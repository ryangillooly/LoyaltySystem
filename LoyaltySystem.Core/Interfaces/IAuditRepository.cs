using LoyaltySystem.Core.Enums;
using LoyaltySystem.Core.Models;

namespace LoyaltySystem.Core.Interfaces;

public interface IAuditRepository
{
    Task CreateAuditRecordAsync<T> (AuditRecord auditRecord);
}