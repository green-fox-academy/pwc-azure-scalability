using System;
using System.Threading.Tasks;
using InvoiceProcessor.Common;
using InvoiceProcessor.Functions.Orchestrators;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace InvoiceProcessor.Functions.Functions
{
    public static class CustomerPayloadUploadedFunction
    {
        [FunctionName(nameof(CustomerPayloadUploadedFunction))]
        public static async Task Run(
            [BlobTrigger("customer-payloads/{name}", Connection = SettingNames.StorageConnection)]
            CloudBlockBlob customerPayloadCloudBlock,
            string name,
            [DurableClient] IDurableOrchestrationClient orchestrationClient,
            ILogger logger)
        {
            if (customerPayloadCloudBlock is null)
            {
                throw new ArgumentNullException(nameof(customerPayloadCloudBlock));
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));
            }

            if (orchestrationClient is null)
            {
                throw new ArgumentNullException(nameof(orchestrationClient));
            }

            if (logger is null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            customerPayloadCloudBlock.Metadata.TryGetValue(MetadataKeys.Customer, out var customer);
            logger.LogDebug("ProcessReceivedInvoices triggered for blob. Name:{Name}, Size:{Size}, Customer:{Customer} bytes", name, customerPayloadCloudBlock.Properties.Length, customer);

            var instanceId = await orchestrationClient.StartNewAsync(nameof(ProcessIncomingInvoiceSetOrchestrator), name, (customerPayloadCloudBlock.Uri, customer));
            logger.LogDebug("{OrchestratorFunction} started. InstanceId:{InstanceId}", nameof(ProcessIncomingInvoiceSetOrchestrator), instanceId);
        }
    }
}
