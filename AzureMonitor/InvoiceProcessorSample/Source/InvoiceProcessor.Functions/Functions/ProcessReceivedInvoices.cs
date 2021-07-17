using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using InvoiceProcessor.Common.Core;
using InvoiceProcessor.Common.Services;
using InvoiceProcessor.Common.Transformations;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;

namespace InvoiceProcessor.Functions.Functions
{
    public class ProcessReceivedInvoices
    {
        private readonly IXslTransformationService _xslTransformationService;
        private readonly IInvoiceSetSerializer _invoiceSetSerializer;

        public ProcessReceivedInvoices(IXslTransformationService xslTransformationService, IInvoiceSetSerializer invoiceSetSerializer)
        {
            _xslTransformationService = xslTransformationService;
            _invoiceSetSerializer = invoiceSetSerializer;
        }

        [FunctionName("ProcessReceivedInvoices")]
        public async Task Run(
            [BlobTrigger("customer-payloads/{name}", Connection = "AzureWebJobsStorage")]
            CloudBlockBlob customerPayloadCloudBlock,
            [Blob("transformed-payloads/{name}", FileAccess.ReadWrite)]
            CloudBlockBlob transformedPayloadCloudBlock,
            Binder binder,
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

            if (binder is null)
            {
                throw new ArgumentNullException(nameof(binder));
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

            string invoiceSetXml;
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

                targetMemoryStream.Position = 0;
                using var streamReader = new StreamReader(targetMemoryStream);
                invoiceSetXml = await streamReader.ReadToEndAsync();
            }

            logger.LogDebug("Upload completed");

            await InsertTransformedPayloadEntity(binder, name, customer, cancellationToken);

            var invoiceSet = _invoiceSetSerializer.DeserializeFromXml(invoiceSetXml);
            logger.LogDebug("InvoiceSet deserialized. InvoiceCount:{InvoiceCount}", invoiceSet.Invoices.Length);

            var cosmosDBAttribute = new CosmosDBAttribute("InvoiceProcessorDb", "Invoices")
            {
                CreateIfNotExists = true,
                PartitionKey = "/customer",
                ConnectionStringSetting = "CosmosDBConnection"
            };

            var invoiceCollector = await binder.BindAsync<IAsyncCollector<Invoice>>(cosmosDBAttribute, cancellationToken);
            foreach (var invoice in invoiceSet.Invoices)
            {
                invoice.Customer = customer;
                invoice.Status = InvoiceStatus.Created;
                invoice.CreatedAt = DateTime.UtcNow;
                await invoiceCollector.AddAsync(invoice, cancellationToken);
            }
        }

        private static async Task InsertTransformedPayloadEntity(Binder binder, string name, string customer, CancellationToken cancellationToken)
        {
            var tableAttribute = new TableAttribute("TransformedPayloads");
            var tableCollector = await binder.BindAsync<IAsyncCollector<TransformedPayloadEntity>>(tableAttribute, cancellationToken);
            await tableCollector.AddAsync(
                new TransformedPayloadEntity
                {
                    PartitionKey = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                    RowKey = name,
                    Customer = customer
                },
                cancellationToken);
        }

        private class TransformedPayloadEntity : TableEntity
        {
            public string Customer { get; set; }
        }
    }
}
