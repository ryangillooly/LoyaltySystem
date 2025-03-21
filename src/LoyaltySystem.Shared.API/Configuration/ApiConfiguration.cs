using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Domain.Interfaces;
using LoyaltySystem.Infrastructure.Data;
using LoyaltySystem.Infrastructure.Events;
using LoyaltySystem.Shared.API.Middleware;
using Microsoft.Extensions.Hosting;
using LoyaltySystem.Shared.API.Services;
using System.Text.Json.Serialization;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Settings;
using LoyaltySystem.Infrastructure.Json;
using LoyaltySystem.Shared.API.Serialization;
using LoyaltySystem.Shared.API.ModelBinding;
using Serilog;

namespace LoyaltySystem.Shared.API.Configuration;

public static class ApiConfiguration
{
    public static void AddSharedServices(this WebApplicationBuilder builder, string apiTitle)
    {
        var postgresSection = builder.Configuration.GetSection("PostgreSQL");
        builder.Services.Configure<PostgresqlSettings>(postgresSection);
        var postgresqlSettings = postgresSection.Get<PostgresqlSettings>();
        ArgumentException.ThrowIfNullOrEmpty(postgresqlSettings?.ConnectionString);
     
        builder.AddSerilog();
        builder.Services
            .AddControllers(options => 
            {
                options.ModelBinderProviders.Insert(0, new EntityIdModelBinderProvider());
            })
            .AddJsonOptions(options => 
            {
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
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
            
        builder.Services
            .AddEndpointsApiExplorer()
            .AddSingleton<IJwtService, JwtService>()
            .AddJwtAuthentication(builder.Configuration)
            .AddJwtAuthorisation()
            .AddScoped<IDatabaseConnection>(_ => new PostgresConnection(postgresqlSettings.ConnectionString))
            .AddRepositories()
            .AddServices()
            .AddScoped<IUnitOfWork, UnitOfWork>()
            .AddScoped<IEventPublisher, ConsoleEventPublisher>()
            .AddSwagger(apiTitle);
    }

    public static void UseSharedMiddleware(this WebApplication application)
    {
        if (!application.Environment.IsProduction())
        {
            application
                .UseSwagger()
                .UseSwaggerUI();
            
            Utilities.EntityIdUtility.Initialize();
        }

        application
            .UseHttpsRedirection()
            .UseMiddleware<RequestLoggingMiddleware>()
            .UseAuthentication()
            .UseAuthorization();

        application.MapControllers();
    }
}