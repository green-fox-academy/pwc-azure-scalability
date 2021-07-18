using System;
using System.Threading.Tasks;
using InvoiceProcessor.Common;
using InvoiceProcessor.Functions.Orchestrators;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace InvoiceProcessor.Functions.Functions
{
    public class CustomerPayloadUploadedFunction
    {
        private readonly TelemetryClient _telemetryClient;

        public CustomerPayloadUploadedFunction(TelemetryClient telemetryClient)
        {
            _telemetryClient = telemetryClient;
        }

        [FunctionName(nameof(CustomerPayloadUploadedFunction))]
        public async Task Run(
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

            var requestTelemetry = new RequestTelemetry { Name = $"Starting {nameof(ProcessIncomingInvoiceSetOrchestrator)}" };
            if (customerPayloadCloudBlock.Metadata.TryGetValue(MetadataKeys.TelemetryParentId, out var telemetryParentId) &&
                customerPayloadCloudBlock.Metadata.TryGetValue(MetadataKeys.TelemetryRootId, out var telemetryRootId))
            {
                requestTelemetry.Context.Operation.Id = telemetryRootId;
                requestTelemetry.Context.Operation.ParentId = telemetryParentId;
            }

            var operation = _telemetryClient.StartOperation(requestTelemetry);
            try
            {
                var customer = customerPayloadCloudBlock.Metadata[MetadataKeys.Customer];
                logger.LogDebug("ProcessReceivedInvoices triggered for blob. Name:{Name}, Size:{Size}, Customer:{Customer} bytes", name, customerPayloadCloudBlock.Properties.Length, customer);

                var instanceId = await orchestrationClient.StartNewAsync(nameof(ProcessIncomingInvoiceSetOrchestrator), name, (customerPayloadCloudBlock.Uri, customer));
                logger.LogDebug("{OrchestratorFunction} started. InstanceId:{InstanceId}", nameof(ProcessIncomingInvoiceSetOrchestrator), instanceId);
            }
            catch (Exception ex)
            {
                _telemetryClient.TrackException(ex);
                operation.Telemetry.Success = false;
                throw;
            }
            finally
            {
                _telemetryClient.StopOperation(operation);
            }
        }
    }
}
