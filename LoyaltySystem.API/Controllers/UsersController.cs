using LoyaltySystem.Core.Dtos;
using LoyaltySystem.Core.DTOs;
using Microsoft.AspNetCore.Mvc;
using LoyaltySystem.Core.Models;
using LoyaltySystem.Core.Interfaces;
using static LoyaltySystem.Core.Exceptions.UserExceptions;

namespace LoyaltySystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        public UsersController(IUserService userService) => _userService = userService;

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            try
            {
                var createdUser = await _userService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetUser), new { userId = createdUser.Id }, createdUser);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error - {ex}");
            }
        }
        
        [HttpPut("{userId:guid}")]
        public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] User user)
        {
            try
            {
                user.Id = userId;
                var updatedUser = await _userService.UpdateUserAsync(user);
                return Ok(updatedUser);
            }
            catch(UserNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Internal server error - {ex}");
            }
        }

        [HttpPost("{userId:guid}/verify-email/{token:guid}")]
        public async Task<IActionResult> VerifyEmail(Guid userId, Guid token)
        {
            await _userService.VerifyEmailAsync(new VerifyUserEmailDto(userId, token));
            return Ok();
        }
        
        [HttpDelete("{userId:guid}")]
        public async Task<IActionResult> DeleteUser(Guid userId)
        {
            await _userService.DeleteUserAsync(userId);
            return NoContent();
        }
        
        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _userService.GetAllAsync());

        [HttpGet("{userId:guid}")]
        public async Task<IActionResult> GetUser(Guid userId)
        {
            try
            {
                var user = await _userService.GetUserAsync(userId);
                return Ok(user);
            }
            catch(UserNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Internal server error - {ex}");
            }
        }

        [HttpGet("{userId:guid}/businesses")]
        public async Task<IActionResult> GetBusinessPermissions(Guid userId) => Ok(await _userService.GetUsersBusinessPermissions(userId));
    }
}