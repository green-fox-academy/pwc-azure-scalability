using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

#pragma warning disable CA1801 // Review unused parameters
namespace InvoiceProcessor.Functions.Workflows.Testing
{
    public static class TestLoggingFunction
    {
        [FunctionName("TestLogging")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest request,
            ILogger logger)
        {
            logger.LogInformation("LogInformation");
            logger.LogError("LogError");
            logger.LogDebug("LogDebug");
            logger.LogWarning("LogWarning");
            logger.LogTrace("LogTrace");

            return new OkResult();
        }
    }
}
#pragma warning restore CA1801 // Review unused parameters
