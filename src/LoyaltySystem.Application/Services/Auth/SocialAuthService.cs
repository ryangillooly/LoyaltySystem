using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.DTOs.Auth;
using LoyaltySystem.Application.DTOs.Auth.Social;
using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Enums;
using LoyaltySystem.Domain.Interfaces;
using LoyaltySystem.Domain.Models;
using LoyaltySystem.Domain.Repositories;
using Serilog;
using System.Security.Claims;

public class SocialAuthService : ISocialAuthService
   {
       private readonly IUserRepository _userRepository;
       private readonly IJwtService _jwtService;
       private readonly ILogger _logger;
       private const string Google = "google";
       private const string Apple = "apple";
       
       public SocialAuthService(
           IUserRepository userRepository,
           IJwtService jwtService,
           ILogger logger)
       {
           _userRepository = userRepository;
           _jwtService = jwtService;
           _logger = logger;
       }
   
       public async Task<OperationResult<SocialAuthResponseDto>> AuthenticateAsync(
           SocialAuthRequestDto request,
           IEnumerable<RoleType> allowedRoles,
           Func<RegisterUserRequestDto, Task<OperationResult<RegisterUserResponseDto>>> registerUserAsync)
       {
           // 1. Validate the social token with the provider
           SocialUserInfo? socialUser = null;
           switch (request.Provider.ToString().ToLowerInvariant())
           {
               case Google:
                   socialUser = await ValidateGoogleTokenAsync(request.AuthCode);
                   break;
               
               case Apple:
                   socialUser = await ValidateAppleTokenAsync(request.AuthCode, request.Nonce);
                   break;
               
               default:
                   return OperationResult<SocialAuthResponseDto>.FailureResult("Unsupported social provider.");
           }
   
           if (socialUser == null || string.IsNullOrEmpty(socialUser.Email))
               return OperationResult<SocialAuthResponseDto>.FailureResult("Invalid social login credentials.");
   
           // 2. Check if user exists
           var user = await _userRepository.GetByEmailAsync(socialUser.Email);
   
           bool isNewUser = false;
           if (user is null)
           {
               var registerDto = new RegisterUserRequestDto
               {
                   Email = socialUser.Email,
                   Username = socialUser.Email,
                   FirstName = socialUser.FirstName!,
                   LastName = socialUser.LastName!,
                   Password = "xxxxx", // Random password, not used
                   ConfirmPassword = "xxxxx",
                   Roles = allowedRoles.ToList()
               };
               var registerResult = await registerUserAsync(registerDto);
               if (!registerResult.Success)
                   return OperationResult<SocialAuthResponseDto>.FailureResult(registerResult.Errors);
               
               user = await _userRepository.GetByIdAsync(registerResult.Data.Id);
               
               if (user is null || string.IsNullOrEmpty(user.Email))
                   return OperationResult<SocialAuthResponseDto>.FailureResult("User could not be found in the database");
               
               isNewUser = true;
           }
           else
           {
               // 4. Ensure user has at least one allowed role
               if (!user.Roles.Any(r => allowedRoles.Contains(r.Role)))
                   return OperationResult<SocialAuthResponseDto>.FailureResult("User does not have the required role.");
           }
   
           // 5. Generate JWT
           
           var tokenResult = _jwtService.GenerateTokenResult(user);
   
           return OperationResult<SocialAuthResponseDto>.SuccessResult(new SocialAuthResponseDto
           {
               Token = tokenResult.AccessToken,
               InternalUser = new InternalUserDto
               {
                   Id = user.Id,
                   Email = user.Email,
                   Username = user.Username
               },
               IsNewUser = isNewUser
           });
       }
       
   
       private async Task<SocialUserInfo?> ValidateGoogleTokenAsync(string authCode)
       {
           // Use Google API to validate the token and extract user info
           // (Use Google.Apis.Auth or similar library)
           // Return SocialUserInfo { Id, Email, FirstName, LastName }
           throw new NotImplementedException();
       }
   
       private async Task<SocialUserInfo?> ValidateAppleTokenAsync(string authCode, string? nonce)
       {
           // Use Apple API to validate the token and extract user info
           // Return SocialUserInfo { Id, Email, FirstName, LastName }
           throw new NotImplementedException();
       }
   }
   
   // Helper DTO for extracted social user info
   public class SocialUserInfo
   {
       public string Id { get; set; }
       public string Email { get; set; }
       public string? FirstName { get; set; }
       public string? LastName { get; set; }
   }