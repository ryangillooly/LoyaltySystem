using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using LoyaltySystem.Core.Enums;
using LoyaltySystem.Core.Models;
using LoyaltySystem.Core.Interfaces;

namespace LoyaltySystem.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuditService _auditService;
        private readonly IEmailService _emailService;

        public UserService(IUserRepository userRepository, IAuditService auditService, IEmailService emailService)
        {
            _userRepository = userRepository;
            _auditService = auditService;
            _emailService = emailService;
        }

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
        public async Task<User> GetByIdAsync(Guid id) => await _userRepository.GetByIdAsync(id);
        public async Task DeleteAsync(Guid id) => await _userRepository.DeleteAsync(id);
        
        public async Task<User> UpdateUserAsync(User updatedUser)
        {
            var currentRecord = await _userRepository.GetByIdAsync(updatedUser.Id);
            if(currentRecord == null) throw new Exception("Record not found.");
            var mergedRecord = User.Merge(currentRecord, updatedUser);
            
            await _userRepository.UpdateUserAsync(mergedRecord);
            
            return mergedRecord;
        }
        
    }
}