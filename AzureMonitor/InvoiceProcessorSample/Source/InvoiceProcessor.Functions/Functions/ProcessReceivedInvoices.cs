using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using InvoiceProcessor.Common.Core;
using InvoiceProcessor.Common.Services;
using InvoiceProcessor.Common.Transformations;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace InvoiceProcessor.Functions.Functions
{
    public class ProcessReceivedInvoices
    {
        private readonly IXslTransformationService _xslTransformationService;
        private readonly IInvoiceSetSerializer _invoiceSetSerializer;
        private readonly HttpClient _client;

        public ProcessReceivedInvoices(IXslTransformationService xslTransformationService, IInvoiceSetSerializer invoiceSetSerializer, IHttpClientFactory httpClientFactory)
        {
            _xslTransformationService = xslTransformationService;
            _invoiceSetSerializer = invoiceSetSerializer;
            _client = httpClientFactory.CreateClient();
        }

        [FunctionName("ProcessReceivedInvoices")]
        public async Task Run(
            [BlobTrigger("customer-payloads/{name}", Connection = SettingNames.StorageConnection)]
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

            await SaveInvoices(binder, customer, invoiceSet.Invoices, cancellationToken);

            var query = @$"SELECT TOP 10 * FROM c
WHERE c.customer = 'Customer1' AND c.status = '{InvoiceStatus.Created}'
ORDER BY c._ts";

            var cosmosDBAttribute = new CosmosDBAttribute("InvoiceProcessorDb", "Invoices")
            {
                ConnectionStringSetting = SettingNames.CosmosDBConnection,
                PartitionKey = Invoice.PartitionKey,
                SqlQuery = query
            };

            var unsentInvoices = (await binder.BindAsync<IEnumerable<Invoice>>(cosmosDBAttribute, cancellationToken)).ToList();
            logger.LogDebug("Unsent invoices. InvoiceCount:{InvoiceCount}", unsentInvoices.Count);

            // Send
            foreach (var invoice in unsentInvoices)
            {
                invoice.Status = InvoiceStatus.Sent;
            }

            await SaveInvoices(binder, customer, unsentInvoices, cancellationToken);
            logger.LogDebug("Sent invoices. InvoiceCount:{InvoiceCount}", unsentInvoices.Count);

            var externalBatchId = await SendInvoicesToExternalService(unsentInvoices, cancellationToken);
            logger.LogDebug("ExternalBatchId:{ExternalBatchId}", externalBatchId);

            var response = await GetBatchStatus(externalBatchId, cancellationToken);
            logger.LogDebug("BatchStatus:{BatchStatus}", response);

            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            response = await GetBatchStatus(externalBatchId, cancellationToken);
            logger.LogDebug("BatchStatus:{BatchStatus}", response);
        }

        private static async Task SaveInvoices(Binder binder, string customer, ICollection<Invoice> invoices, CancellationToken cancellationToken)
        {
            var cosmosDBAttribute = new CosmosDBAttribute("InvoiceProcessorDb", "Invoices")
            {
                CreateIfNotExists = true,
                ConnectionStringSetting = SettingNames.CosmosDBConnection,
                PartitionKey = Invoice.PartitionKey
            };

            var invoiceCollector = await binder.BindAsync<IAsyncCollector<Invoice>>(cosmosDBAttribute, cancellationToken);
            foreach (var invoice in invoices)
            {
                if (invoice.Id == null)
                {
                    invoice.Customer = customer;
                    invoice.Status = InvoiceStatus.Created;
                    invoice.CreatedAt = DateTime.UtcNow;
                }

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

        private async Task<string> GetBatchStatus(Guid externalBatchId, CancellationToken cancellationToken)
        {
            var responseMessage = await _client.GetAsync($"https://localhost:6001/ExternalInvoices/GetInvoiceBatchStatus?id={externalBatchId}", cancellationToken);
            responseMessage.EnsureSuccessStatusCode();
            var response = await responseMessage.Content.ReadAsAsync<string>(cancellationToken);
            return response;
        }

        private async Task<Guid> SendInvoicesToExternalService(List<Invoice> unsentInvoices, CancellationToken cancellationToken)
        {
            var json = JsonConvert.SerializeObject(unsentInvoices);
            HttpResponseMessage responseMessage;
            using (var memoryStream = new MemoryStream())
            {
                using (var sw = new StreamWriter(memoryStream, leaveOpen: true))
                {
                    await sw.WriteAsync(json);
                }

                memoryStream.Position = 0;
                using var content = new MultipartFormDataContent();
                using var streamContent = new StreamContent(memoryStream)
                {
                    Headers =
                    {
                        ContentLength = memoryStream.Length,
                        ContentType = new MediaTypeHeaderValue("application/json")
                    }
                };
                content.Add(streamContent, "invoices", "invoices.json");
                responseMessage = await _client.PostAsync("https://localhost:6001/ExternalInvoices/ProcessInvoiceBatch", content, cancellationToken);
            }

            responseMessage.EnsureSuccessStatusCode();
            var response = await responseMessage.Content.ReadAsAsync<Guid>(cancellationToken);
            return response;
        }

        private class TransformedPayloadEntity : TableEntity
        {
            public string Customer { get; set; }
        }
    }
}
