using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using LoyaltySystem.Core.Exceptions;
using LoyaltySystem.Core.Interfaces;
using LoyaltySystem.Core.Models;
using LoyaltySystem.Core.Settings;

namespace LoyaltySystem.Data.Clients;

public class DynamoDbClient : IDynamoDbClient
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly DynamoDbSettings _dynamoDbSettings;

    public DynamoDbClient(IAmazonDynamoDB dynamoDb, DynamoDbSettings dynamoDbSettings) =>
        (_dynamoDb, _dynamoDbSettings) = (dynamoDb, dynamoDbSettings);

    // Users
    public async Task<GetItemResponse?> GetUserAsync(Guid userId)
    {
        var request = new GetItemRequest
        {
            TableName = _dynamoDbSettings.TableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "PK", new AttributeValue { S = $"User#{userId}" }},
                { "SK", new AttributeValue { S = "Meta#UserInfo"   }}
            }
        };

        var response = await _dynamoDb.GetItemAsync(request);

        if (response.Item == null || !response.IsItemSet)
            return null; // TODO: Change to Custom Exception

        return response;
    }
    
    // Business
    public async Task<GetItemResponse?> GetBusinessAsync(Guid businessId)
    {
        var request = new GetItemRequest
        {
            TableName = _dynamoDbSettings.TableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "PK", new AttributeValue { S = $"Business#{businessId}" }},
                { "SK", new AttributeValue { S = "Meta#BusinessInfo"  }}
            }
        };

        var response = await _dynamoDb.GetItemAsync(request);

        if (response.Item == null || !response.IsItemSet)
            return null; // TODO: Change to Custom Exception

        return response;
    }
    
    // Business Users
    public async Task<QueryResponse?> GetBusinessPermissions(Guid businessId)
    {
        var request = new QueryRequest
        {
            TableName = _dynamoDbSettings.TableName,
            IndexName = _dynamoDbSettings.BusinessUserListGsi,
            KeyConditionExpression = "#PK = :PKValue AND begins_with(#SK, :SKValue)",  // Use placeholders
            ExpressionAttributeNames = new Dictionary<string, string>
            {
                { "#PK", "BusinessUserList-PK" },   // Map to the correct attribute names
                { "#SK", "BusinessUserList-SK" }
            },
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":PKValue", new AttributeValue { S = $"{businessId}" }},
                { ":SKValue", new AttributeValue { S = "Permission#User" }}
            }
        };

        var response = await _dynamoDb.QueryAsync(request);

        if (response.Items == null || response.Count == 0)
            return null;

        return response;
    }
    public async Task<GetItemResponse?> GetBusinessUsersPermissions(Guid businessId, Guid userId)
    {
        var request = new GetItemRequest
        {
            TableName = _dynamoDbSettings.TableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "PK", new AttributeValue { S = $"User#{userId}" }},
                { "SK", new AttributeValue { S = $"Permission#Business#{businessId}"   }}
            }
        };
        
        var response = await _dynamoDb.GetItemAsync(request);

        if (response.Item is null || !response.IsItemSet)
            throw new BusinessUserPermissionNotFoundException(userId, businessId);

        return response;
    }
    public async Task DeleteBusinessUsersPermissions(Guid businessId, List<Guid> userIdList)
    {
        foreach (var userId in userIdList)
        {
            var deleteRequest = new DeleteItemRequest
            {
                TableName = _dynamoDbSettings.TableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "PK", new AttributeValue { S = $"User#{userId}" } },
                    { "SK", new AttributeValue { S = $"Permission#Business#{businessId}" } }
                }
            };

            await _dynamoDb.DeleteItemAsync(deleteRequest); // Replace with batching
        }
    }
    
    
    // Business Campaigns
    public async Task<GetItemResponse?> GetCampaignAsync(Guid businessId, Guid campaignId)
    {
        var request = new GetItemRequest
        {
            TableName = _dynamoDbSettings.TableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "PK", new AttributeValue { S = $"Business#{businessId}" }},
                { "SK", new AttributeValue { S = $"Campaign#{campaignId}" }}
            }
        };

        var response = await _dynamoDb.GetItemAsync(request);

        if (response.Item == null || !response.IsItemSet)
            throw new CampaignNotFoundException(campaignId, businessId);

        return response;
    }
    public async Task<QueryResponse?> GetAllCampaignsAsync(Guid businessId)
    {
        var request = new QueryRequest
        {
            TableName = _dynamoDbSettings.TableName,
            KeyConditionExpression = "PK = :businessId AND begins_with(SK, :campaignPrefix)",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                {":businessId",     new AttributeValue { S = $"Business#{businessId.ToString()}" }},
                {":campaignPrefix", new AttributeValue { S = "Campaign#" }}
            }
        };

        var response = await _dynamoDb.QueryAsync(request);

        if (response.Items.Count is 0 || response.Items is null)
            return null;

        return response;
    }
    
    
    // Business Loyalty Cards
    public async Task DeleteCampaignAsync(Guid businessId, List<Guid> campaignIds)
    {
        // Create batch delete requests
        var batchRequests = new List<WriteRequest>();
        foreach (var campaignId in campaignIds)
        {
            batchRequests.Add(new WriteRequest
            {
                DeleteRequest = new DeleteRequest
                {
                    Key = new Dictionary<string, AttributeValue>
                    {
                        { "PK", new AttributeValue { S = $"Business#{businessId}" } },
                        { "SK", new AttributeValue { S = $"Campaign#{campaignId}" } }
                    }
                }
            });
        }

        // Split requests into chunks of 25, which is the max for a single BatchWriteItem request
        var chunkedBatchRequests = new List<List<WriteRequest>>();
        for (var i = 0; i < batchRequests.Count; i += 25)
        {
            chunkedBatchRequests.Add(batchRequests.GetRange(i, Math.Min(25, batchRequests.Count - i)));
        }

        // Perform the BatchWriteItem for each chunk
        foreach (var chunk in chunkedBatchRequests)
        {
            var batchWriteItemRequest = new BatchWriteItemRequest
            {
                RequestItems = new Dictionary<string, List<WriteRequest>>
                {
                    {_dynamoDbSettings.TableName, chunk}
                }
            };

            try
            {
                await _dynamoDb.BatchWriteItemAsync(batchWriteItemRequest);
            }
            catch (ConditionalCheckFailedException)
            {
                throw new Exception($"Failed to delete items with PK - Business#{businessId} due to condition check");
            }
        }
    }


    // Common
    public async Task BatchWriteRecordsAsync(List<Dictionary<string, AttributeValue>> items)
    {
        var writeRequests = items.Select(item => 
            new WriteRequest 
            {
                PutRequest = new PutRequest { Item = item }
            }).ToList();

        var request = new BatchWriteItemRequest
        {
            RequestItems = new Dictionary<string, List<WriteRequest>>
            {
                { _dynamoDbSettings.TableName, writeRequests }
            }
        };

        try
        {
            await _dynamoDb.BatchWriteItemAsync(request);
        }
        catch (Exception ex)  // You can add more specific exceptions here if needed
        {
            // Handle or throw exceptions as per your application's requirements
            throw new Exception("Failed to batch write items", ex);
        }
    }
    public async Task TransactWriteRecordsAsync(List<Dictionary<string, AttributeValue>> items)
    {
        var chunks = ChunkList(items, 25);

        foreach (var chunk in chunks)
        {
            var transactItems = chunk.Select(item => 
                new TransactWriteItem
                {
                    Put = new Put
                    {
                        TableName = _dynamoDbSettings.TableName,
                        Item = item
                    }
                }).ToList();

            var request = new TransactWriteItemsRequest
            {
                TransactItems = transactItems
            };

            try
            {
                await _dynamoDb.TransactWriteItemsAsync(request);
            }
            catch (Exception ex)
            {
                // Handle or throw exceptions as per your application's requirements.
                // Note: you might want to catch the specific TransactionCanceledException 
                // to get details about which item caused the transaction to fail.
                throw new Exception("Failed to transact write items", ex);
            }
        }
    }

    // Helper method to break a list into chunks
    private static IEnumerable<List<T>> ChunkList<T>(List<T> list, int chunkSize)
    {
        for (int i = 0; i < list.Count; i += chunkSize)
        {
            yield return list.GetRange(i, Math.Min(chunkSize, list.Count - i));
        }
    }

    
    public async Task DeleteItemsWithPkAsync(string pk)
    {
        try
        {
            await DeleteItemsWithPK(pk);
        }
        catch (ConditionalCheckFailedException)
        {
            throw new Exception($"Failed to delete item with PK - {pk} due to condition check");
        }
    }
    public async Task UpdateRecordAsync(Dictionary<string, AttributeValue> item, string? conditionExpression)
    {
        var request = new PutItemRequest
        {
            TableName = _dynamoDbSettings.TableName,
            Item = item
        };

        await _dynamoDb.PutItemAsync(request);
    }
    public async Task<List<string>> GetAllSortKeysForPartitionKey(string PK)
    {
        var queryRequest = new QueryRequest
        {
            TableName = _dynamoDbSettings.TableName,
            KeyConditionExpression = "PK = :PK",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                {":PK", new AttributeValue {S = PK}}
            }
        };

        var response = await _dynamoDb.QueryAsync(queryRequest);
    
        // Extract sort keys (SKs) from the response
        return response.Items.Select(item => item["SK"].S).ToList();
    }
    public async Task DeleteItemsWithPK(string PK)
    {
        var sortKeys = await GetAllSortKeysForPartitionKey(PK);

        // Create batch delete requests
        var batchRequests = new List<WriteRequest>();
        foreach (var SK in sortKeys)
        {
            batchRequests.Add(new WriteRequest
            {
                DeleteRequest = new DeleteRequest
                {
                    Key = new Dictionary<string, AttributeValue>
                    {
                        {"PK", new AttributeValue {S = PK}},
                        {"SK", new AttributeValue {S = SK}}
                    }
                }
            });
        }

        // Split requests into chunks of 25, which is the max for a single BatchWriteItem request
        var chunkedBatchRequests = new List<List<WriteRequest>>();
        for (var i = 0; i < batchRequests.Count; i += 25)
        {
            chunkedBatchRequests.Add(batchRequests.GetRange(i, Math.Min(25, batchRequests.Count - i)));
        }

        // Perform the BatchWriteItem for each chunk
        foreach (var chunk in chunkedBatchRequests)
        {
            var batchWriteItemRequest = new BatchWriteItemRequest
            {
                RequestItems = new Dictionary<string, List<WriteRequest>>
                {
                    {_dynamoDbSettings.TableName, chunk}
                }
            };

            try
            {
                await _dynamoDb.BatchWriteItemAsync(batchWriteItemRequest);
            }
            catch (ConditionalCheckFailedException)
            {
                throw new Exception($"Failed to delete item with PK - {PK} due to condition check");
            }
        }
    }
    public async Task TransactWriteItemsAsync(List<TransactWriteItem> transactWriteItems)
    {
        var request = new TransactWriteItemsRequest
        {
            TransactItems = transactWriteItems
        };

        await _dynamoDb.TransactWriteItemsAsync(request);
    }

    
    // Dynamo Methods
    public async Task<PutItemResponse> PutItemAsync(PutItemRequest request)
    {
        return await _dynamoDb.PutItemAsync(request);
    }

    public async Task<DeleteItemResponse> DeleteItemAsync(DeleteItemRequest request)
    {
        return await _dynamoDb.DeleteItemAsync(request);
    }

    public async Task<UpdateItemResponse> UpdateItemAsync(UpdateItemRequest request)
    {
        return await _dynamoDb.UpdateItemAsync(request);
    }

    public async Task<GetItemResponse> GetItemAsync(GetItemRequest request)
    {
        return await _dynamoDb.GetItemAsync(request);
    }

    public async Task<QueryResponse> QueryAsync(QueryRequest request)
    {
        return await _dynamoDb.QueryAsync(request);
    }

    public async Task<ScanResponse> ScanAsync(ScanRequest request)
    {
        return await _dynamoDb.ScanAsync(request);
    }

    public async Task<BatchWriteItemResponse> BatchWriteItemsAsync(BatchWriteItemRequest request)
    {
        return await _dynamoDb.BatchWriteItemAsync(request);
    }
}