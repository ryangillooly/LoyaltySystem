using LoyaltySystem.API.Extensions;
using LoyaltySystem.Core.Interfaces;
using LoyaltySystem.Core.Utilities;
using LoyaltySystem.Data.Repositories;
using LoyaltySystem.Services;

var builder = WebApplication.CreateBuilder(args);

// Service Configurations
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "MyCORS",
        builderCors =>
        {
            builderCors
                .WithOrigins("http://localhost:3000")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

// Add DynamoDb Services
builder.AddDynamoDb();

// Add Repositories
builder.Services.AddScoped<ILoyaltyCardRepository, LoyaltyCardRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IBusinessRepository, BusinessRepository>();
builder.Services.AddScoped<IAuditRepository, AuditRepository>();
builder.Services.AddScoped<IEmailRepository, EmailRepository>();

// Add Services
builder.Services.AddScoped<IBusinessService, BusinessService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ILoyaltyCardService, LoyaltyCardService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// Add Mappers
builder.Services.AddSingleton<IDynamoDbMapper, DynamoDbMapper>();

var app = builder.Build();

// Configure the HTTP request pipeline.

// For development environment
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();  // Use this for detailed error pages during development.
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();  // UseRouting should be before CORS and Authorization

app.UseCors("MyCORS");  // UseCors should be after UseRouting and before UseAuthorization

app.UseAuthorization();  // UseAuthorization should be after UseCors

app.MapControllers();  // MapControllers should come after all other middlewares

app.Run();  // To run the application
