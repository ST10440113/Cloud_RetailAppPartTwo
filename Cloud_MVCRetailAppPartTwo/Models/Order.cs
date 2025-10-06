using Azure;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Cloud_MVCRetailAppPartTwo.Models
{
    public class Order : ITableEntity
    {
        [Key] public int OrderId { get; set; }

        [JsonPropertyName("quantity")] public int Quantity { get; set; }


        [JsonPropertyName("orderDate")]
        [Display(Name = "Order Date")]
        [Timestamp]
        [DataType(DataType.Date)]
        public DateTime OrderDate { get; set; }


        [JsonPropertyName("status")] public string Status { get; set; }


        [JsonPropertyName("customerId")] public int CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; }



        [JsonPropertyName("productId")] public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product Product { get; set; }




        //ITableEntity stuff
        [JsonPropertyName("partitionKey")] public string PartitionKey { get; set; } = "ORDER";
        [JsonPropertyName("rowKey")] public string RowKey { get; set; } = Guid.NewGuid().ToString("N");

        [JsonPropertyName("timestamp")] public DateTimeOffset? Timestamp { get; set; }

        [JsonPropertyName("etag")] public ETag ETag { get; set; }

    }
}
