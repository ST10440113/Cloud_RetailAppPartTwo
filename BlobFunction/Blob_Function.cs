using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Cloud_MVCRetailAppPartTwo.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System.Net;

namespace BlobFunction;

public class Blob_Function
{
    private readonly ILogger<Blob_Function> _logger;
    private readonly TableClient _tableClient;
    private readonly BlobContainerClient _blobContainerClient;
    private readonly string _storageConnectionString;

    public Blob_Function(ILogger<Blob_Function> logger)
    {
        _logger = logger;
        _storageConnectionString = Environment.GetEnvironmentVariable("connection");
        var serviceClient = new TableServiceClient(_storageConnectionString);
        _tableClient = serviceClient.GetTableClient("Products");
        _blobContainerClient = new BlobContainerClient(_storageConnectionString, "product-images");
        _blobContainerClient.CreateIfNotExists(Azure.Storage.Blobs.Models.PublicAccessType.Blob);



    }


    [Function("AddProductFunction")]
    public async Task<HttpResponseData> AddProduct(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "products")] HttpRequestData req)
    {
        _logger.LogInformation("HTTP Trigger function to add a product with its image received a request");

        var newProduct = new Product();
        string? uploadedBlobUrl = null;


        var contentType = req.Headers.GetValues("Content-Type").FirstOrDefault();
        var mediaType = MediaTypeHeaderValue.Parse(contentType);
        var boundary = HeaderUtilities.RemoveQuotes(mediaType.Boundary).Value;
        var reader = new MultipartReader(boundary, req.Body);
        MultipartSection? section;

        while ((section = await reader.ReadNextSectionAsync()) != null)
        {
            if (!ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition))
                continue;


            if (contentDisposition.DispositionType.Equals("form-data") &&
                string.IsNullOrEmpty(contentDisposition.FileName.Value) &&
                string.IsNullOrEmpty(contentDisposition.FileNameStar.Value))
            {
                var product = contentDisposition.Name.Value?.Trim('"');
                var value = await new StreamReader(section.Body).ReadToEndAsync();

                if (product == "ProductName") newProduct.ProductName = value;
                if (product == "Description") newProduct.Description = value;
                if (product == "Price") newProduct.Price = value;
                if (product == "StockQuantity" && int.TryParse(value, out int quantityValue)) newProduct.StockQuantity = quantityValue;
            }

            else if (contentDisposition.DispositionType.Equals("form-data"))
            {
                var fileName = contentDisposition.FileName.Value?.Trim('"');
                var uniqueFileName = $"{Guid.NewGuid()}-{Path.GetFileName(fileName)}";
                var blobClient = _blobContainerClient.GetBlobClient(uniqueFileName);

                await blobClient.UploadAsync(section.Body, true);
                uploadedBlobUrl = blobClient.Uri.ToString();
                newProduct.ImageUrl = uploadedBlobUrl;
            }
        }

        //check if anythings empty
        if (string.IsNullOrEmpty(newProduct.ProductName) ||
            string.IsNullOrEmpty(newProduct.Description) ||
            string.IsNullOrEmpty(newProduct.Price) ||
              newProduct.StockQuantity <= 0 ||
            string.IsNullOrEmpty(uploadedBlobUrl))
        {
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }


        _tableClient.CreateIfNotExists();
        newProduct.PartitionKey = "PRODUCT";
        newProduct.RowKey = Guid.NewGuid().ToString();
        newProduct.ProductId = GetHashCode();

        await _tableClient.AddEntityAsync(newProduct);
        _logger.LogInformation($"Successfully added {newProduct.ProductName} and uploaded its image");

        return req.CreateResponse(HttpStatusCode.Created);
    }

}
