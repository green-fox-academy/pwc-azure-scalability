using InvoiceProcessor.Common.Services;
using InvoiceProcessor.Common.Transformations;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(InvoiceProcessor.Functions.Startup))]

namespace InvoiceProcessor.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services
                .AddHttpClient()
                .AddSingleton<IXslTransformationService, XslTransformationService>()
                .AddSingleton<IInvoiceSetSerializer, InvoiceSetSerializer>();
        }
    }
}
