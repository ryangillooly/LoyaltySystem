using System.Collections.Generic;
using System.Threading.Tasks;
using LoyaltySystem.Core.Enums;
using LoyaltySystem.Core.Models;
using LoyaltySystem.Core.Interfaces;

namespace LoyaltySystem.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuditService _auditService;

        public UserService(IUserRepository userRepository, IAuditService auditService)
        {
            _userRepository = userRepository;
            _auditService = auditService;
        }

        public async Task<User> CreateAsync(User newUser)
        {
            var emailExists = await _userRepository.DoesEmailExistAsync(newUser.ContactInfo.Email);

            if (emailExists)
                throw new InvalidOperationException("Email already exists");
      
            await _userRepository.CreateUserAsync(newUser);
            await _auditService.CreateAuditRecordAsync<User>(newUser.Id, InteractionType.SignUp);
            return newUser;
        }
        

        public async Task<IEnumerable<User>> GetAllAsync() => await _userRepository.GetAllAsync();
        public async Task<User> GetByIdAsync(Guid id) => await _userRepository.GetByIdAsync(id);
        public async Task DeleteAsync(Guid id) => await _userRepository.DeleteAsync(id);
        public async Task UpdateAsync(User user) => await _userRepository.UpdateAsync(user);
    }
}