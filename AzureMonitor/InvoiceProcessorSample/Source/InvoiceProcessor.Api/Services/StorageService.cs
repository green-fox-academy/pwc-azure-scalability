using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using InvoiceProcessor.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;

namespace InvoiceProcessor.Api.Services
{
    public class StorageService : IStorageService
    {
        private readonly string _azureStorageConnectionString;
        private readonly ILogger<StorageService> _logger;

        public StorageService(ILogger<StorageService> logger, IConfiguration configuration)
        {
            _azureStorageConnectionString = configuration["AzureStorageConnectionString"];
            _logger = logger;
        }

        public async Task UploadAsync(IFormFile file, string containerName, string path, CancellationToken cancellationToken)
        {
            if (file is null)
            {
                throw new ArgumentNullException(nameof(file));
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
                using var content = file.OpenReadStream();
                var blobServiceClient = new BlobServiceClient(_azureStorageConnectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                var handleContainerNotExistsPolicy = Policy
                    .Handle<RequestFailedException>(e => e.ErrorCode == BlobErrorCode.ContainerNotFound)
                    .RetryAsync(1, async (e, count) =>
                    {
                        _logger.LogDebug("Container not found. Creating container");
                        await containerClient.CreateIfNotExistsAsync();
                        _logger.LogDebug("Container created");

                        content.Position = 0;
                    });

                _logger.LogDebug("Uploading file to blob storage");

                var blobClient = containerClient.GetBlobClient(path);
                await handleContainerNotExistsPolicy.ExecuteAsync(async () =>
                {
                    await blobClient.UploadAsync(
                        content,
                        new BlobHttpHeaders
                        {
                            ContentType = file.ContentType,
                            ContentDisposition = file.ContentDisposition,
                        },
                        metadata: new Dictionary<string, string>
                        {
                            { MetadataKeys.Customer, "Customer1" }
                        },
                        cancellationToken: cancellationToken);
                });

                _logger.LogDebug("Upload completed");
            }
        }
    }
}
