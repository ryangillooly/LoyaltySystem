using System;
using LoyaltySystem.Domain.Repositories;
using LoyaltySystem.Infrastructure.Data.TypeHandlers;
using LoyaltySystem.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LoyaltySystem.Infrastructure.Data.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Initialize type handlers
            TypeHandlerConfig.Initialize();
            
            // Register database connection
            services.AddSingleton<IDatabaseConnection>(provider => 
                new PostgresConnection(configuration.GetConnectionString("DefaultConnection")));
            
            // Register repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ILoyaltyCardRepository, LoyaltyCardRepository>();
            services.AddScoped<ILoyaltyProgramRepository, LoyaltyProgramRepository>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<IStoreRepository, StoreRepository>();
            services.AddScoped<IBrandRepository, BrandRepository>();
            
            return services;
        }
    }
} 