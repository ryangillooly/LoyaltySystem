using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Application.Services;
using LoyaltySystem.Domain.Repositories;
using LoyaltySystem.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace LoyaltySystem.Shared.API.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Register repositories
            services.AddScoped<ILoyaltyCardRepository, LoyaltyCardRepository>();
            services.AddScoped<ILoyaltyProgramRepository, LoyaltyProgramRepository>();
            services.AddScoped<ILoyaltyRewardsRepository, LoyaltyRewardsRepository>();

            // Register services
            services.AddScoped<ILoyaltyCardService, LoyaltyCardService>();
            services.AddScoped<ILoyaltyProgramService, LoyaltyProgramService>();
            services.AddScoped<ILoyaltyRewardsService, LoyaltyRewardsService>();

            return services;
        }
    }
} 