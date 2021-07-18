using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceProcessor.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MetricsController : Controller
    {
        private readonly TelemetryClient _telemetryClient;

        public MetricsController(
            TelemetryClient telemetryClient)
        {
            _telemetryClient = telemetryClient;
        }

        [HttpGet]
        [Route("metric")]
        public IActionResult GetMetric()
        {
            _telemetryClient.InstrumentationKey = "73e82620-a343-4db8-bfb5-10558b143919";
            _telemetryClient.TrackEvent(new EventTelemetry() { Name = "Invoice transformed" });

            _telemetryClient.TrackMetric(new MetricTelemetry() { Name = "Invoices in-progress", Sum = 100 });

            return Ok();
        }
    }
}
