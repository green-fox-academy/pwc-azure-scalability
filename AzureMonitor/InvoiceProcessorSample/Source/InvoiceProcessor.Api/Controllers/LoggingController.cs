using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace InvoiceProcessor.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoggingController : ControllerBase
    {
        private readonly ILogger<LoggingController> _logger;

        public LoggingController(
            ILogger<LoggingController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route(nameof(WriteLogs))]
        public ActionResult WriteLogs()
        {
            _logger.LogDebug($"!Method {nameof(WriteLogs)} has been called.");
            _logger.LogDebug("Method {MethodName} has been called.", nameof(WriteLogs));

            _logger.LogDebug($"!Method {nameof(WriteLogs)} has been called.");
            _logger.LogDebug("Method {MethodName} has been called.", nameof(WriteLogs));

            _logger.LogDebug($"!CPU level: {80}, MemoryUsage: {200}");
            _logger.LogDebug("CPU level: {CPULevel}, MemoryUsage: {MemoryUsage}", 80, 200);

            using (_logger.BeginScope(
                new Dictionary<string, object>
                {
                    ["OperationName"] = "Complex Operation",
                    ["CorrelationId"] = Guid.NewGuid()
                }))
            {
                _logger.LogDebug("Operation started.");
                _logger.LogDebug("Operation completed.");
            }

            _logger.LogError(new InvalidOperationException("Something is in a bad state"), "Something went wrong!");

            return Ok();
        }
    }
}
