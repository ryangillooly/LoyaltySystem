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
using FluentValidation;
using FluentValidation.AspNetCore;
using LoyaltySystem.Application.Validation;
using System.Text.Json;

namespace LoyaltySystem.Shared.API.Configuration;

public static class ApiConfiguration
{
    public static IServiceCollection AddSharedServices(this WebApplicationBuilder builder, string apiTitle)
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
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
                
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
            .AddFluentValidationAutoValidation()
            .AddValidatorsFromAssemblyContaining<LoginRequestDtoValidator>()
            .AddEndpointsApiExplorer()
            .AddSingleton<IJwtService, JwtService>()
            .AddJwtAuthentication(builder.Configuration)
            .AddJwtAuthorisation()
            .AddScoped<IDatabaseConnection>(_ => new PostgresConnection(postgresqlSettings.ConnectionString))
            .AddAuthServices()
            .AddRepositories()
            .AddServices(builder.Configuration)
            .AddScoped<IUnitOfWork, UnitOfWork>()
            .AddScoped<IEventPublisher, ConsoleEventPublisher>()
            .AddSwagger(apiTitle);

        return builder.Services;
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