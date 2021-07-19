using System;
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
        private readonly Random random = new Random();

        public MetricsController(
            TelemetryClient telemetryClient)
        {
            _telemetryClient = telemetryClient;
        }

        [HttpGet]
        [Route(nameof(TraceMetric))]
        public IActionResult TraceMetric()
        {
            _telemetryClient.TrackEvent(
                new EventTelemetry() { Name = "Invoice transformed" });

            _telemetryClient.TrackMetric(
                new MetricTelemetry()
                {
                    Name = "Invoices in-progress",
                    Sum = random.Next(20, 200),
                    Min = 20,
                    Max = 250,
                });

            return Ok();
        }
    }
}
