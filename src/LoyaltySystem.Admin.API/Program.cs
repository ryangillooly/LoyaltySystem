using LoyaltySystem.Shared.API.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add shared services with Admin-specific configuration
builder.AddSharedServices("Loyalty System Admin API");

// Add Admin-specific services if needed

var app = builder.Build();

// Use shared middleware
app.UseSharedMiddleware();

app.Run(); 