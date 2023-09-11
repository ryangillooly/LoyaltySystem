using System.Collections.Generic;
using System.Threading.Tasks;
using LoyaltySystem.Core.Models;

namespace LoyaltySystem.Core.Interfaces;

public interface ILoyaltyCardService
{
    Task<IEnumerable<LoyaltyCard>> GetAllAsync();
    Task<LoyaltyCard> GetByIdAsync(Guid id, string userEmail);
    Task<LoyaltyCard> CreateAsync(LoyaltyCard newLoyaltyCard);
}