using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InvoiceProcessor.Common;
using InvoiceProcessor.Common.Core;
using InvoiceProcessor.Common.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace InvoiceProcessor.Functions.Activities
{
    public class SaveInvoicesActivity
    {
        private readonly IInvoiceSetSerializer _invoiceSetSerializer;

        public SaveInvoicesActivity(IInvoiceSetSerializer invoiceSetSerializer)
        {
            _invoiceSetSerializer = invoiceSetSerializer;
        }

        [FunctionName(nameof(SaveInvoicesActivity))]
        public async Task Run(
            [ActivityTrigger](Uri BlobUri, string Customer) input,
            IBinder binder,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            var (blobUri, customer) = input;

            var relativeBlobPath = string.Join(string.Empty, blobUri.Segments.Skip(blobUri.Segments.Length - 2));
            var transformedPayloadAttribute = new BlobAttribute(relativeBlobPath, FileAccess.Read)
            {
                Connection = SettingNames.StorageConnection
            };

            var transformedPayloadXml = await binder.BindAsync<string>(transformedPayloadAttribute, cancellationToken);
            var invoiceSet = _invoiceSetSerializer.DeserializeFromXml(transformedPayloadXml);
            logger.LogDebug("InvoiceSet deserialized. InvoiceCount:{InvoiceCount}", invoiceSet.Invoices.Length);

            var cosmosDBAttribute = new CosmosDBAttribute(CosmosDbKeys.DatabaseName, CosmosDbKeys.InvoicesCollectionName)
            {
                CreateIfNotExists = true,
                ConnectionStringSetting = SettingNames.CosmosDBConnection,
                PartitionKey = Invoice.PartitionKey
            };

            var invoiceCollector = await binder.BindAsync<IAsyncCollector<Invoice>>(cosmosDBAttribute, cancellationToken);
            foreach (var invoice in invoiceSet.Invoices)
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
