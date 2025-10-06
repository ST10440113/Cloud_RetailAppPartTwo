using Azure;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;

namespace Cloud_MVCRetailAppPartTwo.Models
{
    public class Product : ITableEntity
    {
        [Key] public int ProductId { get; set; }
        [Display(Name = "Product")] public string ProductName { get; set; }
        public string Description { get; set; }
        public string Price { get; set; }
        [Display(Name = "Stock Quantity")] public int StockQuantity { get; set; }

        [Display(Name = "Product Image")] public string? ImageUrl { get; set; }



        //ITableEntity stuff
        public string PartitionKey { get; set; } = "PRODUCT";
        public string RowKey { get; set; } = Guid.NewGuid().ToString("N");

        public DateTimeOffset? Timestamp { get; set; }

        public ETag ETag { get; set; }

    }
}
