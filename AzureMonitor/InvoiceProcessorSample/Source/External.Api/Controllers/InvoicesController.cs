using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

namespace External.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class InvoicesController : ControllerBase
    {
        private readonly ILogger<InvoicesController> _logger;

        public InvoicesController(ILogger<InvoicesController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public IActionResult PostInvoices()
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        public IActionResult GetInvoiceStatus()
        {
            throw new NotImplementedException();
        }
    }
}
