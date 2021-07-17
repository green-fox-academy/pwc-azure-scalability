using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Converters;

namespace InvoiceProcessor.FakeExternalService.Api.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class ExternalInvoicesController : ControllerBase
    {
        private readonly TableServiceClient _tableServiceClient;

        public ExternalInvoicesController(TableServiceClient tableServiceClient)
        {
            _tableServiceClient = tableServiceClient;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        [Route(nameof(ProcessInvoiceBatch))]
        public async Task<ActionResult<Guid>> ProcessInvoiceBatch([Required] IFormFile invoices, CancellationToken cancellationToken)
        {
            if (invoices is null)
            {
                throw new ArgumentNullException(nameof(invoices));
            }

            var id = Guid.NewGuid();
            var entity = new OperationState()
            {
                PartitionKey = "OperationStates",
                RowKey = id.ToString(),
                Status = ExternalBatchOperationStatus.Processing,
            };

            var tableClient = _tableServiceClient.GetTableClient("FakeExternalServiceData");
            await tableClient.CreateIfNotExistsAsync(cancellationToken);
            await tableClient.AddEntityAsync(entity, cancellationToken);

            return id;
        }

        [HttpGet]
        [Route(nameof(GetInvoiceBatchStatus))]
        public async Task<ActionResult<ExternalBatchOperationStatus>> GetInvoiceBatchStatus(Guid id, CancellationToken cancellationToken)
        {
            var tableClient = _tableServiceClient.GetTableClient("FakeExternalServiceData");
            await tableClient.CreateIfNotExistsAsync(cancellationToken);

            var operationState = (await tableClient.GetEntityAsync<OperationState>("OperationStates", id.ToString(), cancellationToken: cancellationToken)).Value;
            if (operationState.Status == ExternalBatchOperationStatus.Processing && (DateTimeOffset.UtcNow - operationState.Timestamp.Value) > TimeSpan.FromSeconds(5))
            {
                operationState.Status = ExternalBatchOperationStatus.Completed;
                await tableClient.UpdateEntityAsync(operationState, operationState.ETag, cancellationToken: cancellationToken);
            }

            return operationState.Status;
        }

        private class OperationState : ITableEntity
        {
            public string PartitionKey { get; set; }
            public string RowKey { get; set; }
            public DateTimeOffset? Timestamp { get; set; }
            public ETag ETag { get; set; }

            [JsonConverter(typeof(StringEnumConverter))]
            public ExternalBatchOperationStatus Status { get; set; }
        }
    }
}
