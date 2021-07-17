using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using InvoiceProcessor.FakeExternalService.Api.Controllers;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace InvoiceProcessor.Functions.Activities
{
    public class GetExternalBatchStatusActivity
    {
        private readonly HttpClient _client;

        public GetExternalBatchStatusActivity(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient();
        }

        [FunctionName(nameof(GetExternalBatchStatusActivity))]
        public async Task<ExternalBatchOperationStatus> Run(
            [ActivityTrigger]Guid externalBatchId,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            var responseMessage = await _client.GetAsync($"https://localhost:6001/ExternalInvoices/GetInvoiceBatchStatus?id={externalBatchId}", cancellationToken);
            responseMessage.EnsureSuccessStatusCode();
            var response = await responseMessage.Content.ReadAsAsync<string>(cancellationToken);
            var externalBatchOperationStatus = Enum.Parse<ExternalBatchOperationStatus>(response);
            return externalBatchOperationStatus;
        }
    }
}
