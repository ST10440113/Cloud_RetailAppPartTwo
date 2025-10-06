using Azure.Storage.Queues;
using System.Text;

namespace Cloud_MVCRetailAppPartTwo.Services
{
    public class QueueService
    {
        private readonly QueueClient _queueClient;

        public QueueService(string connectionStringString, string queueName)
        {
            _queueClient = new QueueClient(connectionStringString, queueName);

        }

        public async Task SendMessageAsync(string message)
        {
            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(message));
            await _queueClient.SendMessageAsync(base64);
        }


    }
}

