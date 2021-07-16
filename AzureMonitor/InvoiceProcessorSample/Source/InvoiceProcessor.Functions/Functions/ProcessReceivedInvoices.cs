using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace InvoiceProcessor.Functions.Functions
{
    public static class ProcessReceivedInvoices
    {
        [FunctionName("ProcessReceivedInvoices")]
        public static void Run([BlobTrigger("customer-payloads/{name}", Connection = "AzureWebJobsStorage")]Stream blob, string name, ILogger log)
        {
            if (blob is null)
            {
                throw new ArgumentNullException(nameof(blob));
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));
            }

            if (log is null)
            {
                throw new ArgumentNullException(nameof(log));
            }

            log.LogInformation("C# Blob trigger function Processed blob. Name:{Name}, Size:{Size} bytes", name, blob.Length);
        }
    }
}
