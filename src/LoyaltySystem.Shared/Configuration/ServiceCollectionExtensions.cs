using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Application.Services;
using LoyaltySystem.Domain.Repositories;
using LoyaltySystem.Infrastructure.Repositories;
using LoyaltySystem.Shared.API.Attributes;
using LoyaltySystem.Shared.API.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;

namespace LoyaltySystem.Shared.API.Configuration;

public static class ServiceCollectionExtensions 
{
    public static void AddSerilog(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, services, loggerConfiguration) => loggerConfiguration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console());
    }

    public static IServiceCollection AddSwagger(this IServiceCollection services, string apiTitle)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = apiTitle, Version = "v1" });
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            // Add the EntityId format documentation filter
            c.OperationFilter<EntityIdFormatOperationFilter>();
        });

        return services;
    }
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services
            .AddScoped<IAuthService, AuthService>()
            .AddScoped<ILoyaltyCardService, LoyaltyCardService>()
            .AddScoped<ILoyaltyRewardsService, LoyaltyRewardsService>()
            .AddScoped<ILoyaltyProgramService, LoyaltyProgramService>()
            .AddScoped<IBusinessService, BusinessService>()
            .AddScoped<IBrandService, BrandService>()
            .AddScoped<IStoreService, StoreService>()
            .AddScoped<ICustomerService, CustomerService>();

        return services;
    }
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services
            .AddScoped<IUserRepository, UserRepository>()
            .AddScoped<ILoyaltyCardRepository, LoyaltyCardRepository>()
            .AddScoped<ILoyaltyRewardsRepository, LoyaltyRewardsRepository>()
            .AddScoped<ILoyaltyProgramRepository, LoyaltyProgramRepository>()
            .AddScoped<IBusinessRepository, BusinessRepository>()
            .AddScoped<IBrandRepository, BrandRepository>()
            .AddScoped<IStoreRepository, StoreRepository>()
            .AddScoped<ICustomerRepository, CustomerRepository>();

        return services;
    }
    public static IServiceCollection AddJwtAuthorisation(this IServiceCollection services)
    {
        services
            .AddAuthorizationBuilder()
            .AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"))
            .AddPolicy("RequireBrandManagerRole", policy => policy.RequireRole("Admin", "BrandManager"))
            .AddPolicy("RequireStoreManagerRole", policy => policy.RequireRole("Admin", "BrandManager", "StoreManager"))
            .AddPolicy("RequireStaffRole", policy => policy.RequireRole("Admin", "BrandManager", "StoreManager", "Staff"));

        return services;
    }
    
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure JWT settings
        var jwtSection = configuration.GetSection("JwtSettings");
        services.Configure<JwtSettings>(jwtSection);
        var jwtSettings = jwtSection.Get<JwtSettings>();
        
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                    ClockSkew = TimeSpan.Zero // Reduce clock skew for more precise expiration
                };
        
                // Optional: Handle JWT events
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Add("Token-Expired", "true");
                        }
                        return Task.CompletedTask;
                    }
                };
            });

        return services;
    }
}
