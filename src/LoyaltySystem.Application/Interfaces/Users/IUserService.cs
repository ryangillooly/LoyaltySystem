using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.DTOs.AuthDtos;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Entities;
using System.Data;

namespace LoyaltySystem.Application.Interfaces.Users;

public interface IUserService 
{
    Task<OperationResult<UserDto>> GetByIdAsync(UserId userId);
    Task<OperationResult<UserDto>> GetByEmailAsync(string email);
    Task<OperationResult<UserDto>> GetByUsernameAsync(string username);
    Task<OperationResult<UserDto>> GetUserByCustomerIdAsync(CustomerId customerId);
    
    Task<OperationResult<UserDto>> AddAsync(User user, IDbTransaction? transaction = null);
    Task<OperationResult<UserDto>> UpdateAsync(UserId userId, UpdateUserDto updateDto);
    Task<OperationResult<UserDto>> DeleteAsync(UserId userId, UpdateUserDto updateDto);
    Task<OperationResult> ConfirmEmailAndUpdateAsync(UserId userId);
}
