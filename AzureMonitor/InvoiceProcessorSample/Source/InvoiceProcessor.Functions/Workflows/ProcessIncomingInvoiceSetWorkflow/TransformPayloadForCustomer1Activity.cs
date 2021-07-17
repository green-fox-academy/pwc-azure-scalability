using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using InvoiceProcessor.Common;
using InvoiceProcessor.Common.Transformations;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace InvoiceProcessor.Functions.Activities
{
    public class TransformPayloadForCustomer1Activity
    {
        private const string XmlContentType = "text/xml";
        private readonly IXslTransformationService _xslTransformationService;

        public TransformPayloadForCustomer1Activity(IXslTransformationService xslTransformationService)
        {
            _xslTransformationService = xslTransformationService;
        }

        [FunctionName(nameof(TransformPayloadForCustomer1Activity))]
        public async Task<Uri> Run(
            [ActivityTrigger](Uri BlobPath, string Customer) input,
            IBinder binder,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            var (blobPath, customer) = input;
            var fileName = Path.GetFileName(blobPath.AbsolutePath);
            var customerPayloadAttribute = new BlobAttribute($"{BlobContainerNames.CustomerPayloadsContainer}/{fileName}", FileAccess.Read)
            {
                Connection = SettingNames.StorageConnection
            };

            var transformedPayloadAttribute = new BlobAttribute($"{BlobContainerNames.TransformedPayloadsContainer}/{fileName}", FileAccess.ReadWrite)
            {
                Connection = SettingNames.StorageConnection
            };

            var customerPayloadCloudBlock = await binder.BindAsync<CloudBlockBlob>(customerPayloadAttribute, cancellationToken);
            var transformedPayloadCloudBlock = await binder.BindAsync<CloudBlockBlob>(transformedPayloadAttribute, cancellationToken);

            string invoiceSetXml;
            using (var targetMemoryStream = new MemoryStream())
            {
                using (var downloadStream = new MemoryStream())
                {
                    await customerPayloadCloudBlock.DownloadToStreamAsync(downloadStream, cancellationToken: cancellationToken);
                    downloadStream.Position = 0;

                    _xslTransformationService.TransformClient1XmlToCoreXml(downloadStream, targetMemoryStream);
                }

                logger.LogDebug("Uploading transformed xml to blob storage");
                transformedPayloadCloudBlock.Metadata[MetadataKeys.Customer] = customer;
                transformedPayloadCloudBlock.Properties.ContentType = XmlContentType;
                await transformedPayloadCloudBlock.UploadFromStreamAsync(targetMemoryStream, cancellationToken: cancellationToken);

                targetMemoryStream.Position = 0;
                using var streamReader = new StreamReader(targetMemoryStream);
                invoiceSetXml = await streamReader.ReadToEndAsync();
            }

            return transformedPayloadCloudBlock.Uri;
        }
    }
}
