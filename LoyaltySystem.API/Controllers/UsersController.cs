using Microsoft.AspNetCore.Mvc;
using LoyaltySystem.Core.Models;
using LoyaltySystem.Core.Interfaces;
using System.Threading.Tasks;
using LoyaltySystem.Core.Interfaces;
using LoyaltySystem.Core.Models;

namespace LoyaltySystem.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // Business logic here
            return Ok(await _userService.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(Guid id)
        {
            // Business logic here
            return Ok(await _userService.GetByIdAsync(id));
        }
        
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] User newUser)
        {
            var createdUser = await _userService.CreateUser(newUser);
            if (createdUser is null)
            {
                return BadRequest("Failed to create user");
            }
            else
            {
                Console.WriteLine($"User {createdUser.Username} Created");
            }

            return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id }, createdUser);
        }

        // ... (Other RESTful actions like POST, PUT, DELETE)
    }
}