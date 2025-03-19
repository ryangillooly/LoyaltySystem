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

    // Common
    public async Task BatchWriteRecordsAsync(IEnumerable<Dictionary<string, AttributeValue>> items)
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
    
    public async Task<TransactGetItemsResponse> TransactGetItemsAsync(TransactGetItemsRequest transactGetItemsRequest)
    {
        try
        {
            return await _dynamoDb.TransactGetItemsAsync(transactGetItemsRequest);
        }
        catch (Exception ex)  
        {
            throw new Exception("Failed to transact get items", ex);
        }
    }
}