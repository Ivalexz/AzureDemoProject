namespace DemoProj.Models;
using Azure;
using Azure.Data.Tables;
using System;

public class RequestModel : ITableEntity
{
    public string PartitionKey { get; set; } = "Requests"; // група запитів
    public string RowKey { get; set; } = Guid.NewGuid().ToString(); // унікальний ідентифікатор для кожного запиту
    public DateTimeOffset? Timestamp { get; set; } // технічний час зміни запису
    public ETag ETag { get; set; } // мітка запису для контролю версій

    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}