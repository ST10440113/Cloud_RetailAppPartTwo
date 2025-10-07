using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Cloud_MVCRetailAppPartTwo.Models;
using System;
using System.Text.Json;

namespace QueueFunction;

public class Queue_Function
{
    private readonly ILogger<Queue_Function> _logger;
    private readonly TableClient _tableClient;
    private readonly string _storageConnectionString;

    public Queue_Function(ILogger<Queue_Function> logger)
    {
        _logger = logger;
        _storageConnectionString = ""; 
        var serviceClient = new TableServiceClient(_storageConnectionString);
        _tableClient = serviceClient.GetTableClient("Orders");
    }


    [Function(nameof(QueueOrderSender))]
    public async Task QueueOrderSender([QueueTrigger("orders", Connection = "connection")] QueueMessage message)
    {
        _logger.LogInformation($"C# Queue trigger function processed: {message.MessageText}");

        //create table if it does not exist
        await _tableClient.CreateIfNotExistsAsync();

        
        var order = JsonSerializer.Deserialize<Order>(message.MessageText);
        if (order == null)
        {
            _logger.LogError("Failed to deserialize the order from the queue message.");
            return;

        }

        order.PartitionKey = "ORDER";
        order.RowKey = Guid.NewGuid().ToString();
        order.OrderDate = DateTime.UtcNow;
        order.OrderId = GetHashCode();
        _logger.LogError($"Saving entity with RowKey: {order.RowKey}");
        await _tableClient.AddEntityAsync(order);
        _logger.LogInformation($"Order with OrderId: {order.OrderId} added to the Orders table.");

    }
}





