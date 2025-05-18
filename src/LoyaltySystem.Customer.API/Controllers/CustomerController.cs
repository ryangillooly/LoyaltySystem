using System.Security.Claims;
using LoyaltySystem.Application.DTOs.Customers;
using LoyaltySystem.Application.Interfaces.Customers;
using LoyaltySystem.Application.Interfaces.Users;
using LoyaltySystem.Shared.API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltySystem.Customer.API.Controllers;

[ApiController]
[Route("api/customer")]
[Authorize]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly IUserService _userService;
    private readonly ILogger<CustomerController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomerController"/> class with required services.
    /// </summary>
    /// <param name="customerService">Service for customer-related operations.</param>
    /// <param name="userService">Service for user-related operations.</param>
    /// <param name="logger">Logger for controller events.</param>
    /// <exception cref="ArgumentNullException">Thrown if any required service is null.</exception>
    public CustomerController(
        ICustomerService customerService,
        IUserService userService,
        ILogger<CustomerController> logger)
    {
        _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var customerIdClaim = User.FindFirstValue("CustomerId");
            
        if (string.IsNullOrEmpty(customerIdClaim))
            return BadRequest(new { message = "User not linked to a customer" });
            
        var result = await _customerService.GetCustomerByIdAsync(customerIdClaim);
        if (!result.Success)
            return BadRequest(new { message = result.Errors });
            
        return Ok(result.Data);
    }
        
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(UpdateCustomerDto updateDto)
    {
        var customerIdClaim = User.FindFirstValue("CustomerId");
            
        if (string.IsNullOrEmpty(customerIdClaim))
            return BadRequest(new { message = "User not linked to a customer" });
            
        _logger.LogInformation("Customer updating their profile: {CustomerId}", customerIdClaim);
            
        var result = await _customerService.UpdateCustomerAsync(customerIdClaim, updateDto);
        if (!result.Success)
        {
            _logger.LogWarning("Customer profile update failed: {Error}", result.Errors);
            return BadRequest(new { message = result.Errors });
        }
            
        return Ok(result.Data);
    }
        
    /// <summary>
    /// Updates the profile of a specified customer by ID. Accessible only to users with Admin or SuperAdmin roles.
    /// </summary>
    /// <param name="id">The unique identifier of the customer to update.</param>
    /// <param name="updateDto">The updated customer profile data.</param>
    /// <returns>The updated customer data if successful; otherwise, a BadRequest with error details.</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> UpdateCustomer([FromRoute] string id, [FromBody] UpdateCustomerDto updateDto)
    {
        _logger.LogInformation("Admin updating customer ID: {CustomerId}", id);
            
        var result = await _customerService.UpdateCustomerAsync(id, updateDto);
            
        if (!result.Success)
        {
            _logger.LogWarning("Admin update customer failed - {Error}", result.Errors);
            return BadRequest(result.Errors);
        }
            
        return Ok(result.Data);
    }
        
    /// <summary>
    /// Links the authenticated user to an existing customer profile by customer ID.
    /// </summary>
    /// <param name="customerId">The ID of the customer profile to link.</param>
    /// <returns>
    /// Returns <see cref="OkObjectResult"/> with a success message if linking is successful; 
    /// <see cref="BadRequestObjectResult"/> if the user is already linked, the customer does not exist, or linking fails; 
    /// <see cref="UnauthorizedObjectResult"/> if the user is not authenticated.
    /// </returns>
    [HttpPost("link/{customerId}")]
    public async Task<IActionResult> LinkCustomerToUser(string customerId)
    {
        var success = User.TryGetUserId(out var userId);
        if (!success)
            return Unauthorized(new { message = "User not authenticated" });
            
        // Check if user already has a customer ID
        var userResult = await _userService.GetByIdAsync(userId);
        if (!userResult.Success)
            return BadRequest(new { message = userResult.Errors });
            
        if (userResult.Data.CustomerId != null)
            return BadRequest(new { message = "User already linked to a customer profile" });
            
        // Check if the customer exists
        var customerResult = await _customerService.GetCustomerByIdAsync(customerId);
        if (!customerResult.Success)
            return BadRequest(new { message = "Customer not found" });
            
        // Link user to customer
        var linkResult = await _customerService.LinkCustomerAsync(userId, customerId);
        if (!linkResult.Success)
            return BadRequest(new { message = linkResult.Errors });
            
        return Ok(new { message = "Customer profile linked successfully" });
    }
        
    [HttpGet("cards")]
    public async Task<IActionResult> GetLoyaltyCards()
    {
        var customerIdClaim = User.FindFirstValue("CustomerId");
            
        if (string.IsNullOrEmpty(customerIdClaim))
            return BadRequest(new { message = "User not linked to a customer" });
            
        // Implement this in your loyalty card service
        // var result = await _loyaltyCardService.GetCardsByCustomerIdAsync(customerIdClaim);
            
        // Placeholder
        return Ok(new { message = "Get customer loyalty cards endpoint" });
    }
}