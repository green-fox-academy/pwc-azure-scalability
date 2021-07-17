using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using InvoiceProcessor.Common.Core;
using InvoiceProcessor.FakeExternalService.Api.Controllers;

namespace InvoiceProcessor.Functions.Services
{
    public interface IFakeExternalServiceClient
    {
        Task<ExternalBatchOperationStatus> GetInvoiceBatchStatus(Guid externalBatchId, CancellationToken cancellationToken);
        Task<Guid> SendInvoicesToExternalService(ICollection<Invoice> invoices, CancellationToken cancellationToken);
    }
}