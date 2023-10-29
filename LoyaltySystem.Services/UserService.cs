using Amazon.DynamoDBv2.Model;
using LoyaltySystem.Core.DTOs;
using LoyaltySystem.Core.Enums;
using LoyaltySystem.Core.Models;
using LoyaltySystem.Core.Interfaces;
using LoyaltySystem.Core.Settings;

namespace LoyaltySystem.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly EmailSettings _emailSettings;

        public UserService(IUserRepository userRepository, IEmailService emailService, EmailSettings emailSettings) =>
            (_userRepository, _emailService, _emailSettings) = (userRepository, emailService, emailSettings);

        public async Task<User> CreateAsync(User newUser)
        {
            var emailExists = await _emailService.IsEmailUnique(newUser.ContactInfo.Email);
            if (emailExists) throw new InvalidOperationException("Email already exists");

            var token = Guid.NewGuid();
                
            await _userRepository.CreateAsync(newUser, token);
            //await _userRepository.SendVerificationEmailAsync(newUser.ContactInfo.Email, newUser.Id, token);
            
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
            var mergedRecord = User.Merge(currentRecord, updatedUser);
            
            await _userRepository.UpdateUserAsync(mergedRecord);
            
            return mergedRecord;
        }

        public async Task<List<BusinessUserPermissions>> GetUsersBusinessPermissions(Guid userId) =>
            await _userRepository.GetUsersBusinessPermissions(userId);

        public async Task VerifyEmailAsync(VerifyEmailDto dto)
        {
            await _userRepository.VerifyEmailAsync(dto);
        }
    }
}