using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Shared.API.Controllers;
using Microsoft.AspNetCore.Mvc;
using ILogger = Serilog.ILogger;

namespace LoyaltySystem.Admin.API.Controllers;

[ApiController]
[Route("api/account")]
public class AccountController : BaseAccountController 
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AccountController"/> class with the specified account service and logger.
    /// </summary>
    public AccountController(IAccountService accountService, ILogger logger) 
    : base(accountService, logger) 
    { }
}
