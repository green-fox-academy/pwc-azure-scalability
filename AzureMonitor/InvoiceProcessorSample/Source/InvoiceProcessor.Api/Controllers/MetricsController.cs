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
        [Route(nameof(TraceMetric))]
        public IActionResult TraceMetric()
        {
            _telemetryClient.TrackEvent(new EventTelemetry() { Name = "Invoice transformed" });

            _telemetryClient.TrackMetric(new MetricTelemetry() { Name = "Invoices in-progress", Sum = 100 });

            return Ok();
        }
    }
}
