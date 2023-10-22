using Amazon.DynamoDBv2.Model;
using LoyaltySystem.Core.Dtos;
using Microsoft.AspNetCore.Mvc;
using LoyaltySystem.Core.Models;
using LoyaltySystem.Core.Interfaces;
using LoyaltySystem.Core.Mappers;

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
            var newUser = new UserMapper().CreateUserDtoToUser(dto);
            var createdUser = await _userService.CreateAsync(newUser);
            return CreatedAtAction(nameof(GetUser), new { userId = createdUser.Id }, createdUser);
        }
        
        [HttpPut("{userId:guid}")]
        public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] User user)
        {
            user.Id = userId;
            var updatedUser = await _userService.UpdateUserAsync(user);
            if (updatedUser == null) return NotFound();

            return Ok(updatedUser);
        }
        
        [HttpDelete("{userId:guid}")]
        public async Task<IActionResult> DeleteUser(Guid userId)
        {
            await _userService.DeleteUserAsync(userId);
            // Need to make sure that we delete all data related to a User which is being deleted (i.e. Permissions, Loyalty Cards etc)
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
            catch(ResourceNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch(Exception ex)
            {
                // Handle other exceptions as needed
                return StatusCode(500, $"Internal server error - {ex}");
            }
        }

        [HttpGet("{userId:guid}/businesses")]
        public async Task<IActionResult> GetBusinessPermissions(Guid userId) => Ok(await _userService.GetUsersBusinessPermissions(userId));
    }
}