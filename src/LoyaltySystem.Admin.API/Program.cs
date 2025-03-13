using LoyaltySystem.Shared.API.Configuration;
using LoyaltySystem.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.AddSharedServices("Loyalty System Admin API");

var app = builder.Build();
app.UseSharedMiddleware();

await app.RunAsync(); 