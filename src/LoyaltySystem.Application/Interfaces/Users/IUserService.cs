using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.DTOs.Auth.PasswordReset;
using LoyaltySystem.Application.DTOs.AuthDtos;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Entities;
using System.Data;

namespace LoyaltySystem.Application.Interfaces.Users;

public interface IUserService 
{
    Task<OperationResult<InternalUserDto>> GetByIdAsync(UserId userId);
    Task<OperationResult<InternalUserDto>> GetByEmailAsync(string email);
    Task<OperationResult<InternalUserDto>> GetByUsernameAsync(string username);
    Task<OperationResult<InternalUserDto>> GetUserByCustomerIdAsync(CustomerId customerId);
    
    Task<OperationResult<InternalUserDto>> AddAsync(User user, IDbTransaction? transaction = null);
    Task<OperationResult<InternalUserDto>> UpdateAsync(UserId userId, UpdateUserRequestDto updateRequestDto);
    Task<OperationResult<InternalUserDto>> DeleteAsync(UserId userId, UpdateUserRequestDto updateRequestDto);
    Task<OperationResult<InternalUserDto>> UpdatePasswordAsync(InternalUserDto internalUserDto, ResetPasswordRequestDto resetDto);
    
    Task<OperationResult> ConfirmEmailAndUpdateAsync(UserId userId);
}
