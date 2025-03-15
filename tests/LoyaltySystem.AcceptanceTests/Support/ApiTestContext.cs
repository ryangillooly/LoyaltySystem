using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using LoyaltySystem.AcceptanceTests.DTOs;
using Reqnroll;

namespace LoyaltySystem.AcceptanceTests.Support;

/// <summary>
/// Maintains state between test steps and provides helper methods
/// </summary>
public class ApiTestContext
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ScenarioContext _scenarioContext;
    
    public string? JwtToken { get; set; }
    public string? UserId { get; set; }
    public string? LoyaltyCardId { get; set; }
    public string? LoyaltyProgramId { get; set; }
    public AuthResponseDto? AuthResponse { get; set; }
    public UserDto? UserDetails { get; set; }
    public HttpResponseMessage? LastResponse { get; set; }
    
    public ApiTestContext(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(TestConfiguration.ApiBaseUrl);
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }
    
    public void SetAuthToken(string token)
    {
        JwtToken = token;
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
    
    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
    {
        LastResponse = await _httpClient.SendAsync(request);
        return LastResponse;
    }
    
    public async Task<T?> ReadResponseAs<T>()
    {
        if (LastResponse == null)
        {
            throw new InvalidOperationException("No response available to read");
        }
        
        var content = await LastResponse.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content, _jsonOptions);
    }
    
    public StringContent CreateJsonContent<T>(T data)
    {
        var json = JsonSerializer.Serialize(data, _jsonOptions);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }
} 