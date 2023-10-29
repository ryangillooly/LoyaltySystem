using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using LoyaltySystem.Core.Models;
using LoyaltySystem.Services;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Google.Apis.Auth;
using LoyaltySystem.Core.Interfaces;

namespace LoyaltySystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;

        public AuthController(IUserService userService, IConfiguration configuration)
        {
            _userService = userService;
            _configuration = configuration;
        }

       /*
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] string tokenId)
        {
            return new IActionResult();
        }
        */
       
        [HttpPost("verify-google-token")]
        public async Task<IActionResult> VerifyGoogleToken(string tokenId)
        {
            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(tokenId);

                // payload contains the user's information
                var userId = payload.Subject;
                var userEmail = payload.Email;
                // ... you can extract other details as needed

                // With this verified information, you can now check if this user is already in your database, 
                // create a new user, or issue them a JWT for accessing the rest of your API securely.

                return Ok();
            }
            catch (Exception ex)
            {
                return Unauthorized();
            }
        }
        
        /*
        private string GenerateJwtToken(string userEmail)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("YOUR_SECRET_KEY"));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Email, userEmail)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = credentials,
                Issuer = "YOUR_ISSUER",
                Audience = "YOUR_AUDIENCE"
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        */
    }

    public class UserLoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
