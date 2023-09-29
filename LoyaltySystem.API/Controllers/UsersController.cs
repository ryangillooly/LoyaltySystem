using LoyaltySystem.Core.Enums;
using Microsoft.AspNetCore.Mvc;
using LoyaltySystem.Core.Models;
using LoyaltySystem.Core.Interfaces;

namespace LoyaltySystem.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        public UsersController(IUserService userService) => _userService = userService;

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] User newUser)
        {
            var createdUser = await _userService.CreateAsync(newUser);
            return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id }, createdUser);
        }
        
        [HttpPut("{userId:guid}")]
        public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] User user)
        {
            user.Id = userId;
            var updatedUser = await _userService.UpdateUserAsync(user);
            if (updatedUser == null) return NotFound();

            return Ok(updatedUser);
        }
        
        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _userService.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(Guid id) => Ok(await _userService.GetByIdAsync(id));
    }
}