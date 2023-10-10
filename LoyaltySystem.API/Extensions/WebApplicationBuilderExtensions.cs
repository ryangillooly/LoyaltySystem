using Amazon.DynamoDBv2;
using LoyaltySystem.Core.Interfaces;
using LoyaltySystem.Core.Settings;
using LoyaltySystem.Data.Clients;

namespace LoyaltySystem.API.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static void AddDynamoDb(this WebApplicationBuilder webApplicationBuilder)
    {
        if (webApplicationBuilder.Environment.IsDevelopment())
        {
            // When in development mode, use the local DynamoDB instance
            webApplicationBuilder.Services.AddSingleton<IAmazonDynamoDB>(sp =>
            {
                var config = new AmazonDynamoDBConfig
                {
                    //ServiceURL = "http://localhost:8000", // Default DynamoDB local URL
                    AuthenticationRegion = "eu-west-2" // DynamoDB local doesn't care about this, but it's required.
                };
                return new AmazonDynamoDBClient(config);
            });
        }
        else
        {
            webApplicationBuilder.Services.AddAWSService<IAmazonDynamoDB>();
        }

// Add DynamoDb Settings from AppSettings (Could move to class - AddDynamoSettings)
        var dynamoDbSettings = new DynamoDbSettings();
        webApplicationBuilder.Configuration.GetSection("DynamoDbSettings").Bind(dynamoDbSettings);
        webApplicationBuilder.Services.AddSingleton(dynamoDbSettings);

// Add Clients
        webApplicationBuilder.Services.AddScoped<IDynamoDbClient, DynamoDbClient>();
    }
}