using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using InvoiceProcessor.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace InvoiceProcessor.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class InvoicesController : ControllerBase
    {
        private readonly ILogger<InvoicesController> _logger;
        private readonly IStorageService _storageService;

        public InvoicesController(ILogger<InvoicesController> logger, IStorageService storageService)
        {
            _logger = logger;
            _storageService = storageService;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<Guid>> ProcessInvoices([Required] IFormFile importFile, CancellationToken cancellationToken)
        {
            if (importFile is null)
            {
                throw new ArgumentNullException(nameof(importFile));
            }

            if (!string.Equals(importFile.ContentType, "text/xml", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest($"Invalid ContentType. ContentType should be 'text/xml'. ContentType:{importFile.ContentType}");
            }

            if (!string.Equals(Path.GetExtension(importFile.FileName), ".xml", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest($"Invalid FileName. File should be xml. FileName:{importFile.FileName}");
            }

            var id = Guid.NewGuid();
            _logger.LogDebug("Processing invoices. InvoiceId:{InvoiceId}", id);
            using (_logger.BeginScope("{InvoiceId}", id))
            {
                await _storageService.UploadAsync(importFile, "customer-payloads", $"{id}.xml", cancellationToken);
                return id;
            }
        }
    }
}
