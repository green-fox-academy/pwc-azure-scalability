using System;
using System.Threading;
using System.Threading.Tasks;
using InvoiceProcessor.FakeExternalService.Api.Controllers;
using InvoiceProcessor.Functions.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace InvoiceProcessor.Functions.Activities
{
    public class GetExternalBatchStatusActivity
    {
        private readonly IFakeExternalServiceClient _fakeExternalServiceClient;

        public GetExternalBatchStatusActivity(IFakeExternalServiceClient fakeExternalServiceClient)
        {
            _fakeExternalServiceClient = fakeExternalServiceClient;
        }

        [FunctionName(nameof(GetExternalBatchStatusActivity))]
        public async Task<ExternalBatchOperationStatus> Run(
            [ActivityTrigger]Guid externalBatchId,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            return await _fakeExternalServiceClient.GetInvoiceBatchStatus(externalBatchId, cancellationToken);
        }


    }
}
