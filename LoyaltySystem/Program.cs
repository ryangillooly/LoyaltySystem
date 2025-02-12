using LoyaltySystem.Repositories;
using LoyaltySystem.Services;

namespace LoyaltySystem;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Read from appsettings.json for "DefaultConnection" and "Jwt:SecretKey" etc.
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        
        builder.Services.AddScoped<IBusinessRepository, BusinessRepository>();
        builder.Services.AddScoped<IBusinessService, BusinessService>();
        
        builder.Services.AddScoped<ILoyaltyCardRepository, LoyaltyCardRepository>();
        builder.Services.AddScoped<ILoyaltyCardService, LoyaltyCardService>();

        builder.Services.AddScoped<IMembersRepository, MembersRepository>();
        builder.Services.AddScoped<IMembersService, MembersService>();
        
        builder.Services.AddScoped<IPromotionsRepository, PromotionsRepository>();
        builder.Services.AddScoped<IPromotionsService, PromotionsService>();
        
        builder.Services.AddScoped<IRedemptionRepository, RedemptionRepository>();
        builder.Services.AddScoped<IRedemptionService, RedemptionService>();
        
        builder.Services.AddScoped<IRewardsRepository, RewardsRepository>();
        builder.Services.AddScoped<IRewardsService, RewardsService>();
        
        builder.Services.AddScoped<IStampsRepository, StampsRepository>();
        builder.Services.AddScoped<IStampsService, StampsService>();
        
        builder.Services.AddScoped<IStoresRepository, StoresRepository>();
        builder.Services.AddScoped<IStoresService, StoresService>();

        builder.Services.AddScoped<IFraudSettingsRepository, FraudSettingsRepository>();
        builder.Services.AddScoped<IFraudSettingsService, FraudSettingsService>();

        
        builder.Services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();
        builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
        
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<ITokenService, TokenService>();

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Optional: app.UseAuthentication(); app.UseAuthorization();

        app.UseSwagger();
        app.UseSwaggerUI();

        app.MapControllers();
        app.Run();
    }
}