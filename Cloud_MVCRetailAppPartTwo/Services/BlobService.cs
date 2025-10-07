using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Cloud_MVCRetailAppPartTwo.Models;
using System.Net;

namespace Cloud_MVCRetailAppPartTwo.Services
{
    public class BlobService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName = "product-images";
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        public BlobService(IHttpClientFactory httpClientFactory, IConfiguration configuration, string connectionString, string containerName)
        {

            _blobServiceClient = new BlobServiceClient(connectionString);
 
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _containerName = containerName;
        }

        
            public async Task<HttpResponseMessage> UploadAsync(IFormFile file, Product product)
            {
                var httpClient = _httpClientFactory.CreateClient();
                var apiBaseUrl = _configuration["BlobFunctionApi:BaseUrl"];

                using var formData = new MultipartFormDataContent();
                formData.Add(new StringContent(product.ProductName ?? ""), "ProductName");
                formData.Add(new StringContent(product.Description ?? ""), "Description");
                formData.Add(new StringContent(product.Price ?? ""), "Price");
                formData.Add(new StringContent(product.StockQuantity.ToString()), "StockQuantity");

                if (file != null && file.Length > 0)
                    formData.Add(new StreamContent(file.OpenReadStream()), "ImageUrl", file.FileName);

                
                return await httpClient.PostAsync($"{apiBaseUrl}products", formData);
            }
        


        public async Task DeleteAsync(Product product)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobUri = new Uri(product.ImageUrl);
            var blobName = WebUtility.UrlDecode(blobUri.Segments.Last());
            var blobClient = containerClient.GetBlobClient(blobName);

            await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
        }

        public async Task EditAsync(IFormFile file, Product product)
        {
            var container = _blobServiceClient.GetBlobContainerClient(_containerName);
            var newBlob = container.GetBlobClient(file.FileName);

            using var stream = file.OpenReadStream();
            await newBlob.UploadAsync(stream, overwrite: true);

            product.ImageUrl = newBlob.Uri.ToString();
        }




    }
}
