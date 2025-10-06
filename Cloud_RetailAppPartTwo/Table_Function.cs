using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Azure.Data.Tables;
using Azure.Storage.Queues.Models;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;


namespace Cloud_RetailAppPartTwo;

public class Table_Function
{
    private readonly ILogger<Table_Function> _logger;
    private readonly string _storageConnectionString;
    private TableClient _tableClient;

    public Table_Function(ILogger<Table_Function> logger)
    {
        _logger = logger;
        _storageConnectionString = Environment.GetEnvironmentVariable("connection");
        var serviceClient = new TableServiceClient(_storageConnectionString);
        _tableClient = serviceClient.GetTableClient("Customers");



    }


    //[Function("AddCustomerFunction")]
    //public async Task AddCustomer([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
    //{
    //    _tableClient.CreateIfNotExists();

    //    using var reader = new StreamReader(req.Body);
    //    var body = await reader.ReadToEndAsync();

    //    var customer = JsonSerializer.Deserialize<Customer>(body);

    //    if (customer == null)
    //    {
    //        _logger.LogError("Invalid customer data");
    //        return;
    //    }
    //    customer.PartitionKey = "CUSTOMER";
    //    customer.RowKey = Guid.NewGuid().ToString();
    //    await _tableClient.AddEntityAsync(customer);

    //    _logger.LogInformation("Successfully saved customer to Customer Table");

    //}


}