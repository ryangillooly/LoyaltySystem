using LoyaltySystem.Core.DTOs;
using LoyaltySystem.Core.Exceptions;
using LoyaltySystem.Core.Models;
using LoyaltySystem.Core.Interfaces;
using LoyaltySystem.Core.Settings;

namespace LoyaltySystem.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;

    public UserService(IUserRepository userRepository, IEmailService emailService) =>
        (_userRepository, _emailService) = (userRepository, emailService);

    public async Task<User> CreateAsync(User newUser)
    {
        var emailExists = await _emailService.IsEmailUnique(newUser.ContactInfo.Email);
        if (emailExists) throw new InvalidOperationException($"Email {newUser.ContactInfo.Email} already exists");

        var token = new UserEmailToken(newUser.Id, newUser.ContactInfo.Email);
        
        await _userRepository.CreateAsync(newUser, token);
        await _emailService.SendVerificationEmailAsync(newUser, token);
        
        return newUser;
    }
    public async Task<IEnumerable<User>> GetAllAsync() => await _userRepository.GetAllAsync();
    public async Task<User> GetUserAsync(Guid userId)
    {
        var user = await _userRepository.GetUserAsync(userId);
        if (user is null) throw new UserExceptions.UserNotFoundException(userId);
        return user;
    }

    public async Task DeleteUserAsync(Guid userId)
    {
        // TODO: Need to make sure that we don't delete a user if it still owns a Business
        // TODO: Need to make sure that we delete all data related to a User which is being deleted (i.e. Permissions, Loyalty Cards etc)
        await _userRepository.DeleteUserAsync(userId);
    }

    public async Task<User> UpdateUserAsync(User updatedUser)
    {
        var currentRecord = await _userRepository.GetUserAsync(updatedUser.Id);
        var mergedRecord = User.Merge(currentRecord, updatedUser);
        
        await _userRepository.UpdateUserAsync(mergedRecord);
        
        return mergedRecord;
    }
    public async Task<List<BusinessUserPermissions>> GetUsersBusinessPermissions(Guid userId) =>
        await _userRepository.GetUsersBusinessPermissions(userId);
    public async Task VerifyEmailAsync(VerifyUserEmailDto dto)
    {
        await _userRepository.VerifyEmailAsync(dto);
    }
}