using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using InvoiceProcessor.Functions.Activities;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace InvoiceProcessor.Functions.Orchestrators
{
    public static class ProcessIncomingInvoiceSetOrchestrator
    {
        [FunctionName(nameof(ProcessIncomingInvoiceSetOrchestrator))]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var (blobUri, customer) = context.GetInput<(Uri blobUri, string customer)>();
            if (blobUri == null)
            {
                throw new InvalidOperationException("BlobUri input should not be null");
            }

            if (string.IsNullOrEmpty(customer))
            {
                throw new InvalidOperationException("Customer input should not be null or empty");
            }

            var outputs = new List<string>();
            var tableEntity = new InvoiceSetEntity
            {
                Customer = customer,
                PartitionKey = context.CurrentUtcDateTime.ToString("yyyy-MM"),
                RowKey = Path.GetFileName(blobUri.AbsoluteUri),
                OriginalFileUri = blobUri.ToString()
            };

            // Save info to table storage
            var partitionKey = context.CurrentUtcDateTime.ToString("yyyy-MM");
            var rowKey = Path.GetFileName(blobUri.AbsoluteUri);
            await context.CallActivityAsync(nameof(SaveInvoiceSetEntityActivity), tableEntity);
            //await context.CallActivityAsync(nameof(SaveInvoiceSetEntityActivity), (customer, partitionKey, rowKey, blobUri, (Uri)null));

            // Transform xml
            var transformPayloadFunctionName = $"TransformPayloadFor{customer}Activity";
            var resultUri = await context.CallActivityAsync<Uri>(transformPayloadFunctionName, (blobUri, customer));

            // Save info to table storage
            tableEntity.TransformedFileUri = resultUri.ToString();
            await context.CallActivityAsync(nameof(SaveInvoiceSetEntityActivity), tableEntity);
            //await context.CallActivityAsync(nameof(SaveInvoiceSetEntityActivity), (customer, partitionKey, rowKey, blobUri, resultUri));

            // Save invoices individually to CosmosDB
            await context.CallActivityAsync(nameof(SaveInvoicesActivity), (resultUri, customer));

            return outputs;
        }

    }
}
