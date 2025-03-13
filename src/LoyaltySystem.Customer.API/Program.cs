using LoyaltySystem.Shared.API.Configuration;
using LoyaltySystem.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add shared services with Customer-specific configuration
builder.AddSharedServices("Loyalty System Customer API");

// Add Customer-specific services if needed

var app = builder.Build();

// Use shared middleware
app.UseSharedMiddleware();

app.Run(); 