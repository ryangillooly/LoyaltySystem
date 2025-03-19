using System.Net.Http.Json;
using FluentAssertions;
using LoyaltySystem.AcceptanceTests.Support;
using LoyaltySystem.AcceptanceTests.DTOs;
using Reqnroll;

namespace LoyaltySystem.AcceptanceTests.StepDefinitions;

[Binding]
public class AuthenticationSteps
{
    private readonly ApiTestContext _context;
    private readonly ScenarioContext _scenarioContext;
    
    public AuthenticationSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
        _context = new ApiTestContext(scenarioContext);
    }
    
    [When(@"I register a new account with email ""([^""]*)"" and password ""([^""]*)""")]
    public async Task WhenIRegisterANewAccountWithEmailAndPassword(string email, string password)
    {
        var registerDto = new RegisterUserDto
        {
            Email = email,
            Password = password,
            ConfirmPassword = password,
            FirstName = "Test",
            LastName = "User"
        };
        
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/register");
        request.Content = _context.CreateJsonContent(registerDto);
        
        await _context.SendAsync(request);
    }
    
    [Given(@"I have registered with email ""([^""]*)"" and password ""([^""]*)""")]
    public async Task GivenIHaveRegisteredWithEmailAndPassword(string email, string password)
    {
        // Ensure the user exists - either create or verify
        var registerDto = new RegisterUserDto
        {
            Email = email,
            Password = password,
            ConfirmPassword = password,
            FirstName = "Test",
            LastName = "User"
        };
        
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/register");
        request.Content = _context.CreateJsonContent(registerDto);
        
        var response = await _context.SendAsync(request);
        
        // If user already exists, that's fine for this step
        if (response.IsSuccessStatusCode)
        {
            _context.UserDetails = await _context.ReadResponseAs<UserDto>();
        }
    }
    
    [When(@"I login with email ""([^""]*)"" and password ""([^""]*)""")]
    public async Task WhenILoginWithEmailAndPassword(string email, string password)
    {
        var loginDto = new LoginDto
        {
            Email = email,
            Password = password
        };
        
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login");
        request.Content = _context.CreateJsonContent(loginDto);
        
        await _context.SendAsync(request);
    }
    
    [Then(@"I should receive a successful registration response")]
    public void ThenIShouldReceiveASuccessfulRegistrationResponse()
    {
        _context.LastResponse.Should().NotBeNull();
        _context.LastResponse!.IsSuccessStatusCode.Should().BeTrue();
        _context.LastResponse!.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }
    
    [Then(@"the response should contain user details for ""([^""]*)""")]
    public async Task ThenTheResponseShouldContainUserDetailsFor(string email)
    {
        var userDetails = await _context.ReadResponseAs<UserDto>();
        
        userDetails.Should().NotBeNull();
        userDetails!.Email.Should().Be(email);
        
        _context.UserDetails = userDetails;
        _context.UserId = userDetails.Id;
    }
    
    [Then(@"I should receive a successful login response")]
    public void ThenIShouldReceiveASuccessfulLoginResponse()
    {
        _context.LastResponse.Should().NotBeNull();
        _context.LastResponse!.IsSuccessStatusCode.Should().BeTrue();
    }
    
    [Then(@"the response should contain a valid JWT token")]
    public async Task ThenTheResponseShouldContainAValidJWTToken()
    {
        var authResponse = await _context.ReadResponseAs<AuthResponseDto>();
        
        authResponse.Should().NotBeNull();
        authResponse!.Token.Should().NotBeNullOrEmpty();
        
        // Store the token for subsequent requests
        _context.AuthResponse = authResponse;
        _context.JwtToken = authResponse.Token;
        _context.SetAuthToken(authResponse.Token);
    }
    
    [Given(@"I am authenticated with a valid JWT token")]
    public async Task GivenIAmAuthenticatedWithAValidJWTToken()
    {
        if (string.IsNullOrEmpty(_context.JwtToken))
        {
            // Login to get a token if we don't have one
            await GivenIHaveRegisteredWithEmailAndPassword("test.user@example.com", "Test@123!");
            await WhenILoginWithEmailAndPassword("test.user@example.com", "Test@123!");
            await ThenTheResponseShouldContainAValidJWTToken();
        }
    }
} 