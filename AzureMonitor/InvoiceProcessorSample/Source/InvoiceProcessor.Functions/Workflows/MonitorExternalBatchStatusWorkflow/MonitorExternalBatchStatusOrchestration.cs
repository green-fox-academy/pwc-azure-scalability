using System;
using System.Threading;
using System.Threading.Tasks;
using InvoiceProcessor.FakeExternalService.Api.Controllers;
using InvoiceProcessor.Functions.Activities;
using InvoiceProcessor.Functions.Workflows.SendInvoicesBatchedWorkflow;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace InvoiceProcessor.Functions.Workflows.MonitorExternalBatchStatusWorkflow
{
    public static class MonitorExternalBatchStatusOrchestration
    {
        [FunctionName(nameof(MonitorExternalBatchStatusOrchestration))]
        public static async Task RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var (customer, externalBatchId) = context.GetInput<(string Customer, Guid ExternalBatchId)>();
            var pollingInterval = TimeSpan.FromSeconds(60);
            var expiryTime = context.CurrentUtcDateTime.AddHours(1);

            while (true)
            {
                var batchStatus = await context.CallActivityAsync<ExternalBatchOperationStatus>(nameof(GetExternalBatchStatusActivity), externalBatchId);
                switch (batchStatus)
                {
                    case ExternalBatchOperationStatus.Processing:
                        var nextCheck = context.CurrentUtcDateTime.Add(pollingInterval);
                        await context.CreateTimer(nextCheck, CancellationToken.None);
                        if (context.CurrentUtcDateTime > expiryTime)
                        {
                            throw new TimeoutException($"External batch processing did not finish in time. ExternalBatchId:{externalBatchId}");
                        }

                        break;
                    case ExternalBatchOperationStatus.Completed:
                        await context.CallActivityAsync(nameof(CompleteInvoicesActivity), (customer, externalBatchId));
                        return;
                    default:
                        throw new InvalidOperationException($"ExternalBatchOperationStatus has unexpected value. ExternalBatchOperationStatus:{batchStatus}");
                }
            }
        }
    }
}
