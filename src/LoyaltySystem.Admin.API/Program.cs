using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Shared.API.Configuration;

var builder = WebApplication.CreateBuilder(args);

Console.WriteLine("Env:" + builder.Environment.EnvironmentName );

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder
    .AddSharedServices("Loyalty System Admin API")
    .AddSocialAuth();

var app = builder.Build();
app.UseSharedMiddleware();

await app.RunAsync(); 