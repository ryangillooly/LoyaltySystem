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
       
       /// <summary>
       /// Initializes a new instance of the <see cref="SocialAuthService"/> class with the specified user repository, JWT service, and logger.
       /// </summary>
       public SocialAuthService(
           IUserRepository userRepository,
           IJwtService jwtService,
           ILogger logger)
       {
           _userRepository = userRepository;
           _jwtService = jwtService;
           _logger = logger;
       }
   
       /// <summary>
       /// Authenticates a user via a social provider (Google or Apple), registering a new user if necessary, and returns a JWT token along with user details.
       /// </summary>
       /// <param name="request">The social authentication request containing provider and credentials.</param>
       /// <param name="allowedRoles">The set of roles that are permitted to authenticate using this method.</param>
       /// <param name="registerUserAsync">A delegate function to register a new user if one does not exist.</param>
       /// <returns>An operation result containing the JWT token, internal user information, and a flag indicating if the user is newly registered.</returns>
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
       
   
       /// <summary>
       /// Validates a Google authentication code and extracts user information.
       /// </summary>
       /// <param name="authCode">The authorization code received from Google during social login.</param>
       /// <returns>
       /// A <see cref="SocialUserInfo"/> object containing the user's Google account details if validation succeeds; otherwise, <c>null</c>.
       /// </returns>
       private async Task<SocialUserInfo?> ValidateGoogleTokenAsync(string authCode)
       {
           // Use Google API to validate the token and extract user info
           // (Use Google.Apis.Auth or similar library)
           // Return SocialUserInfo { Id, Email, FirstName, LastName }
           throw new NotImplementedException();
       }
   
       /// <summary>
       /// Validates an Apple authentication code and extracts user information from the Apple API.
       /// </summary>
       /// <param name="authCode">The authorization code received from Apple's authentication flow.</param>
       /// <param name="nonce">An optional nonce value used for additional security validation.</param>
       /// <returns>
       /// A <see cref="SocialUserInfo"/> object containing the user's Apple ID, email, first name, and last name if validation succeeds; otherwise, <c>null</c>.
       /// </returns>
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