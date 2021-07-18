using System;
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
        [Route("log")]
        public ActionResult GetLog()
        {
            _logger.LogInformation($"!Method {nameof(GetLog)} has been called.");
            _logger.LogInformation("Method {MethodName} has been called.", nameof(GetLog));

            _logger.LogWarning($"!Method {nameof(GetLog)} has been called.");
            _logger.LogWarning("Method {MethodName} has been called.", nameof(GetLog));

            _logger.LogWarning($"!CPU level: {80}, MemoryUsage: {200}");
            _logger.LogWarning("CPU level: {CPULevel}, MemoryUsage: {MemoryUsage}", 80, 200);

            _logger.LogError(new NotImplementedException(), "Something went wrong!");

            return Ok();
        }
    }
}
