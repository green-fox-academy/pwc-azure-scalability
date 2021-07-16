using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using InvoiceProcessor.Common.Transformations;
using Microsoft.Azure.Storage.Blob;
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
            CloudBlockBlob customerPayloadCloudBlock,
            [Blob("transformed-payloads/{name}", FileAccess.ReadWrite)]
            CloudBlockBlob transformedPayloadCloudBlock,
            string name,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            if (customerPayloadCloudBlock is null)
            {
                throw new ArgumentNullException(nameof(customerPayloadCloudBlock));
            }

            if (transformedPayloadCloudBlock is null)
            {
                throw new ArgumentNullException(nameof(transformedPayloadCloudBlock));
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));
            }

            if (logger is null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            customerPayloadCloudBlock.Metadata.TryGetValue("Customer", out var customer);
            logger.LogDebug("ProcessReceivedInvoices triggered for blob. Name:{Name}, Size:{Size}, Customer:{Customer} bytes", name, customerPayloadCloudBlock.Properties.Length, customer);

            using (var targetMemoryStream = new MemoryStream())
            {
                using (var downloadStream = new MemoryStream())
                {
                    await customerPayloadCloudBlock.DownloadToStreamAsync(downloadStream, cancellationToken: cancellationToken);
                    downloadStream.Position = 0;

                    _xslTransformationService.TransformClient1XmlToCoreXml(downloadStream, targetMemoryStream);
                }

                logger.LogDebug("Uploading transformed xml to blob storage");
                transformedPayloadCloudBlock.Properties.ContentType = "text/xml";
                transformedPayloadCloudBlock.Metadata["Customer"] = customer;
                await transformedPayloadCloudBlock.UploadFromStreamAsync(targetMemoryStream, cancellationToken: cancellationToken);
            }

            logger.LogDebug("Upload completed");
        }
    }
}
