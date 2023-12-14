using AutoMapper;
using LoyaltySystem.Core.Dtos;
using LoyaltySystem.Core.DTOs;
using static LoyaltySystem.Core.Exceptions.UserExceptions;
using LoyaltySystem.Core.Models;
using LoyaltySystem.Core.Interfaces;

namespace LoyaltySystem.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailService   _emailService;
    private readonly IMapper         _mapper;

    public UserService(IUserRepository userRepository, IEmailService emailService, IMapper mapper) =>
        (_userRepository, _emailService, _mapper) = (userRepository, emailService, mapper);

    public async Task<User> CreateAsync(CreateUserDto dto)
    {
        await _emailService.ValidateEmailAsync(dto.ContactInfo.Email);
        
        var newUser = _mapper.Map<User>(dto);
        var token   = _emailService.GenerateEmailToken(newUser);
        
        try
        {
            await _userRepository.CreateAsync(newUser, token);
            //await _emailService.SendVerificationEmailAsync(newUser, token); // TODO: Uncomment SendEmail
        }
        catch (Exception ex)
        {
            await DeleteUserAsync(newUser.Id);
            throw new UserCreationException(ex);
        }
        
        return newUser;
    }
    public async Task<IEnumerable<User>> GetAllAsync() => await _userRepository.GetAllAsync();
    public async Task<User> GetUserAsync(Guid userId) => await _userRepository.GetUserAsync(userId);

    // TODO: Need to make sure that we don't delete a user if it still owns a Business
    // TODO: Need to make sure that we delete all data related to a User which is being deleted (i.e. Permissions, Loyalty Cards etc)
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
    public async Task VerifyEmailAsync(VerifyUserEmailDto dto) => 
        await _userRepository.VerifyEmailAsync(dto);
}