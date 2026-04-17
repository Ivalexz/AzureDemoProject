using Azure.Data.Tables;
using DemoProj.Models;
 
namespace DemoProj.Services;

public class AzureTableService
{
    readonly TableClient tableClient;

    public AzureTableService(IConfiguration configuration)
    {
        var connectionString = configuration["AzureStorage:ConnectionString"];
        var tableName = configuration["AzureStorage:TableName"];

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentException("Azure Storage connection string is not configured.");
        }

        if (string.IsNullOrEmpty(tableName))
        {
            throw new ArgumentException("Azure Storage table name is not configured.");
        }

        tableClient = new TableClient(connectionString, tableName);
        tableClient.CreateIfNotExists();
    }

    public async Task AddRequest(RequestFromModel obj)
    {
        var userEntry = new RequestModel
        {
            Name = obj.Name,
            Email = obj.Email,
            Message = obj.Message,
            Topic = obj.Topic,
            CreatedAt = DateTime.UtcNow
        };

        await tableClient.AddEntityAsync(userEntry);
    }

    public async Task<List<RequestModel>> GetAllRequest()
    {
        var listOfRequests = new List<RequestModel>();

        await foreach (var user in tableClient.QueryAsync<RequestModel>())
        {
            listOfRequests.Add(user);
        }

        return listOfRequests.ToList();
    }

    public async Task<RequestModel> GetRequestByRowKey(string rowkey)
    {
        try
        {
            var response = await tableClient.GetEntityAsync<RequestModel>("Requests", rowkey);
            return response.Value;
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    public async Task UpdateRequest(string rowKey, RequestFromModel obj)
    {
        var existingRequest = await GetRequestByRowKey(rowKey);
        if (existingRequest != null)
        {
            existingRequest.Name = obj.Name;
            existingRequest.Email = obj.Email;
            existingRequest.Message = obj.Message;
            existingRequest.Topic = obj.Topic;
            existingRequest.CreatedAt = DateTime.UtcNow;
            await tableClient.UpdateEntityAsync(existingRequest, existingRequest.ETag);
        }
    }
}