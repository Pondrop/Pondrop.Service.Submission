using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pondrop.Service.Submission.Application.Interfaces.BlobStorage;
using Pondrop.Service.Submission.Application.Models;

namespace Pondrop.Service.Submission.Infrastructure.BlobStorage;
public class BlobStorageService : IBlobStorageService
{
    private readonly BlobStorageConfiguration _config;
    private readonly ILogger<BlobStorageService> _logger;
    private readonly BlobServiceClient _blobServiceClient;
    private readonly BlobContainerClient _blobClient;

    public BlobStorageService(
        IOptions<BlobStorageConfiguration> config,
        ILogger<BlobStorageService> logger)
    {
        _logger = logger;
        _config = config.Value;
        _blobServiceClient = new BlobServiceClient(_config.ConnectionString);

        _blobClient = _blobServiceClient.GetBlobContainerClient(_config.ContainerName);

    }

    public async Task<string> UploadImageAsync(string fileName, string base64, string userId = "", string mimeType = null)
    {
        if (!await _blobClient.ExistsAsync())
            await _blobClient.CreateIfNotExistsAsync();


        string file = fileName.Replace(" ", "-");
        string name = $"{Path.GetFileNameWithoutExtension(file)}-{DateTimeOffset.Now.ToUnixTimeSeconds().ToString()}{Path.GetExtension(file)}";
        string folder = $"{userId}/{name}";

        var stream = ConvertBase64ToStream(base64);
        var blobInfo = await _blobClient.UploadBlobAsync(folder, stream);

        return !blobInfo.GetRawResponse().IsError ? $"{_blobClient.Uri.AbsoluteUri}/{folder}" : string.Empty;
    }

    private Stream ConvertBase64ToStream(string base64)
    {
        var bytes = Convert.FromBase64String(base64);
        return new MemoryStream(bytes);
    }

}