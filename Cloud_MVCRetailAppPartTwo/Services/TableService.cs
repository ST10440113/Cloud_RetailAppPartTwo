using Azure;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Cloud_MVCRetailAppPartTwo.Models;

namespace Cloud_MVCRetailAppPartTwo.Services
{
    public class TableService
    {
        private readonly TableClient _table;
        private readonly TableClient _Ptable;
        private readonly TableClient _Otable;
        private readonly BlobServiceClient _blobServiceClient;


        public TableService(string connection)
        {
            _table = new TableClient(connection, "Customers");
            _Ptable = new TableClient(connection, "Products");
            _Otable = new TableClient(connection, "Orders");


        }


        public async Task<List<Customer>> ListCustomersAsync()
        {
            var customers = new List<Customer>();

            await foreach (var customer in _table.QueryAsync<Customer>())
            {
                customers.Add(customer);
            }
            return customers;
        }


        public async Task AddCustomerAsync(Customer c)
        {
            await _table.AddEntityAsync(c);
        }


        public async Task<Customer?> GetCustomerAsync(string partitionKey, string rowKey)
        {
            try
            {
                Pageable<Customer> queryResultsFilter = _table.Query<Customer>(filter: $"PartitionKey eq '{partitionKey}'");
                var customer = queryResultsFilter.FirstOrDefault(c => c.PartitionKey == partitionKey && c.RowKey == rowKey);
                return customer;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;

            }
        }


        public async Task UpdateCustomerAsync(Customer entity, bool ForInsert = false)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            if (string.IsNullOrEmpty(entity.PartitionKey) || string.IsNullOrEmpty(entity.RowKey))
            {
                throw new ArgumentException("PartitionKey and RowKey must be set for the entity.");
            }
            await _table.UpsertEntityAsync(entity, TableUpdateMode.Merge);

        }


        public async Task DeleteCustomerAsync(string partitionKey, string rowKey)
        {
            await _table.DeleteEntityAsync(partitionKey, rowKey);
        }






        public async Task<List<Product>> ListProductsAsync()
        {
            var products = new List<Product>();

            await foreach (var product in _Ptable.QueryAsync<Product>())
            {
                products.Add(product);
            }
            return products;
        }


        public async Task AddProductAsync(Product p)
        {
            await _Ptable.AddEntityAsync(p);
        }


        public async Task<Product?> GetProductAsync(string partitionKey, string rowKey)
        {
            try
            {
                Pageable<Product> queryResultsFilter = _Ptable.Query<Product>(filter: $"PartitionKey eq '{partitionKey}'");
                var product = queryResultsFilter.FirstOrDefault(c => c.PartitionKey == partitionKey && c.RowKey == rowKey);

                return product;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;

            }
        }



        public async Task UpdateProductAsync(Product entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            if (string.IsNullOrEmpty(entity.PartitionKey) || string.IsNullOrEmpty(entity.RowKey))
            {
                throw new ArgumentException("PartitionKey and RowKey must be set for the entity.");
            }
            await _Ptable.UpsertEntityAsync(entity, TableUpdateMode.Merge);

        }


        public async Task DeleteProductAsync(string partitionKey, string rowKey)
        {
            await _Ptable.DeleteEntityAsync(partitionKey, rowKey);
        }




        public async Task<List<Order>> ListOrdersAsync()
        {
            var orders = new List<Order>();

            await foreach (var order in _Otable.QueryAsync<Order>())
            {
                orders.Add(order);
            }
            return orders;
        }



        public async Task AddOrderAsync(Order o)
        {
            if (string.IsNullOrEmpty(o.PartitionKey) && string.IsNullOrEmpty(o.RowKey))
            {
                throw new ArgumentException("PartitionKey and RowKey must be set");
            }
            try
            {
                await _Otable.AddEntityAsync(o);
            }
            catch (RequestFailedException ex)
            {

                throw new InvalidOperationException(ex.Message);

            }
        }



        public async Task<Order?> GetOrderAsync(string partitionKey, string rowKey)
        {
            try
            {
                Pageable<Order> queryResultsFilter = _Otable.Query<Order>(filter: $"PartitionKey eq '{partitionKey}'");
                var order = queryResultsFilter.FirstOrDefault(c => c.PartitionKey == partitionKey && c.RowKey == rowKey);


                return order;

            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }


        public async Task UpdateOrderAsync(Order entity, bool ForInsert = false)
        {

            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (string.IsNullOrEmpty(entity.PartitionKey) || string.IsNullOrEmpty(entity.RowKey))
            {
                throw new ArgumentException("PartitionKey and RowKey must be set for the entity.");
            }

            await _Otable.UpsertEntityAsync(entity, TableUpdateMode.Merge);
        }




        public async Task DeleteOrderAsync(string partitionKey, string rowKey)
        {
            await _Otable.DeleteEntityAsync(partitionKey, rowKey);
        }






    }
}

