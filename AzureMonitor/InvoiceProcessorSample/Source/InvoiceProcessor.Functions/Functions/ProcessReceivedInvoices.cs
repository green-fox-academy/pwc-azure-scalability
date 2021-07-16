using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using InvoiceProcessor.Common.Transformations;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace InvoiceProcessor.Functions.Functions
{
    public class ProcessReceivedInvoices
    {
        private readonly IXslTransformationService _xslTransformationService;

        public ProcessReceivedInvoices(IXslTransformationService xslTransformationService)
        {
            _xslTransformationService = xslTransformationService;
        }

        [FunctionName("ProcessReceivedInvoices")]
        public async Task Run(
            [BlobTrigger("customer-payloads/{name}", Connection = "AzureWebJobsStorage")]
            BlobClient customerPayloadBlobClient,
            [Blob("transformed-payloads/{name}", FileAccess.ReadWrite)]
            BlobClient targetBlobClient,
            string name,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            if (customerPayloadBlobClient is null)
            {
                throw new ArgumentNullException(nameof(customerPayloadBlobClient));
            }

            if (targetBlobClient is null)
            {
                throw new ArgumentNullException(nameof(targetBlobClient));
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));
            }

            if (logger is null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            var properties = (await customerPayloadBlobClient.GetPropertiesAsync(cancellationToken: cancellationToken)).Value;
            properties.Metadata.TryGetValue("Customer", out var customer);

            logger.LogDebug("ProcessReceivedInvoices triggered for blob. Name:{Name}, Size:{Size}, Customer:{Customer} bytes", name, properties.ContentLength, customer);

            using (var memoryStream = new MemoryStream())
            {
                using (var downloadStream = (await customerPayloadBlobClient.DownloadStreamingAsync(cancellationToken: cancellationToken)).Value.Content)
                {
                    _xslTransformationService.TransformClient1XmlToCoreXml(downloadStream, memoryStream);
                }

                logger.LogDebug("Uploading transformed xml to blob storage");
                await targetBlobClient.UploadAsync(
                    memoryStream,
                    new BlobHttpHeaders
                    {
                        ContentType = "text/xml"
                    },
                    metadata: new Dictionary<string, string>
                    {
                        { "Customer", customer }
                    },
                    cancellationToken: cancellationToken);
            }

            logger.LogDebug("Upload completed");
        }
    }
}
