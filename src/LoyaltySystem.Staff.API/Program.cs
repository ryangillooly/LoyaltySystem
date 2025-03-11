using LoyaltySystem.Shared.API.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add shared services with Staff-specific configuration
builder.AddSharedServices("Loyalty System Staff API");

// Add Staff-specific services if needed

var app = builder.Build();

// Use shared middleware
app.UseSharedMiddleware();

app.Run(); 