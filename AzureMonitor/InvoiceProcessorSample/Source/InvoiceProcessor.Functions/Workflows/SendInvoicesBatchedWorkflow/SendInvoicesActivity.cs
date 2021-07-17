using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using InvoiceProcessor.Common;
using InvoiceProcessor.Common.Core;
using InvoiceProcessor.Common.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace InvoiceProcessor.Functions.Activities
{
    public class SendInvoicesActivity
    {
        private const int BatchSize = 100;
        private readonly IInvoiceSetSerializer _invoiceSetSerializer;
        private readonly HttpClient _client;

        public SendInvoicesActivity(IInvoiceSetSerializer invoiceSetSerializer, IHttpClientFactory httpClientFactory)
        {
            _invoiceSetSerializer = invoiceSetSerializer;
            _client = httpClientFactory.CreateClient();
        }

        [FunctionName(nameof(SendInvoicesActivity))]
        public async Task<(Guid ExternalBatchId, string[] InvoiceIds)> Run(
            [ActivityTrigger]string customer,
            Binder binder,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            var query = @$"SELECT TOP {BatchSize} * FROM c
WHERE c.customer = '{customer}' AND c.status = '{InvoiceStatus.Created}'
ORDER BY c._ts";

            var cosmosDBAttribute = new CosmosDBAttribute(CosmosDbKeys.DatabaseName, CosmosDbKeys.InvoicesCollectionName)
            {
                ConnectionStringSetting = SettingNames.CosmosDBConnection,
                PartitionKey = Invoice.PartitionKey,
                SqlQuery = query
            };

            var unsentInvoices = (await binder.BindAsync<IEnumerable<Invoice>>(cosmosDBAttribute, cancellationToken)).ToList();
            logger.LogDebug("Unsent invoices. InvoiceCount:{InvoiceCount}", unsentInvoices.Count);

            var externalBatchId = await SendInvoicesToExternalService(unsentInvoices, cancellationToken);
            logger.LogDebug("ExternalBatchId:{ExternalBatchId}", externalBatchId);

            foreach (var invoice in unsentInvoices)
            {
                invoice.Status = InvoiceStatus.SentToExternalSystem;
                invoice.ExternalBatchId = externalBatchId;
            }

            await SaveInvoices(binder, customer, unsentInvoices, cancellationToken);
            logger.LogDebug("Sent invoices. InvoiceCount:{InvoiceCount}", unsentInvoices.Count);

            return (externalBatchId, unsentInvoices.Select(o => o.Id).ToArray());
        }

        private static async Task SaveInvoices(Binder binder, string customer, ICollection<Invoice> invoices, CancellationToken cancellationToken)
        {
            var cosmosDBAttribute = new CosmosDBAttribute(CosmosDbKeys.DatabaseName, CosmosDbKeys.InvoicesCollectionName)
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
    }
}
