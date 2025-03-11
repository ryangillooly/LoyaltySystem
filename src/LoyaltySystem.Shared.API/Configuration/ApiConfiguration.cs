using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Application.Services;
using LoyaltySystem.Domain.Repositories;
using LoyaltySystem.Infrastructure.Data;
using LoyaltySystem.Infrastructure.Events;
using LoyaltySystem.Infrastructure.Repositories;
using LoyaltySystem.Shared.API.Middleware;
using Microsoft.Extensions.Hosting;

namespace LoyaltySystem.Shared.API.Configuration
{
    public static class ApiConfiguration
    {
        public static void AddSharedServices(this WebApplicationBuilder builder, string apiTitle)
        {
            // Add controllers
            builder.Services.AddControllers();
            
            // Add API explorer
            builder.Services.AddEndpointsApiExplorer();
            
            // Configure Swagger
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = apiTitle, Version = "v1" });
                
                // Add JWT Authentication to Swagger
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
            });

            // Configure JWT Authentication
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
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]))
                };
            });

            // Add authorization policies
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
                options.AddPolicy("RequireBrandManagerRole", policy => policy.RequireRole("Admin", "BrandManager"));
                options.AddPolicy("RequireStoreManagerRole", policy => policy.RequireRole("Admin", "BrandManager", "StoreManager"));
                options.AddPolicy("RequireStaffRole", policy => policy.RequireRole("Admin", "BrandManager", "StoreManager", "Staff"));
            });

            // Add database connection
            builder.Services.AddScoped<IDatabaseConnection>(provider => 
                new PostgresConnection(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Add repositories
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<ILoyaltyCardRepository, LoyaltyCardRepository>();
            builder.Services.AddScoped<ILoyaltyProgramRepository, LoyaltyProgramRepository>();
            builder.Services.AddScoped<IBrandRepository, BrandRepository>();
            builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
            builder.Services.AddScoped<IStoreRepository, StoreRepository>();

            // Add Unit of Work
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Add application services
            builder.Services.AddScoped<LoyaltyCardService>();
            builder.Services.AddScoped<AuthService>();
            builder.Services.AddScoped<LoyaltyProgramService>();
            builder.Services.AddScoped<CustomerService>();
            builder.Services.AddScoped<StoreService>();
            builder.Services.AddScoped<BrandService>();

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
} 