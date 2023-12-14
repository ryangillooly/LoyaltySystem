using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using LoyaltySystem.Core.Interfaces;
using LoyaltySystem.Core.Models;
using LoyaltySystem.Core.Settings;
using static LoyaltySystem.Core.Models.Constants;

namespace LoyaltySystem.Data.Repositories;

public class EmailRepository : IEmailRepository
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly DynamoDbSettings _dynamoDbSettings;

    public EmailRepository(IAmazonDynamoDB dynamoDb, DynamoDbSettings dynamoDbSettings)
        => (_dynamoDb, _dynamoDbSettings) = (dynamoDb, dynamoDbSettings);
    
    public async Task<bool> DoesEmailExistAsync(string email)
    {
        var request = new QueryRequest
        {
            TableName = _dynamoDbSettings.TableName,
            IndexName = _dynamoDbSettings.EmailGsi,                           // Use GSI for querying
            KeyConditionExpression = "Email = :emailValue", // Assuming your GSI PK is named "Email"
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                {":emailValue", new AttributeValue { S = email }}
            },
            Limit = 1                                       // We only need to know if at least one item exists
        };

        var response = await _dynamoDb.QueryAsync(request);
        return response.Count > 0; // If count > 0, email exists
    }
    
    public async Task SendEmailAsync(EmailInfo model)
    {
        var client = new AmazonSimpleEmailServiceClient(RegionEndpoint.EUWest1); // Change the region if necessary

        var sendRequest = new SendEmailRequest
        {
            Source      = model.FromEmail,
            Destination = new Destination { ToAddresses = { model.ToEmail }},
            Message     = new Message
            {
                Subject = new Content(model.Subject),
                Body    = new Body { Text = new Content { Charset = "UTF-8", Data = model.Body }}
            }
        };

        try
        {
            Console.WriteLine("Sending email using Amazon SES...");
            var response = await client.SendEmailAsync(sendRequest);
            Console.WriteLine("Email sent successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error sending email: " + ex.Message);
            throw;
        }
    }
}