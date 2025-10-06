using Azure;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Cloud_MVCRetailAppPartTwo.Models;
using System.Net.Http.Headers;

namespace Cloud_MVCRetailAppPartTwo.Services
{
    public class FileService
    {
        private readonly string _connectionString;
        private readonly string _fileShareName;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public FileService(string connectionString, string fileShareName, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _fileShareName = fileShareName ?? throw new ArgumentNullException(nameof(fileShareName));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }
        public async Task UploadFileAsync(string directoryName, string fileName, Stream fileStream)
        {
            var httpClient = _httpClientFactory.CreateClient();

            using var content = new StreamContent(fileStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            var apiBaseUrl = _configuration["FileFunctionApi:BaseUrl"];

            var url = $"{apiBaseUrl}upload/{fileName}";
            var response = await httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"File upload failed");
            }
        }

        public async Task<Stream> DownloadFileAsync(string directoryName, string fileName)
        {
            try
            {
                var serviceClient = new ShareServiceClient(_connectionString);
                var shareClient = serviceClient.GetShareClient(_fileShareName);
                var directoryClient = shareClient.GetDirectoryClient(directoryName);
                var fileClient = directoryClient.GetFileClient(fileName);
                var downloadInfo = await fileClient.DownloadAsync();
                return downloadInfo.Value.Content;
            }
            catch (RequestFailedException ex)
            {
                throw new Exception("Error downloading file from Azure File Share", ex);
            }
        }

        public async Task<List<FileModel>> ListFilesAsync()
        {
            var fileModels = new List<FileModel>();

            try
            {
                var serviceClient = new ShareServiceClient(_connectionString);
                var shareClient = serviceClient.GetShareClient("files");
                var directoryClient = shareClient.GetDirectoryClient("directory");
                await directoryClient.CreateIfNotExistsAsync();

                await foreach (ShareFileItem item in directoryClient.GetFilesAndDirectoriesAsync())
                {
                    if (!item.IsDirectory)
                    {
                        var fileClient = directoryClient.GetFileClient(item.Name);
                        var properties = await fileClient.GetPropertiesAsync();

                        fileModels.Add(new FileModel
                        {
                            Name = item.Name,
                            Size = properties.Value.ContentLength,
                            LastModified = properties.Value.LastModified
                        });
                    }
                }
            }
            catch (RequestFailedException ex)
            {
                throw new Exception("Error listing files in Azure File Share", ex);
            }

            return fileModels;
        }


    }
}

