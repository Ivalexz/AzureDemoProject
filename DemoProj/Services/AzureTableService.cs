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
            Phone = obj.Phone,
            Category = obj.Category,
            Status = "New", 
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
            existingRequest.Phone = obj.Phone;
            existingRequest.Category = obj.Category;
            existingRequest.Status = "Updated";
            existingRequest.UpdatedAt = DateTime.UtcNow;
            await tableClient.UpdateEntityAsync(existingRequest, existingRequest.ETag);
        }
    }
    
    public async Task DeleteRequest(string rowKey)
    {
        await tableClient.DeleteEntityAsync("Requests", rowKey);
    }
    
    public async Task CloseRequest(string rowKey)
    {
        var existingRequest = await GetRequestByRowKey(rowKey);
        if (existingRequest != null)
        {
            existingRequest.Status = "Closed";
            await tableClient.UpdateEntityAsync(existingRequest, existingRequest.ETag);
        }
    }
    
    public async Task<List<RequestModel>> GetFilteredRequests(string search)
    {
        var allRequests = await GetAllRequest();
        
        var filtered = allRequests.Where(r => r.Status == "New" || r.Status == "Updated" || r.Status == "Closed").ToList();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            filtered = filtered.Where(r =>
                (r.Name ?? "").Contains(term, StringComparison.OrdinalIgnoreCase) ||
                (r.Email ?? "").Contains(term, StringComparison.OrdinalIgnoreCase) ||
                (r.Message ?? "").Contains(term, StringComparison.OrdinalIgnoreCase) ||
                (r.Topic ?? "").Contains(term, StringComparison.OrdinalIgnoreCase) ||
                (r.Phone ?? "").Contains(term, StringComparison.OrdinalIgnoreCase) ||
                (r.Category ?? "").Contains(term, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }
        var list = filtered.ToList();
        for (int i = 0; i < list.Count - 1; i++)
        {
            for (int j = i + 1; j < list.Count; j++)
            {
                if (list[i].CreatedAt < list[j].CreatedAt)
                {
                    var temp = list[i];
                    list[i] = list[j];
                    list[j] = temp;
                }
            }
        }

        return list;
    }
}