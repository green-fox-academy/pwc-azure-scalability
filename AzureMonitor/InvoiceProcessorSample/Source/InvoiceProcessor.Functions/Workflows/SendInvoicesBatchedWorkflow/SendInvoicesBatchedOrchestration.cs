using System;
using System.Threading;
using System.Threading.Tasks;
using InvoiceProcessor.Functions.Activities;
using InvoiceProcessor.Functions.Workflows.MonitorExternalBatchStatusWorkflow;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace InvoiceProcessor.Functions.Workflows.SendInvoicesBatchedWorkflow
{
    public static class SendInvoicesBatchedOrchestration
    {
        [FunctionName("SendInvoicesBatchedOrchestration")]
        public static async Task RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            var customer = context.GetInput<string>();
            try
            {
                var (externalBatchId, invoiceIds) = await context.CallActivityAsync<(Guid ExternalBatchId, string[] InvoiceIds)>(nameof(SendInvoicesActivity), customer);
                context.StartNewOrchestration(
                    nameof(MonitorExternalBatchStatusOrchestration),
                    (customer, externalBatchId),
                    $"{nameof(MonitorExternalBatchStatusOrchestration)}.{customer}.{externalBatchId}");
            }
            catch (Exception ex)
            {
                if (!context.IsReplaying)
                {
                    logger.LogError(ex, "Error during sending and monitoring invoices for external batched processing");
                }
            }

            var nextRun = context.CurrentUtcDateTime.AddSeconds(30);
            await context.CreateTimer(nextRun, cancellationToken);

            context.ContinueAsNew(customer);
        }
    }
}
