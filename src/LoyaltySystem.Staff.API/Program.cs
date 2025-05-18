using LoyaltySystem.Shared.API.Configuration;

var builder = WebApplication.CreateBuilder(args);

Console.WriteLine("Env:" + builder.Environment.EnvironmentName );

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.AddSharedServices("Loyalty System Staff API");

var app = builder.Build();
app.UseSharedMiddleware();
app.Run(); 