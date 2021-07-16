using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace InvoiceProcessor.Api.Services
{
    public class StorageService : IStorageService
    {
        private readonly string _azureStorageConnectionString;
        private readonly ILogger<StorageService> _logger;

        public StorageService(ILogger<StorageService> logger, IConfiguration configuration)
        {
            _azureStorageConnectionString = configuration.GetConnectionString("AzureStorageConnectionString");
            _logger = logger;
        }

        public async Task UploadAsync(Stream content, string containerName, string path, CancellationToken cancellationToken)
        {
            if (content is null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            if (string.IsNullOrEmpty(containerName))
            {
                throw new ArgumentException($"'{nameof(containerName)}' cannot be null or empty.", nameof(containerName));
            }

            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException($"'{nameof(path)}' cannot be null or empty.", nameof(path));
            }

            using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["ContainerName"] = containerName,
                ["Path"] = path
            }))
            {
                var blobServiceClient = new BlobServiceClient(_azureStorageConnectionString);
                var containerClient = (await blobServiceClient.CreateBlobContainerAsync(containerName, cancellationToken: cancellationToken)).Value;
                var blobClient = containerClient.GetBlobClient(path);

                _logger.LogDebug("Uploading file to blob storage");
                await blobClient.UploadAsync(content, cancellationToken);
                _logger.LogDebug("Upload completed");
            }
        }
    }
}
