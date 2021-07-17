using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using InvoiceProcessor.Common.Core;
using InvoiceProcessor.FakeExternalService.Api.Controllers;
using Newtonsoft.Json;

namespace InvoiceProcessor.Functions.Services
{
    public class FakeExternalServiceClient : IFakeExternalServiceClient
    {
        private readonly HttpClient _client;

        public FakeExternalServiceClient(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient(ClientNames.FakeExternalServiceHttpClient);
        }

        public async Task<ExternalBatchOperationStatus> GetInvoiceBatchStatus(Guid externalBatchId, CancellationToken cancellationToken)
        {
            var responseMessage = await _client.GetAsync($"ExternalInvoices/GetInvoiceBatchStatus?id={externalBatchId}", cancellationToken);
            responseMessage.EnsureSuccessStatusCode();
            var response = await responseMessage.Content.ReadAsAsync<string>(cancellationToken);
            var externalBatchOperationStatus = Enum.Parse<ExternalBatchOperationStatus>(response);
            return externalBatchOperationStatus;
        }

        public async Task<Guid> SendInvoicesToExternalService(ICollection<Invoice> invoices, CancellationToken cancellationToken)
        {
            var json = JsonConvert.SerializeObject(invoices);
            HttpResponseMessage responseMessage;
            using (var memoryStream = new MemoryStream())
            {
                using (var sw = new StreamWriter(memoryStream, leaveOpen: true))
                {
                    await sw.WriteAsync(json);
                }

                memoryStream.Position = 0;
                using var content = new MultipartFormDataContent();
                using var streamContent = new StreamContent(memoryStream)
                {
                    Headers =
                    {
                        ContentLength = memoryStream.Length,
                        ContentType = new MediaTypeHeaderValue("application/json")
                    }
                };
                content.Add(streamContent, "invoices", "invoices.json");
                responseMessage = await _client.PostAsync("ExternalInvoices/ProcessInvoiceBatch", content, cancellationToken);
            }

            responseMessage.EnsureSuccessStatusCode();
            var response = await responseMessage.Content.ReadAsAsync<Guid>(cancellationToken);
            return response;
        }
    }
}
