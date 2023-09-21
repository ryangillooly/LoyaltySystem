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
        
        [HttpPut]
        [Route("{userId}/permissions")]
        public async Task<bool> UpdatePermissions(string userId, [FromBody] List<UserPermission> permissions)
        {
            var permissionList = new List<UserPermission>();
            
            foreach (var permission in permissions)
            {
                permissionList.Add
                (
                    new UserPermission
                    {
                        UserId = userId,
                        BusinessId = permission.BusinessId,
                        Role = Enum.Parse<UserRole>(permission.Role.ToString())
                    }
                );
            }

            await _userService.UpdatePermissionsAsync(permissionList);
            return true;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _userService.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(Guid id) => Ok(await _userService.GetByIdAsync(id));
    }
}