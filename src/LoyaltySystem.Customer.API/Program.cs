using FluentValidation;
using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Application.Validation;
using LoyaltySystem.Shared.API.Configuration;

var builder = WebApplication.CreateBuilder(args);

Console.WriteLine("Env:" + builder.Environment.EnvironmentName );

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder
    .AddSharedServices("Loyalty System Customer API")
    .AddSocialAuth();

// Register FluentValidation validators
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestDtoValidator>();

var app = builder.Build();
app.UseSharedMiddleware();

app.Run(); 