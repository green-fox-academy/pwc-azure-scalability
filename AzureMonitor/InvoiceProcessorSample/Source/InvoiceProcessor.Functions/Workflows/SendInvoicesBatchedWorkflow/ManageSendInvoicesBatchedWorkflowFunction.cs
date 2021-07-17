using System.Linq;
using System.Threading.Tasks;
using InvoiceProcessor.Functions.Workflows.SendInvoicesBatchedWorkflow;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace InvoiceProcessor.Functions.Workflows.SendInvoicesBatched
{
    public static class ManageSendInvoicesBatchedWorkflowFunction
    {
        [FunctionName(nameof(ManageSendInvoicesBatchedWorkflowFunction))]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest request,
            [DurableClient] IDurableOrchestrationClient orchestrationClient,
            ILogger logger)
        {
            var queryParams = request.GetQueryParameterDictionary();
            queryParams.TryGetValue("Customer", out var customer);
            customer ??= "Customer1";
            queryParams.TryGetValue("ShouldRun", out var shouldRunValue);
            shouldRunValue ??= "true";

            var shouldRun = bool.Parse(shouldRunValue);
            var instanceId = $"{nameof(SendInvoicesBatchedOrchestration)}.{customer}";
            var status = (await orchestrationClient.GetStatusAsync(instanceId))?.RuntimeStatus;
            if (shouldRun)
            {
                await orchestrationClient.StartNewAsync(nameof(SendInvoicesBatchedOrchestration), instanceId, customer);
            }
            else if (IsRunning(status))
            {
                await orchestrationClient.TerminateAsync(instanceId, "Terminated by user");
            }

            return orchestrationClient.CreateCheckStatusResponse(request, instanceId);
        }

        private static bool IsRunning(OrchestrationRuntimeStatus? status)
        {
            if (status == null)
            {
                return false;
            }

            var runningOrPending = new[]
            {
                OrchestrationRuntimeStatus.Running,
                OrchestrationRuntimeStatus.Pending,
                OrchestrationRuntimeStatus.ContinuedAsNew
            };

            return runningOrPending.Contains(status.Value);
        }
    }
}
