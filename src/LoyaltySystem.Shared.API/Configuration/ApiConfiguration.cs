using LoyaltySystem.Admin.API.Settings;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Application.Services;
using LoyaltySystem.Domain.Interfaces;
using LoyaltySystem.Domain.Repositories;
using LoyaltySystem.Infrastructure.Data;
using LoyaltySystem.Infrastructure.Events;
using LoyaltySystem.Infrastructure.Repositories;
using LoyaltySystem.Shared.API.Middleware;
using Microsoft.Extensions.Hosting;
using LoyaltySystem.Shared.API.Settings;
using LoyaltySystem.Shared.API.Services;
using System.Text.Json.Serialization;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Infrastructure.Json;
using LoyaltySystem.Shared.API.Serialization;
using LoyaltySystem.Shared.API.ModelBinding;
using LoyaltySystem.Shared.API.Attributes;

namespace LoyaltySystem.Shared.API.Configuration;

public static class ApiConfiguration
{
    public static void AddSharedServices(this WebApplicationBuilder builder, string apiTitle)
    {
        var postgresSection = builder.Configuration.GetSection("PostgreSQL");
        var postgresqlSettings = postgresSection.Get<PostgresqlSettings>();
        
        // Configure JWT settings
        var jwtSection = builder.Configuration.GetSection("JwtSettings");
        builder.Services.Configure<JwtSettings>(jwtSection);
        var jwtSettings = jwtSection.Get<JwtSettings>();
        
        // Configure controllers with JSON options for EntityId serialization
        builder.Services.AddControllers(options => 
            {
                // Add model binder provider for EntityId types
                options.ModelBinderProviders.Insert(0, new ModelBinding.EntityIdModelBinderProvider());
            })
            .AddJsonOptions(options => 
            {
                // Register converters for all EntityId types
                options.JsonSerializerOptions.Converters.Add(new OperatingHoursConverter());
                options.JsonSerializerOptions.Converters.Add(new EntityIdJsonConverter<UserId>());
                options.JsonSerializerOptions.Converters.Add(new EntityIdJsonConverter<CustomerId>());
                options.JsonSerializerOptions.Converters.Add(new EntityIdJsonConverter<BrandId>());
                options.JsonSerializerOptions.Converters.Add(new EntityIdJsonConverter<StoreId>());
                options.JsonSerializerOptions.Converters.Add(new EntityIdJsonConverter<LoyaltyProgramId>());
                options.JsonSerializerOptions.Converters.Add(new EntityIdJsonConverter<LoyaltyCardId>());
                options.JsonSerializerOptions.Converters.Add(new EntityIdJsonConverter<RewardId>());
                options.JsonSerializerOptions.Converters.Add(new EntityIdJsonConverter<TransactionId>());
                options.JsonSerializerOptions.Converters.Add(new EntityIdJsonConverter<UserRoleId>());
                
                // Use enum string names instead of integer values
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
            
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.Configure<PostgresqlSettings>(postgresSection);
        
        // Register JWT service
        builder.Services.AddSingleton<IJwtService, JwtService>();
        
        builder.Services.AddSwaggerGen(c =>
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
                    new string[] {}
                }
            });
            
            // Add the EntityId format documentation filter
            c.OperationFilter<Attributes.EntityIdFormatOperationFilter>();
        });
        
        // Add JWT Authentication
        builder.Services.AddAuthentication(options =>
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
        
        // Add Policies
        builder.Services
            .AddAuthorizationBuilder()
            .AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"))
            .AddPolicy("RequireBrandManagerRole", policy => policy.RequireRole("Admin", "BrandManager"))
            .AddPolicy("RequireStoreManagerRole", policy => policy.RequireRole("Admin", "BrandManager", "StoreManager"))
            .AddPolicy("RequireStaffRole", policy => policy.RequireRole("Admin", "BrandManager", "StoreManager", "Staff"));
        
        ArgumentException.ThrowIfNullOrEmpty(postgresqlSettings?.ConnectionString);
        builder.Services.AddScoped<IDatabaseConnection>(_ => new PostgresConnection(postgresqlSettings.ConnectionString));

        // Add repositories
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<ILoyaltyCardRepository, LoyaltyCardRepository>();
        builder.Services.AddScoped<ILoyaltyProgramRepository, LoyaltyProgramRepository>();
        builder.Services.AddScoped<IBusinessRepository, BusinessRepository>();
        builder.Services.AddScoped<IBrandRepository, BrandRepository>();
        builder.Services.AddScoped<IStoreRepository, StoreRepository>();
        builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();

        // Add Unit of Work
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Add application services
        builder.Services.AddScoped<AuthService>();
        builder.Services.AddScoped<LoyaltyCardService>();
        builder.Services.AddScoped<LoyaltyProgramService>();
        builder.Services.AddScoped<BusinessService>();
        builder.Services.AddScoped<BrandService>();
        builder.Services.AddScoped<StoreService>();
        builder.Services.AddScoped<CustomerService>();

        // Add event handling
        builder.Services.AddScoped<IEventPublisher, ConsoleEventPublisher>();
    }

    public static void UseSharedMiddleware(this WebApplication app)
    {
        // Configure the HTTP request pipeline
        if (app.Environment.IsProduction() == false)
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            
            // Initialize the entity ID utility for development mode
            LoyaltySystem.Shared.API.Utilities.EntityIdUtility.Initialize();
        }

        app.UseHttpsRedirection();
        
        // Add request logging middleware
        app.UseMiddleware<RequestLoggingMiddleware>();

        // Add authentication middleware
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
    }
}