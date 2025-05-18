using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Shared.API.Controllers;
using Microsoft.AspNetCore.Mvc;
using ILogger = Serilog.ILogger;

namespace LoyaltySystem.Admin.API.Controllers;

[ApiController]
[Route("api/account")]
public class AccountController : BaseAccountController 
{
    public AccountController(IAccountService accountService, ILogger logger) 
    : base(accountService, logger) 
    { }
}
