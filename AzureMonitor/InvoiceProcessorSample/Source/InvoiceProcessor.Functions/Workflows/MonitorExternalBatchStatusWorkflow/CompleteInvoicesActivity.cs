using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using InvoiceProcessor.Common;
using InvoiceProcessor.Common.Core;
using InvoiceProcessor.Common.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace InvoiceProcessor.Functions.Workflows.SendInvoicesBatchedWorkflow
{
    public class CompleteInvoicesActivity
    {
        private readonly IInvoiceSetSerializer _invoiceSetSerializer;
        private readonly HttpClient _client;

        public CompleteInvoicesActivity(IInvoiceSetSerializer invoiceSetSerializer, IHttpClientFactory httpClientFactory)
        {
            _invoiceSetSerializer = invoiceSetSerializer;
            _client = httpClientFactory.CreateClient();
        }

        [FunctionName(nameof(CompleteInvoicesActivity))]
        public async Task Run(
            [ActivityTrigger](string Customer, Guid ExternalBatchId) input,
            Binder binder,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            var (customer, externalBatchId) = input;

            var query = @$"SELECT * FROM c
WHERE c.customer = '{customer}' AND c.status = '{InvoiceStatus.SentToExternalSystem}' AND c.externalBatchId = '{externalBatchId}'
ORDER BY c._ts";

            var cosmosDBAttribute = new CosmosDBAttribute(CosmosDbKeys.DatabaseName, CosmosDbKeys.InvoicesCollectionName)
            {
                ConnectionStringSetting = SettingNames.CosmosDBConnection,
                PartitionKey = Invoice.PartitionKey,
                SqlQuery = query
            };

            var invoices = (await binder.BindAsync<IEnumerable<Invoice>>(cosmosDBAttribute, cancellationToken)).ToList();
            logger.LogDebug("Completing invoices. InvoiceCount:{InvoiceCount}", invoices.Count);

            foreach (var invoice in invoices)
            {
                invoice.Status = InvoiceStatus.ProcessedByExternalSystem;
                invoice.ExternalBatchCompletedAt = DateTime.UtcNow;
            }

            await SaveInvoices(binder, customer, invoices, cancellationToken);
            logger.LogDebug("Completed invoices. InvoiceCount:{InvoiceCount}", invoices.Count);
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
    }
}
