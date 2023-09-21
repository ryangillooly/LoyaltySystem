using Amazon.DynamoDBv2;
using LoyaltySystem.Core.Interfaces;
using LoyaltySystem.Core.Models;
using LoyaltySystem.Core.Settings;
using LoyaltySystem.Data.Repositories;
using LoyaltySystem.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add AWS Services
builder.Services.AddAWSService<IAmazonDynamoDB>();

// Add Repositories
builder.Services.AddScoped<ILoyaltyCardRepository, LoyaltyCardRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRepository<Business>, BusinessRepository>();
builder.Services.AddScoped<IAuditRepository, AuditRepository>();

// Add Services
builder.Services.AddScoped<IBusinessService, BusinessService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ILoyaltyCardService, LoyaltyCardService>();
builder.Services.AddScoped<IAuditService, AuditService>();

// Add DynamoDb Settings from AppSettings (Could move to class - AddDynamoSettings)
var dynamoDbSettings = new DynamoDbSettings();
builder.Configuration.GetSection("DynamoDbSettings").Bind(dynamoDbSettings);
builder.Services.AddSingleton(dynamoDbSettings);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();