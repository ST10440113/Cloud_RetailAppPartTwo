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
        public BlobService(string connectionString)
        {

            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        public async Task<string> UploadAsync(Stream fileStream, string filename)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(filename);
            await blobClient.UploadAsync(fileStream);
            return blobClient.Uri.ToString();
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
