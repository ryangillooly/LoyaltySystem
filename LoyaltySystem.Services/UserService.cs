using Amazon.DynamoDBv2.Model;
using LoyaltySystem.Core.Enums;
using LoyaltySystem.Core.Models;
using LoyaltySystem.Core.Interfaces;

namespace LoyaltySystem.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;

        public UserService(IUserRepository userRepository, IEmailService emailService) =>
            (_userRepository, _emailService) = (userRepository, emailService);

        public async Task<User> CreateAsync(User newUser)
        {
            var emailExists = await _emailService.IsEmailUnique(newUser.ContactInfo.Email);

            if (emailExists)
                throw new InvalidOperationException("Email already exists");

            var auditRecord = new AuditRecord(EntityType.User, newUser.Id, ActionType.CreateAccount)
            {
                Source = "Mobile Webpage"
            };
            
            await _userRepository.CreateAsync(newUser);
            // await _auditService.CreateAuditRecordAsync<User>(auditRecord); // Look to use Event Handlers for Auditing (event / delegates)
            
            return newUser;
        }
        
        public async Task<IEnumerable<User>> GetAllAsync() => await _userRepository.GetAllAsync();

        public async Task<User> GetUserAsync(Guid userId)
        {
            var user = await _userRepository.GetUserAsync(userId);
            if(user is null) throw new ResourceNotFoundException("User not found");
            return user;
        }

        public async Task DeleteUserAsync(Guid userId) => await _userRepository.DeleteUserAsync(userId);
        
        public async Task<User> UpdateUserAsync(User updatedUser)
        {
            var currentRecord = await _userRepository.GetUserAsync(updatedUser.Id);
            if(currentRecord == null) throw new Exception("Record not found.");
            var mergedRecord = User.Merge(currentRecord, updatedUser);
            
            await _userRepository.UpdateUserAsync(mergedRecord);
            
            return mergedRecord;
        }
        
    }
}