using Azure.Storage.Queues;
using Cloud_MVCRetailAppPartTwo.Models;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace Cloud_MVCRetailAppPartTwo.Services
{
    public class QueueService
    {
        private readonly QueueClient _queueClient;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public QueueService(IHttpClientFactory httpClientFactory, IConfiguration configuration, string connectionString, string queueName)
        {
            _queueClient = new QueueClient(connectionString, queueName);
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;

        }

        public async Task SendMessageAsync(Order order)
        {
            var queueClient = new QueueClient(_configuration.GetConnectionString("connection"), "orders");
            await queueClient.CreateIfNotExistsAsync();
            string orderJson = JsonSerializer.Serialize(order);
            await queueClient.SendMessageAsync(Convert.ToBase64String(Encoding.UTF8.GetBytes(orderJson)));
                      
        }


    }
}

