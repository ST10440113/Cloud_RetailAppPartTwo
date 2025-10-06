using Azure.Storage.Files.Shares;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace FileFunction;

public class File_Function
{
    private readonly ILogger<File_Function> _logger;

    public File_Function(ILogger<File_Function> logger)
    {
        _logger = logger;
    }

    [Function("UploadFile")]
    public async Task<HttpResponseData> UploadFile([HttpTrigger(AuthorizationLevel.Function, "post", Route = "upload/{fileName}")] HttpRequestData req, string fileName)
    {
        var response = req.CreateResponse();

        try
        {
            string connectionString = Environment.GetEnvironmentVariable("connection");
            string shareName = "files";
            string directoryName = "directory";

            var shareClient = new ShareClient(connectionString, shareName);
            await shareClient.CreateIfNotExistsAsync();

            var directoryClient = shareClient.GetDirectoryClient(directoryName);
            await directoryClient.CreateIfNotExistsAsync();

            var fileClient = directoryClient.GetFileClient(fileName);

            using var memoryStream = new MemoryStream();
            await req.Body.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            await fileClient.CreateAsync(memoryStream.Length);
            await fileClient.UploadRangeAsync(new Azure.HttpRange(0, memoryStream.Length), memoryStream);

            response.StatusCode = System.Net.HttpStatusCode.OK;
            await response.WriteStringAsync($"File '{fileName}' uploaded successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file.");
            response.StatusCode = System.Net.HttpStatusCode.InternalServerError;
            await response.WriteStringAsync($"Error: {ex.Message}");
        }

        return response;
    }

}
