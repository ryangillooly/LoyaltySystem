using Microsoft.Extensions.Configuration;

namespace LoyaltySystem.AcceptanceTests.Support;

public static class TestConfiguration
{
    private static IConfiguration? _configuration;
    
    public static IConfiguration Configuration
    {
        get
        {
            if (_configuration == null)
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("testsettings.json", optional: true, reloadOnChange: true)
                    .AddJsonFile($"testsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", optional: true)
                    .AddEnvironmentVariables();
                
                _configuration = builder.Build();
            }
            
            return _configuration;
        }
    }
    
    public static string ApiBaseUrl => Configuration["ApiBaseUrl"] ?? "http://localhost:5000";
    
    public static string GetApiEndpoint(string endpoint)
    {
        return $"{ApiBaseUrl.TrimEnd('/')}/{endpoint.TrimStart('/')}";
    }
} 