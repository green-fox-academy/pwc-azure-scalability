using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace InvoiceProcessor.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TracingController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly TelemetryClient _telemetryClient;

        public TracingController(ILogger<TracingController> logger, TelemetryClient telemetryClient)
        {
            _logger = logger;
            _telemetryClient = telemetryClient;
        }

        [HttpGet]
        [Route(nameof(Sample1))]
        public async Task<ActionResult> Sample1()
        {
            var requestActivity = new Activity(nameof(Sample1));
            requestActivity.Start(); // You can omit this code.
            var requestOperation = _telemetryClient.StartOperation<RequestTelemetry>(requestActivity);

            await Task.Delay(500);
            var dependencyActivity = new Activity("Some dependency");
            dependencyActivity.SetParentId(requestActivity.Id); // You can omit this code.
            dependencyActivity.Start(); // You can omit this code.
            var dependencyOperation = _telemetryClient.StartOperation<DependencyTelemetry>(dependencyActivity);

            await Task.Delay(500);
            _telemetryClient.StopOperation(dependencyOperation);
            await Task.Delay(500);
            _telemetryClient.StopOperation(requestOperation);

            return Ok();
        }

        [HttpGet]
        [Route(nameof(Sample2))]
        public async Task<ActionResult> Sample2()
        {
            var requestActivity = new Activity(nameof(Sample2));
            var requestOperation = _telemetryClient.StartOperation<RequestTelemetry>(requestActivity);

            await Task.Delay(500);
            var dependencyActivity = new Activity("Some dependency");
            var dependencyOperation = _telemetryClient.StartOperation<DependencyTelemetry>(dependencyActivity);

            await Task.Delay(500);
            _telemetryClient.StopOperation(dependencyOperation);
            await Task.Delay(500);
            _telemetryClient.StopOperation(requestOperation);

            return Ok();
        }
    }
}
