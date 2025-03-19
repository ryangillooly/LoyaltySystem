using LoyaltySystem.Shared.API.Configuration;

var builder = WebApplication.CreateBuilder(args);

Console.WriteLine("Env:" + builder.Environment.EnvironmentName );

// Explicitly configure environment-specific appsettings
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.AddSharedServices("Loyalty System Customer API");

var app = builder.Build();
app.UseSharedMiddleware();

app.Run(); 